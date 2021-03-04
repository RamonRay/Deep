using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockControllerActivator : MonoBehaviour {

    FlockController m_controller;

	// Use this for initialization
	void Start () {
        m_controller = GetComponent<FlockController>();
	}

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.O))
        {
            m_controller.enabled = true;
        }
	}
}
