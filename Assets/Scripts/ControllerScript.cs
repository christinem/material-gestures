/* Adjusted from source: https://www.raywenderlich.com/149239/htc-vive-tutorial-unity */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerScript : MonoBehaviour {

	private SteamVR_TrackedObject trackedObj;
	public GameObject collidingObject; 
//	private Vector3 lastPosition; // the controller's last position
//	private Vector3 nearestVertex;
	public bool holding;
	Vector3 controllerPosition;

    static MeshDeformation[] meshScripts = new MeshDeformation[2];

    //Logging purposes
    static List<string> output = new List<string>();

    //list of .mesh files to read
    // Listed from most coarse to most fine
    static string[] beams = new string[] {  "C:\\Users\\cmurad\\Documents\\GAUSS\\data\\meshesTetgen\\Beam\\final_beams\\0_coarsest_beam.mesh" ,
                                            "C:\\Users\\cmurad\\Documents\\GAUSS\\data\\meshesTetgen\\Beam\\final_beams\\1_coarse_beam.mesh",
                                            "C:\\Users\\cmurad\\Documents\\GAUSS\\data\\meshesTetgen\\Beam\\final_beams\\2_medium_beam.mesh",
                                            "C:\\Users\\cmurad\\Documents\\GAUSS\\data\\meshesTetgen\\Beam\\final_beams\\3_fine_beam.mesh",
                                            "C:\\Users\\cmurad\\Documents\\GAUSS\\data\\meshesTetgen\\Beam\\final_beams\\4_finest_beam.mesh"};

    int[,] mesh_indices;

    static int cur_mesh_index = 0;
    
    //index of the list above ^ which is currently being displayed by this object
    public int beam_index = 0;
    private void OnDisable()
    {
        //Write file, when game is closed
        System.IO.File.WriteAllLines("C:\\Users\\cmurad\\Documents\\VRSTUDYOUTPUT.txt", output.ToArray());

    }

    public SteamVR_Controller.Device Controller
	{
		get {
            try
            {
            return SteamVR_Controller.Input((int)trackedObj.index);
            }
            catch (System.NullReferenceException ex)
            {
                return null;
            }
        }
	}

    void Awake()
    {

        trackedObj = GetComponent<SteamVR_TrackedObject>();
   
        for (int i = 0; i < 2; i++)
        {
            GameObject go = GameObject.Find("Bar" + (i + 1));
            meshScripts[i] = (MeshDeformation)go.GetComponent(typeof(MeshDeformation));
        }


        int n = beams.Length;
        int n_pairs = n * (n - 1) / 2;
        mesh_indices = new int[n_pairs, 2];

        int k = 0;
        for(int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                int z = UnityEngine.Random.Range(0, 2);
                mesh_indices[k, z % 2] = i;
                mesh_indices[k, (z+1) % 2] = j;
                k++;
            }
        }

        //Random.seed()
        for (int i = n_pairs; i > 1; i--)
        {
            int pos = UnityEngine.Random.Range(0, i);
            for (int j = 0; j < 2; j++)
            {
                var x = mesh_indices[i - 1, j];
                mesh_indices[i - 1, j] = mesh_indices[pos, j];
                mesh_indices[pos, j] = x;
            }
        }


    }

	void Update() {

        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            if (!MeshDeformation.experiment_running)
            {
                GameObject go = GameObject.Find("start_text");

                go.GetComponent<Renderer>().enabled = false;

                for (int i = 0; i < 2; i++)
                {
                    MeshDeformation script = meshScripts[i];
                    script.mesh_file_location = beams[mesh_indices[cur_mesh_index, i]];
                    script.myStart();
                }
                MeshDeformation.experiment_running = true;
                return;
            }

            for (int i = 0; i < 2; i++)
            {
                MeshDeformation script = meshScripts[i];
                if (script.controllerStates[0].isGrabbing)
                {
                    logChosenBar(i);
                    resetObjects();
                    return; // Can only grab one object at a time so return here
                }
            }
        }
    }

    void logChosenBar(int i)
    {
        int betterBar = i;
        int worseBar = (i + 1) % 2;

        output.Add("Better: " + mesh_indices[cur_mesh_index, betterBar] + " Worse: " + mesh_indices[cur_mesh_index, worseBar]);
    }

    void resetObjects()
    {
        cur_mesh_index++;
        if(cur_mesh_index == mesh_indices.GetLength(0))
        {
            //We are done
            GameObject go = GameObject.Find("thank_you_text");

            go.GetComponent<Renderer>().enabled = true;

            for (int i = 0; i < 2; i++)
            {
                MeshDeformation script = meshScripts[i];
                script.meshThreadFinished = true;
            }

            return;
        }

        for (int i = 0; i < 2; i++)
        {
            MeshDeformation script = meshScripts[i];
            script.mesh_file_location = beams[mesh_indices[cur_mesh_index, i]];
            script.reset_timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            script.reset_object_pending = true; // Need to do this so we don't reset the object while the thread is running
        }
    }

	/* -------- Trigger Functions -------- */
		
	//public void OnTriggerEnter(Collider other)
	//{
 //       if (holding == false)
 //       {
 //           SetCollidingObject(other);
 //       }
	//}
		
	//public void OnTriggerExit(Collider other)
	//{
	//	if (!collidingObject)
	//	{
	//		return;
	//	}

 //       if (holding == false)
 //       {
 //           collidingObject = null;
 //       }
	//}

	///* -------- Helpers --------- */

	//private void SetCollidingObject(Collider col)
	//{
 //       if (collidingObject || !col.GetComponent<Rigidbody>())
	//	{
	//		return;
	//	}
	//	collidingObject = col.gameObject; 
	//}
}
