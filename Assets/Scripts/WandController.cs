using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandController : MonoBehaviour {

    private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int) trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;

    private GameObject pickup;

    Mesh deformingMesh;

	// Use this for initialization
	void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
	}
	
	// Update is called once per frame
	void Update () {
        // Deform Mesh of object
        deformingMesh = pickup.GetComponent<MeshFilter>().mesh;
	}

    public void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Trigger entered");
        pickup = collider.gameObject;
    }

    public void OnTriggerExit(Collider collider)
    {
        Debug.Log("Trigger exist");
        pickup = null;
    }
} 
