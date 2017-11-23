using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformation : MonoBehaviour {

	public static Mesh mesh;
	private Vector3[] meshVertices;


	// Use this for initialization
	void Start () {
		mesh = ((MeshFilter)gameObject.GetComponent("MeshFilter")).mesh;
		meshVertices = mesh.vertices;

	}
	
	// Update is called once per frame
	void Update () {

	}

	public void Deform(Vector3 oldVertex, int index, Vector3 between, Vector3 newVertex) {

		Vector3[] vertices = mesh.vertices;
		Vector3[] newVertices = getNewVertices (oldVertex, index, between, newVertex);
		int i = 0;
		while (i < vertices.Length) {
			vertices[i] = newVertices[i];
			i++;
		}
		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		meshVertices = vertices;

	}

	private Vector3[] getNewVertices(Vector3 oldVertex, int index, Vector3 between, Vector3 newVertex) {
		Vector3[] originalVertices = meshVertices; // original vertices of this mesh

		// Get and return the new vertices

		// Temporary testing code
		Vector3[] newVertices = new Vector3[originalVertices.Length];

		for (int i = 0; i < originalVertices.Length; i++) {
			newVertices[i] = originalVertices[i] + between;
		}

		return newVertices;
	}

}

