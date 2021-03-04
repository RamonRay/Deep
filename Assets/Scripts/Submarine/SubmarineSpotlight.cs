using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineSpotlight : MonoBehaviour {
    [SerializeField] Transform controller;
    [SerializeField] Transform spotLight;
    [SerializeField] LayerMask flashlightLayer;
    public RaycastHit hit { get; private set; }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        RaycastLighting();
	}
    //Sync the rotation of the spotlight with the controller.
    private void SyncRotation()
    {
        if (controller.gameObject.activeSelf)
        {
            spotLight.rotation = controller.rotation;
        }
        else
        {
            //Do Nothing
        }
    }

    //Lightup the area where the controller is pointing at.
    private void RaycastLighting()
    {
        RaycastHit _hit;
        if(Physics.Raycast(controller.position,controller.forward,out _hit,100f,flashlightLayer))
        {
            spotLight.LookAt(hit.point);
        }
        else
        {
            SyncRotation();
        }
        hit = _hit;
    }
}
