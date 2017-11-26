using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformation : MonoBehaviour {
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
	private Vector3[] meshVertices;

    // Use this for initialization
    void Start () {
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

        // Get the mesh this script has been assigned to.
        mesh = ((MeshFilter)gameObject.GetComponent("MeshFilter")).mesh;
		meshVertices = mesh.vertices;
	}

    void updateControllers()
    {
        for(int i = 0; i < controllerStates.Length; i++)
        {
            ControllerScript script = controllerStates[i].gameObject.GetComponent<ControllerScript>();
            SteamVR_Controller.Device device = script.Controller;
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
            controllerStates[i].pos = pos;
            controllerStates[i].grabbedVertIndex = grabbedVertIndex;
            controllerStates[i].script = script;
            controllerStates[i].device = device;
        }
    }

	// Update is called once per frame
	void Update () {
        updateControllers();
        updateVertices(controllerStates);
    }

    private void updateVertices(ControllerState[] controllerStates)
    {
        // We need to know whether the user is 'grabbing', which vertex the user is grabbing, and where the user's controller is located.
        //Vector3[] originalVertices = meshVertices; // original vertices of this mesh

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
			if (distSqr < minDistanceSqr)
			{
				minDistanceSqr = distSqr;
                nearestVertIndex = i;
			}
		}

        return nearestVertIndex;
	}

}

