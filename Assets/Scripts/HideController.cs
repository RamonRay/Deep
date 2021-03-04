using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class HideController : MonoBehaviour {

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        SetControllerVisible(gameObject, false);
	}

    void SetControllerVisible(GameObject controller, bool visible)
    {
        foreach (SteamVR_RenderModel model in controller.GetComponentsInChildren<SteamVR_RenderModel>())
            model.gameObject.SetActive(visible);
    }
}
