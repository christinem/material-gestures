using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformation : MonoBehaviour {

	public static Mesh mesh;
	private Vector3[] meshVertices;
	public GameObject controllerOne;
	public GameObject controllerTwo;

    bool controllerOneCloseEnough = false;
    bool controllerTwoCloseEnough = false;


    // Use this for initialization
    void Start () {
		mesh = ((MeshFilter)gameObject.GetComponent("MeshFilter")).mesh;
		meshVertices = mesh.vertices;

	}

	// Update is called once per frame
	void Update () {
		ControllerScript controlOneScript = controllerOne.GetComponent<ControllerScript>();
		ControllerScript controlTwoScript = controllerTwo.GetComponent<ControllerScript>();

		SteamVR_Controller.Device controlOne = controlOneScript.Controller;
		SteamVR_Controller.Device controlTwo = controlTwoScript.Controller;

        Vector3 controllerOnePos = controllerOne.transform.position;
        int controllerOneIndex = 0;
        Vector3 controllerOneNearest = Vector3.zero;

        Vector3 controllerTwoPos = controllerTwo.transform.position;
        int controllerTwoIndex = 0;
        Vector3 controllerTwoNearest = Vector3.zero;

        Vector3[] newVertices = mesh.vertices;

		if (controlOne.GetHairTriggerDown()) {
      		NearestVertexTo (controllerOnePos, out controllerOneIndex, out controllerOneCloseEnough, out controllerOneNearest);
        }

		if (controlTwo.GetHairTriggerDown()) {
			NearestVertexTo (controllerTwoPos, out controllerTwoIndex, out controllerTwoCloseEnough, out controllerTwoNearest);
		}

       if (controlOne.GetHairTrigger() || controlTwo.GetHairTrigger())
       //if (controllerOneCloseEnough || controllerTwoCloseEnough)
        {
            newVertices = getNewVertices(controllerOnePos, controllerOneNearest, controllerOneIndex, controllerOneCloseEnough,
                                         controllerTwoPos, controllerTwoNearest, controllerTwoIndex, controllerTwoCloseEnough);
            ReRender(newVertices);
        }
        
    }

    private void ReRender(Vector3[] newVertices)
    {
        Vector3[] vertices = mesh.vertices;

        int i = 0;

        while (i < vertices.Length)
        {
            vertices[i] = newVertices[i];
            i++;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        meshVertices = vertices;
    }
		
	private Vector3[] getNewVertices(Vector3 controllerOnePos, Vector3 controllerOneNearest, int controllerOneNearestIndex, 
        bool controllerOneTriggered, Vector3 controllerTwoPos, Vector3 controllerTwoNearest, int controllerTwoNearestIndex, 
        bool controllerTwoTriggered) {
		
		Vector3[] originalVertices = meshVertices; // original vertices of this mesh

        // Get and return the new vertices

        // Temporary testing code
        Vector3[] newVertices = meshVertices;

        if (controllerOneTriggered) {
            Debug.Log("Controller 1");
            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 between = controllerOnePos - controllerOneNearest;
                newVertices[i] = originalVertices[i] + between;
            }
        }

        if (controllerTwoTriggered)
        {
            Debug.Log("Controller 2");
            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 between = controllerTwoPos - controllerTwoNearest;
                newVertices[i] = originalVertices[i] + between;
            }
        }

        return newVertices;
	}

	public void NearestVertexTo(Vector3 point, out int index, out bool isCloseEnough, out Vector3 nearest)
	{

		// Debug.Log("Getting Nearest Vertex");
		// convert point to local space
		point = transform.InverseTransformPoint(point);

		Mesh mesh = GetComponent<MeshFilter>().mesh;
		float minDistanceSqr = Mathf.Infinity;
		Vector3 nearestVertex = Vector3.zero;
		index = 0;

        Debug.Log(mesh.vertices);
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

        if (minDistanceSqr < 3)
        {
            isCloseEnough = true;
        } else
        {
            isCloseEnough = false;
        }
        // convert nearest vertex back to world space
        nearest = transform.TransformPoint(nearestVertex);
	}

}

