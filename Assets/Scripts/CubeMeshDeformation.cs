/* Source: https://www.dropbox.com/s/9qzav5k85iv9d79/MeshDeformation_Cube.txt?dl=0 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMeshDeformation : MonoBehaviour {

    public bool editable; 

    public GameObject point1;
    public GameObject point2;
    public GameObject point3;
    public GameObject point4;
    public GameObject point5;
    public GameObject point6;
    public GameObject point7;
    public GameObject point8;

    public Material myMaterial;

    // Use this for initialization
    void Start () {
		if (transform.gameObject.GetComponent<MeshCollider>())
        {
            transform.gameObject.GetComponent<MeshCollider>().convex = true;
        }

        if (!editable)
        {
            Vector3[] Verticles = new Vector3[]
            {
                //6 sides
				//back side
				new Vector3(0,0,0),new Vector3(1,0,0),new Vector3(0,1,0), new Vector3(1,1,0),
				//right side
				new Vector3(1,0,0),new Vector3(1,0,1),new Vector3(1,1,0), new Vector3(1,1,1),
				//forward side
				new Vector3(1,0,1),new Vector3(0,0,1),new Vector3(1,1,1), new Vector3(0,1,1),
				//left side
				new Vector3(0,0,1),new Vector3(0,0,0),new Vector3(0,1,1), new Vector3(0,1,0),
				//up side
				new Vector3(0,1,0),new Vector3(1,1,0),new Vector3(0,1,1), new Vector3(1,1,1),
				//down side
				new Vector3(0,0,0),new Vector3(1,0,0),new Vector3(0,0,1), new Vector3(1,0,1),
            };

            int[] Triangles = new int[]
            {
                0,3,1,
                3,0,2,

                4,7,5,
                7,4,6,

                8,11,9,
                11,8,10,

                12,15,13,
                15,12,14,

                16,19,17,
                19,16,18,

                21,22,20,
                22,21,23,
            };

            Vector2[] UV = new Vector2[]
            {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),

                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),

                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),

                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),

                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),

                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),
            };

            Vector3[] Normals = new Vector3[]
            {
                Vector3.back,
                Vector3.back,
                Vector3.back,
                Vector3.back,

                Vector3.right,
                Vector3.right,
                Vector3.right,
                Vector3.right,

                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,

                Vector3.left,
                Vector3.left,
                Vector3.left,
                Vector3.left,

                Vector3.up,
                Vector3.up,
                Vector3.up,
                Vector3.up,

                Vector3.down,
                Vector3.down,
                Vector3.down,
                Vector3.down,
            };

            if (!transform.GetComponent<MeshFilter>())
            {
                transform.gameObject.AddComponent<MeshFilter>();
            }
            if (!transform.GetComponent<MeshRenderer>())
            {
                transform.gameObject.AddComponent<MeshRenderer>();
            }

            Mesh myMesh = new Mesh();

            myMesh.vertices = Verticles;
            myMesh.triangles = Triangles;
            myMesh.normals = Normals;
            myMesh.uv = UV;
            ;

            transform.GetComponent<MeshFilter>().mesh = myMesh;

            if (transform.GetComponent<MeshCollider>())
            {
                transform.GetComponent<MeshCollider>().sharedMesh = myMesh;
            }

            transform.GetComponent<Renderer>().material = myMaterial;

        }
	}

    // Update is called once per frame
    void Update()
    {
        //MODE 2 = Editable Cube

        if (editable)
        {
            Vector3[] Verticles = new Vector3[]
            {
				//6 Sides
				//Back Side
				point1.transform.position,point2.transform.position,point3.transform.position,point4.transform.position,
				//Right Side
				point2.transform.position,point6.transform.position,point4.transform.position,point8.transform.position,
				//Front Side
				point5.transform.position,point6.transform.position,point7.transform.position,point8.transform.position,
				//Left Side
				point5.transform.position,point1.transform.position,point7.transform.position,point3.transform.position,
				//Up Side
				point3.transform.position,point4.transform.position,point7.transform.position,point8.transform.position,
				//Down Side
				point1.transform.position,point2.transform.position,point5.transform.position,point6.transform.position,
            };


            int[] Triangles = new int[]
            {
                2,1,0,
                1,2,3,

                4,7,5,
                7,4,6,

                9,10,8,
                10,9,11,

                12,15,13,
                15,12,14,

                16,19,17,
                19,16,18,

                21,22,20,
                22,21,23,
            };

            Vector2[] UV = new Vector2[]
            {

                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),

                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),

                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),

                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),

                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),

                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),
            };

            Vector3[] Normals = new Vector3[]
            {
                Vector3.back,
                Vector3.back,
                Vector3.back,
                Vector3.back,

                Vector3.right,
                Vector3.right,
                Vector3.right,
                Vector3.right,

                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,

                Vector3.left,
                Vector3.left,
                Vector3.left,
                Vector3.left,

                Vector3.up,
                Vector3.up,
                Vector3.up,
                Vector3.up,

                Vector3.down,
                Vector3.down,
                Vector3.down,
                Vector3.down,
            };

            if (!transform.GetComponent<MeshFilter>())
            {
                transform.gameObject.AddComponent<MeshFilter>();
            }
            if (!transform.GetComponent<MeshRenderer>())
            {
                transform.gameObject.AddComponent<MeshRenderer>();
            }

            Mesh myMesh = new Mesh();

            myMesh.vertices = Verticles;
            myMesh.triangles = Triangles;
            myMesh.normals = Normals;
            myMesh.uv = UV;
            ;

            transform.GetComponent<MeshFilter>().mesh = myMesh;

            if (transform.GetComponent<MeshCollider>())
            {
                transform.GetComponent<MeshCollider>().sharedMesh = myMesh;
            }

            transform.GetComponent<Renderer>().material = myMaterial;
        }
    }
}
