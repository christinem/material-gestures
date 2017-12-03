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
    public static extern IntPtr CreateMyObjectInstance(int i);
    [DllImport("C:\\Users\\cmurad\\Documents\\GAUSS\\build\\x64\\Release\\GAUSSDLL.dll")]
    public static extern int increment(IntPtr gaussObject, bool grabbed, int index, double x, double y, double z);
    [DllImport("C:\\Users\\cmurad\\Documents\\GAUSS\\build\\x64\\Release\\GAUSSDLL.dll")]
    public static extern void get_mesh_sizes(IntPtr gaussObject, ref int n_face_indices, ref int n_vertices);
    [DllImport("C:\\Users\\cmurad\\Documents\\GAUSS\\build\\x64\\Release\\GAUSSDLL.dll")]
    public static extern void get_updated_mesh(IntPtr gaussObject, double[] verts_flat, int[] face_indices_flat);


    // My Parameters
    double minSelectionDist = 0.15; // Chosen through experiment

    // Script parameters (set by Unity)
    public GameObject controllerOne;
    public GameObject controllerTwo;

    // Controller state to simplify controller handling
    private struct ControllerState
    {
        public int id; // Integer id to identify the controller
        public bool isGrabbing; // Is the trigger currently depressed and was it close enough to a vert when it was pulled? 
        public int grabbedVertIndex; // If isGrabbing, then is index to the grabbed vertex
        public Vector3 pos; // Controller position 

        public GameObject gameObject;
        public ControllerScript script;
        public SteamVR_Controller.Device device;
    }

    private ControllerState[] controllerStates;

    public Mesh mesh;

    bool meshThreadRunning;
    bool meshThreadFinished;
    Thread meshThread;
    Vector3[] threadMeshVertices;
    IntPtr gaussObject;
    double[] verts_flat;
    int[] face_indices_flat;

    void meshThreadWork()
    {
        while(!meshThreadFinished)
        {
            if(meshThreadRunning)
            {
                for(int i = 0; i < 2; i++)
                {
                    
                }

                ControllerState state = controllerStates[1];
                Vector3 localControllerPos = state.pos;
                increment(gaussObject, state.isGrabbing, state.grabbedVertIndex, (double)localControllerPos.x, (double)localControllerPos.y, (double)localControllerPos.z);
                get_updated_mesh(gaussObject, verts_flat, face_indices_flat);

                //Now update the mesh with the new one
                for (int i = 0; i < threadMeshVertices.Length; i++)
                {
                    threadMeshVertices[i] = new Vector3((float)verts_flat[i * 3], (float)verts_flat[i * 3 + 1], (float)verts_flat[i * 3 + 2]);
                }

                meshThreadRunning = false;
                //Thread.Sleep(Timeout.Infinite);
                meshThread.Suspend();
            }
        }
    }
    void OnDisable()
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

    // Use this for initialization
    void Start () {
        // Get the mesh this script has been assigned to.
        mesh = ((MeshFilter)gameObject.GetComponent("MeshFilter")).mesh;



        gaussObject = CreateMyObjectInstance(0);
        int n_face_indices = 0;
        int flat_verts_len = 0;
        get_mesh_sizes(gaussObject, ref n_face_indices, ref flat_verts_len);
        verts_flat = new double[flat_verts_len];
        face_indices_flat = new int[n_face_indices];
        get_updated_mesh(gaussObject, verts_flat, face_indices_flat);

        //Now update the mesh with the new one
        Vector3[] newVerts = new Vector3[flat_verts_len / 3];
        for(int i = 0; i < newVerts.Length; i++)
        {
            newVerts[i] = new Vector3((float)verts_flat[i * 3], (float)verts_flat[i * 3 + 1], (float)verts_flat[i * 3 + 2]);
        }

        for (int i = 0; i < face_indices_flat.Length/3; i++)
        {
            int i0 = face_indices_flat[i * 3];
            int i2 = face_indices_flat[i * 3+2];
            face_indices_flat[i * 3] = i2;
            face_indices_flat[i * 3 + 2] = i0;
        }


        mesh.SetTriangles(face_indices_flat, 0);
        mesh.RecalculateNormals();
        
        //mesh.SetIndices(face_indices_flat, MeshTopology.Triangles, 0);
        mesh.vertices = newVerts;
        
        mesh.RecalculateBounds();
        threadMeshVertices = mesh.vertices;

        meshThreadFinished = false;
        meshThreadRunning = false;
        meshThread = new Thread(meshThreadWork);
        meshThread.Start();
        

        // Put the Unity controllers into an array to simplify the rest of the logic
        // TODO put this into a proper class constructor
        controllerStates = new ControllerState[2];
        controllerStates[0].gameObject = controllerOne;
        controllerStates[0].id = 0;
        controllerStates[0].isGrabbing = false;
        controllerStates[0].grabbedVertIndex = -1;
        controllerStates[1].gameObject = controllerTwo;
        controllerStates[1].id = 1;
        controllerStates[1].isGrabbing = false;
        controllerStates[1].grabbedVertIndex = -1;
    }

    void updateControllers()
    {
        for(int i = 0; i < controllerStates.Length; i++)
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
                Debug.Log(mesh.vertices.Length);
                int nearestVertIndex = nearestVertexTo(pos);
                Vector3 nearestVertPos = transform.TransformPoint(mesh.vertices[nearestVertIndex]);
                double dist = (pos - nearestVertPos).magnitude;

                if(dist < minSelectionDist)
                {
                    Debug.Log("Grabbed vert " + nearestVertIndex);
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
	void Update () {
        // updateControllers();
        //updateVertices(controllerStates);

        if(!meshThreadRunning)
        {
            mesh.vertices = threadMeshVertices; // This is necessary to trigger an update for some reason >_<
            mesh.RecalculateBounds(); // Todo Necessary?
            mesh.RecalculateNormals();

            updateControllers();
            // start a new mesh update call
            meshThreadRunning = true;
            meshThread.Resume();
        } else if (meshThreadRunning)
        {
            // pass
        }

    }

    private void updateVertices(ControllerState[] controllerStates)
    {
        // Now using threads
        // We need to know whether the user is 'grabbing', which vertex the user is grabbing, and where the user's controller is located.
        //Vector3[] originalVertices = meshVertices; // original vertices of this mesh

      /*  Vector3[] origVerts = mesh.vertices;
        Vector3 offset = new Vector3(0.0f,0.01f,0.0f);
       // Debug.Log("Mesh has " + mesh.vertices.Length + "verts: i = " + increment(1));

        if(mesh.vertices.Length == 515)
        {
            Thread.Sleep(500);
        }
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            origVerts[i] += offset;
        }

        mesh.vertices = origVerts; // This is necessary to trigger an update for some reason >_<
        mesh.RecalculateBounds(); // Todo Necessary?
        return; // Just stop after first controller*/
        /*
        foreach (ControllerState state in controllerStates)
        {
            if (state.isGrabbing)
            {
                Vector3[] origVerts = mesh.vertices;

                Vector3 meshPoint = transform.TransformPoint(mesh.vertices[state.grabbedVertIndex]);
                Vector3 offset = state.pos - meshPoint;
 
                for (int i = 0; i < mesh.vertices.Length; i++)
                {
                    origVerts[i] += offset;
                }

                mesh.vertices = origVerts; // This is necessary to trigger an update for some reason >_<
                mesh.RecalculateBounds(); // Todo Necessary?
                return; // Just stop after first controller
            }
        }*/
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
			if (distSqr < minDistanceSqr)
			{
				minDistanceSqr = distSqr;
                nearestVertIndex = i;
			}
		}

        return nearestVertIndex;
	}

}

