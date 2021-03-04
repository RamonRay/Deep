using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateDisplays : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        // Display 0 activated by default
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
        if (Display.displays.Length > 2)
            Display.displays[2].Activate();
        if (Display.displays.Length > 3)
            Display.displays[3].Activate();
        Display.displays[0].SetRenderingResolution(1920, 1080);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
