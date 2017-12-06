using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;


public class MeshDeformation : MonoBehaviour {
    // DLL Imports
    [DllImport("C:\\Users\\cmurad\\Documents\\GAUSS\\build\\x64\\Release\\GAUSSDLL.dll")]
    public static extern IntPtr CreateMyObjectInstance(double grabbing_spring_stiffness, double spring_root_mass, double density, double youngsModulus, double poissonsRatio, double timestep, string mesh_path);
    [DllImport("C:\\Users\\cmurad\\Documents\\GAUSS\\build\\x64\\Release\\GAUSSDLL.dll")]
    public static extern int increment(IntPtr gaussObject, bool grabbed, int index, double x, double y, double z);
    [DllImport("C:\\Users\\cmurad\\Documents\\GAUSS\\build\\x64\\Release\\GAUSSDLL.dll")]
    public static extern void get_mesh_sizes(IntPtr gaussObject, ref int n_face_indices, ref int n_vertices);
    [DllImport("C:\\Users\\cmurad\\Documents\\GAUSS\\build\\x64\\Release\\GAUSSDLL.dll")]
    public static extern void get_updated_mesh(IntPtr gaussObject, double[] verts_flat, int[] face_indices_flat);

    //Instances of all MeshDeformations. Used for logging purposes
    public static List<MeshDeformation> instances = new List<MeshDeformation>();

    // My Parameters
    double minSelectionDist = 0.15; // Chosen through experiment
    double grabbing_spring_stiffness = 200.0;
    double spring_root_mass = 100000000.0;
    double timestep = 0.05;
    double density = 1000.0;
    double youngsModulus = 1000000.0;
    double poissonsRatio = 0.35;
     

    // Script parameters (set by Unity)
    public GameObject controllerOne;
    //public GameObject controllerTwo;

    // Controller state to simplify controller handling
    public struct ControllerState
    {
        public int id; // Integer id to identify the controller
        public bool isGrabbing; // Is the trigger currently depressed and was it close enough to a vert when it was pulled? 
        public int grabbedVertIndex; // If isGrabbing, then is index to the grabbed vertex
        public Vector3 pos; // Controller position 

        public GameObject gameObject;
        public ControllerScript script;
        public SteamVR_Controller.Device device;
    }

    public ControllerState[] controllerStates;

    public Mesh mesh;

    public static bool experiment_running = false;

    bool meshThreadRunning;
    public bool meshThreadFinished;
    public bool reset_object_pending;
    public long reset_timestamp = 0;
    Thread meshThread;
    Vector3[] threadMeshVertices;
    IntPtr gaussObject;
    double[] verts_flat;
    int[] face_indices_flat;

    public string mesh_file_location = "";//passed into readMesh to read in the correct file for this instance

    
    void meshThreadWork()
    {
        while(!meshThreadFinished)
        {
            if(meshThreadRunning)
            {
                // Currently only have interaction for first controller implemented
                ControllerState state = controllerStates[0];
                Vector3 localControllerPos = state.pos; // This is the controller position in object space (transformed into gauss coordinate frame)

                // This call does the actual simulation timestep
                increment(gaussObject, state.isGrabbing, state.grabbedVertIndex, (double)localControllerPos.x, (double)localControllerPos.y, (double)localControllerPos.z);

                // Now update the mesh with the new one
                get_updated_mesh(gaussObject, verts_flat, face_indices_flat);
                for (int i = 0; i < threadMeshVertices.Length; i++)
                {
                    threadMeshVertices[i] = new Vector3((float)verts_flat[i * 3], (float)verts_flat[i * 3 + 1], (float)verts_flat[i * 3 + 2]);
                }

                meshThreadRunning = false;
                meshThread.Suspend();
            }
        }
    }

    //Separated it out into its own function
    void KillThread()
    {
        // If the thread is still running, we should shut it down,
        // otherwise it can prevent the game from exiting correctly.
        if (!meshThreadFinished)
        {
            // This forces the while loop in the ThreadedWork function to abort.
            meshThreadFinished = true;

            // This waits until the thread exits,
            // ensuring any cleanup we do after this is safe.
            meshThread.Resume();
            meshThread.Join();
        }

        // Thread is guaranteed no longer running. Do other cleanup tasks.
    }
    
    // Gets called when you quit unity. Need to kill the simulation thread or else it hangs
    void OnDisable()
    {
        KillThread();
    }

    //Added this to reset the object when user has clicked the menu button.
    public void ObjectReset() {
        //KillThread(); //Separated

        //----------------------
        //Straight copy pasted from start() function.
        //without recreating any global variables.
        Debug.Log("Loading " + mesh_file_location);
        gaussObject = CreateMyObjectInstance(grabbing_spring_stiffness, spring_root_mass, density, youngsModulus, poissonsRatio, timestep, mesh_file_location);
        int n_face_indices = 0;
        int flat_verts_len = 0;
        get_mesh_sizes(gaussObject, ref n_face_indices, ref flat_verts_len);
        verts_flat = new double[flat_verts_len];
        face_indices_flat = new int[n_face_indices];
        get_updated_mesh(gaussObject, verts_flat, face_indices_flat);
        //Now update the unity mesh with the new one from gauss
        Vector3[] newVerts = new Vector3[flat_verts_len / 3];
        for (int i = 0; i < newVerts.Length; i++)
        {
            newVerts[i] = new Vector3((float)verts_flat[i * 3], (float)verts_flat[i * 3 + 1], (float)verts_flat[i * 3 + 2]);
        }
        // Need to change order of indices for triangle normals to be computed correctly
        for (int i = 0; i < face_indices_flat.Length / 3; i++)
        {
            int i0 = face_indices_flat[i * 3];
            int i2 = face_indices_flat[i * 3 + 2];
            face_indices_flat[i * 3] = i2;
            face_indices_flat[i * 3 + 2] = i0;
        }
        mesh.SetTriangles(new int[0], 0);
        mesh.vertices = newVerts;
        mesh.SetTriangles(face_indices_flat, 0);
        //mesh.vertices = new Vector3[0];

        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        // Set up the simulation thread
        threadMeshVertices = mesh.vertices;
        // meshThreadFinished = false;
        //meshThreadRunning = false;
        //meshThread = new Thread(meshThreadWork);
        //meshThread.Start();

        // Put the Unity controllers into an array to simplify the rest of the logic
        // TODO put this into a proper class constructor
        controllerStates[0].gameObject = controllerOne;
        controllerStates[0].id = 0;
        controllerStates[0].isGrabbing = false;
        controllerStates[0].grabbedVertIndex = -1;
        //controllerStates[1].gameObject = controllerTwo;
        //controllerStates[1].id = 1;
        //controllerStates[1].isGrabbing = false;
        //controllerStates[1].grabbedVertIndex = -1;
        //----------------------
    }
    // Use this for initialization
    public void myStart () {
        
        // Get the mesh this script has been assigned to. Assigning to mesh.vertices triggers a mesh update
        mesh = ((MeshFilter)gameObject.GetComponent("MeshFilter")).mesh;


        // Set up the simulation
        // This is where you set the path to the bar
        Debug.Log("Loading " + mesh_file_location);
        gaussObject = CreateMyObjectInstance(grabbing_spring_stiffness, spring_root_mass, density, youngsModulus, poissonsRatio, timestep, mesh_file_location); 

        int n_face_indices = 0;
        int flat_verts_len = 0;
        get_mesh_sizes(gaussObject, ref n_face_indices, ref flat_verts_len);

        verts_flat = new double[flat_verts_len];
        face_indices_flat = new int[n_face_indices];
        get_updated_mesh(gaussObject, verts_flat, face_indices_flat);

        //Now update the unity mesh with the new one from gauss
        Vector3[] newVerts = new Vector3[flat_verts_len / 3];
        for(int i = 0; i < newVerts.Length; i++)
        {
            newVerts[i] = new Vector3((float)verts_flat[i * 3], (float)verts_flat[i * 3 + 1], (float)verts_flat[i * 3 + 2]);
        }

        // Need to change order of indices for triangle normals to be computed correctly
        for (int i = 0; i < face_indices_flat.Length/3; i++)
        {
            int i0 = face_indices_flat[i * 3];
            int i2 = face_indices_flat[i * 3+2];
            face_indices_flat[i * 3] = i2;
            face_indices_flat[i * 3 + 2] = i0;
        }
        mesh.SetTriangles(new int [0], 0);
        mesh.vertices = newVerts;
        mesh.SetTriangles(face_indices_flat, 0);
             
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // Set up the simulation thread
        threadMeshVertices = mesh.vertices;
        meshThreadFinished = false;
        meshThreadRunning = false;
        reset_object_pending = false;
        meshThread = new Thread(meshThreadWork);
        meshThread.Start();
        

        // Put the Unity controllers into an array to simplify the rest of the logic
        // TODO put this into a proper class constructor
        controllerStates = new ControllerState[2];
        controllerStates[0].gameObject = controllerOne;
        controllerStates[0].id = 0;
        controllerStates[0].isGrabbing = false;
        controllerStates[0].grabbedVertIndex = -1;
        //controllerStates[1].gameObject = controllerTwo;
        //controllerStates[1].id = 1;
        //controllerStates[1].isGrabbing = false;
        //controllerStates[1].grabbedVertIndex = -1;

        instances.Add(this);
    }


    // Updates the state of each controller
    void updateControllers()
    {
        for(int i = 0; i < 1; i++)
        {
            ControllerScript script = controllerStates[i].gameObject.GetComponent<ControllerScript>();
            SteamVR_Controller.Device device = script.Controller;
            if(device == null)
            {
                continue;
            }

            Vector3 pos = controllerStates[i].gameObject.transform.position;
            int grabbedVertIndex = controllerStates[i].grabbedVertIndex;
            bool isGrabbing = controllerStates[i].isGrabbing;

            // Counterintuitively.. getHairTriggerDown() only returns true on the first frame that the trigger is pulled.
            // This is good though because that's actually what we want.
            if (device.GetHairTriggerDown())
            {
                int nearestVertIndex = nearestVertexTo(pos);
                Vector3 nearestVertPos = transform.TransformPoint(mesh.vertices[nearestVertIndex]);
                double dist = (pos - nearestVertPos).magnitude;

                if(dist < minSelectionDist)
                {
                    isGrabbing = true;
                    grabbedVertIndex = nearestVertIndex;
                }
            }

            if (device.GetHairTriggerUp())
            {
                isGrabbing = false;
            }


            controllerStates[i].isGrabbing = isGrabbing;
            controllerStates[i].pos = transform.InverseTransformPoint(pos);
            controllerStates[i].grabbedVertIndex = grabbedVertIndex;
            controllerStates[i].script = script;
            controllerStates[i].device = device;

            
        }
      
    }

    // Update is called once per frame
    void Update() {
        if (experiment_running)
        {
            updateControllers();
            // We need to wait for a simulation timestep to finish before updating the mesh
            if (!meshThreadRunning)
            {
                if (reset_object_pending)
                {
                    long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    if (now - reset_timestamp > 500L)
                    {
                        ObjectReset();
                        reset_object_pending = false;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                 
                    if (float.IsNaN(threadMeshVertices[0].x))
                    {
                        reset_object_pending = true;
                        return;
                    }

                    mesh.vertices = threadMeshVertices; // This assignment triggers the update. It's not a normal variable.
                    mesh.RecalculateBounds();
                    mesh.RecalculateNormals();

                }

                // start a new mesh update call
                meshThreadRunning = true;
                meshThread.Resume();
            }
        }

    }
 
	public int nearestVertexTo(Vector3 point)
	{
        // Returns the index of the closest vert to point.
		// convert point to local space
		Vector3 localPoint = transform.InverseTransformPoint(point);

		Mesh mesh = GetComponent<MeshFilter>().mesh;
		float minDistanceSqr = Mathf.Infinity;
		int nearestVertIndex = 0;
        
		// scan all vertices to find nearest
		for (int i = 0; i < mesh.vertices.Length; i++)
		{
			Vector3 diff = localPoint - mesh.vertices[i];
			float distSqr = diff.sqrMagnitude;
            //if (distSqr > 5) //Easrly stopping
            //{
            //    return i; // Will always be too far away to grab
            //}

			if (distSqr < minDistanceSqr)
			{
				minDistanceSqr = distSqr;
                nearestVertIndex = i;
			}
		}

        return nearestVertIndex;
	}


}

