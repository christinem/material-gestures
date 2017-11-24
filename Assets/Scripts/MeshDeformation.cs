using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformation : MonoBehaviour {

	public static Mesh mesh;
	private Vector3[] meshVertices;
	public GameObject controllerOne;
	public GameObject controllerTwo;


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

		Vector3[] vertices = mesh.vertices;
		Vector3[] newVertices = mesh.vertices;

		// if pressing controller one button and controller one is colliding with this object
		if (controlOne.GetHairTrigger () && controlOneScript.collidingObject == gameObject) {
			// get controller one position and nearest index and do the deformation 
			Vector3 controllerOnePos = controllerOne.transform.position;
			int controllerOneIndex;
			Vector3 controllerOneNearest;

			NearestVertexTo (controllerOnePos, out controllerOneIndex, out controllerOneNearest);
			newVertices = getNewVertices (controllerOnePos, controllerOneIndex, controllerOneNearest);
		}

		// if pressing controller two button and controller one is colliding with this object
		if (controlTwo.GetHairTrigger () && controlTwoScript.collidingObject == gameObject) {
			// get controller two position and nearest index and do the deformation 
			Vector3 controllerTwoPos = controllerTwo.transform.position;
			int controllerTwoIndex;
			Vector3 controllerTwoNearest;

			NearestVertexTo (controllerTwoPos, out controllerTwoIndex, out controllerTwoNearest);
			newVertices = getNewVertices (controllerTwoPos, controllerTwoIndex, controllerTwoNearest);
		}

		// re-render mesh based on new vertices
		int i = 0;

		while (i < vertices.Length) {
			vertices[i] = newVertices[i];
			i++;
		}

		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		meshVertices = vertices;
	}
		
	private Vector3[] getNewVertices(Vector3 controllerPos, int controllerPosIndex, Vector3 controllerNearest) {
		
		Vector3[] originalVertices = meshVertices; // original vertices of this mesh

		// Get and return the new vertices

		// Temporary testing code
		Vector3[] newVertices = new Vector3[originalVertices.Length];

		for (int i = 0; i < originalVertices.Length; i++) {
            Vector3 between = controllerPos - controllerNearest;
            newVertices[i] = originalVertices[i] + between;
		}

		return newVertices;
	}

	public void NearestVertexTo(Vector3 point, out int index, out Vector3 nearest)
	{

		// Debug.Log("Getting Nearest Vertex");
		// convert point to local space
		point = transform.InverseTransformPoint(point);

		Mesh mesh = GetComponent<MeshFilter>().mesh;
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

