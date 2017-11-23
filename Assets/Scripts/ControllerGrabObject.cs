/* Adjusted from source: https://www.raywenderlich.com/149239/htc-vive-tutorial-unity */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerGrabObject : MonoBehaviour {

	private SteamVR_TrackedObject trackedObj;
	private GameObject collidingObject; 
	private GameObject objectInHand; 
	private Vector3 lastPosition; // the controller's last position
	private Vector3 nearestVertex;
	private bool holding;

	private SteamVR_Controller.Device Controller
	{
		get { return SteamVR_Controller.Input((int)trackedObj.index); }
	}

	void Awake()
	{
		trackedObj = GetComponent<SteamVR_TrackedObject>();
		lastPosition = Vector3.zero;
	}

    // Update is called once per frame
    void Update()
    {
        if (Controller.GetHairTriggerDown())
        {
            holding = true;
            Debug.Log(collidingObject);
            if (collidingObject)
            {
                int index;
                NearestVertexTo(transform.position, out index, out nearestVertex);
                Debug.Log("Index: " + index);
                Debug.Log("NearestVertex: " + nearestVertex);

                if (lastPosition == Vector3.zero)
                {
                    lastPosition = nearestVertex;
                }

                Vector3 newPosition = transform.position;
                Debug.Log("New Position: " + newPosition);
                Vector3 between = newPosition - lastPosition;
                Debug.Log("Between: " + between);
                MeshDeformation deformationScript = collidingObject.GetComponent<MeshDeformation>();
                deformationScript.Deform(nearestVertex, index, between, newPosition); // vertex to change, index of vertex to change, vector representing distance and direction, new vertex
                lastPosition = newPosition;
            }
        }

        if (Controller.GetHairTriggerUp())
        {
            holding = false;
        }
    }
	/* -------- Trigger Functions -------- */
		
	public void OnTriggerEnter(Collider other)
	{
        if (holding == false)
        {
            SetCollidingObject(other);
        }
	}
		
	public void OnTriggerStay(Collider other)
	{
        if (holding == false)
        {
            SetCollidingObject(other);
        }
		
	}
		
	public void OnTriggerExit(Collider other)
	{
		if (!collidingObject)
		{
			return;
		}

        if (holding == false)
        {
            collidingObject = null;
        }
	}

	/* -------- Helpers --------- */

	private void SetCollidingObject(Collider col)
	{
        if (collidingObject || !col.GetComponent<Rigidbody>())
		{
			return;
		}
		collidingObject = col.gameObject; 
	}

	/* Adjusted from https://answers.unity.com/questions/7788/closest-point-on-mesh-collider.html */
	public void NearestVertexTo(Vector3 point, out int index, out Vector3 nearest)
	{

        // Debug.Log("Getting Nearest Vertex");
		// convert point to local space
		point = transform.InverseTransformPoint(point);

		Mesh mesh = collidingObject.GetComponent<MeshFilter>().mesh;
		float minDistanceSqr = Mathf.Infinity;
		Vector3 nearestVertex = Vector3.zero;
		index = 0;
		// scan all vertices to find nearest
		for (int i = 0; i < mesh.vertices.Length; i++)
		{
			Vector3 vertex = mesh.vertices[i];
			Vector3 diff = point-vertex;
			float distSqr = diff.sqrMagnitude;
			if (distSqr < minDistanceSqr)
			{
				minDistanceSqr = distSqr;
				nearestVertex = vertex;
				index = i;
			}
		}
		// convert nearest vertex back to world space
		nearest = transform.TransformPoint(nearestVertex);
	}
}
