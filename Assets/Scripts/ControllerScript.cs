/* Adjusted from source: https://www.raywenderlich.com/149239/htc-vive-tutorial-unity */

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
//		lastPosition = Vector3.zero;
	}

	void Update() {
		controllerPosition = transform.position;
        if (Controller.GetHairTriggerDown())
        {
            holding = true;
        } 

        if (Controller.GetHairTriggerUp())
        {
            holding = false;
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
