using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DMXController18 : MonoBehaviour {

    public static DMXController18 instance;
    private GameObject osc ;
	private string whichLight ;

    private Color[] colors = new Color[4] { Color.white, Color.red, Color.green, Color.blue };  // for UI

    // Use this for initialization
    void Start () {
        osc = GameObject.Find("OSCMain");
        //Blackout ();
        Invoke("Blackout", 1);  // need to give the networking time to connect

        if(instance==null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("There are more than one DMXController in the scene.");
        }
    }
	
	// Update is called once per frame
	void Update () {
        // if the esacepe key is pressed then player is exiting - quick, send a message to the lighting manager 
        if (Input.GetKeyDown(KeyCode.Escape))
        { 
            osc.SendMessage("SendOSCMessage", "/lighting show theme");  // start up the theme lighting again
        }
    }

    public void Blackout()
    {
        osc.SendMessage("SendOSCMessage", "/lighting operations blackout");
    }

    public void AllOn()
    {
        osc.SendMessage("SendOSCMessage", "/lighting operations allOn");
    }

    public void TurnOn(string groupName, int red, int green, int blue, int amber, int dimmer)
    {
        osc.SendMessage("SendOSCMessage", "/lighting color " + groupName + " " + red.ToString() + " " + green.ToString() + " " + blue.ToString() + " " + amber.ToString() + " " + dimmer.ToString());
    }

    public void TurnOn(string groupName, Color32 thisColor, int amber, int dimmer)
    {
        osc.SendMessage("SendOSCMessage", "/lighting color " + groupName + " " + thisColor.r.ToString() + " " + thisColor.g.ToString() + " " + thisColor.b.ToString() + " " + amber.ToString() + " " + dimmer.ToString());
    }

    public void TurnOn(string groupName, Color thisColor, int amber, int dimmer)
    {
        osc.SendMessage("SendOSCMessage", "/lighting color " + groupName + " " + (thisColor.r * 255).ToString() + " " + (thisColor.g * 255).ToString() + " " + (thisColor.b * 255).ToString() + " " + amber.ToString() + " " + dimmer.ToString());
    }

    public void TurnOff(string groupName)
    {
        osc.SendMessage("SendOSCMessage", "/lighting color " + groupName + " 0 0 0 0 0");
    }

    /*------------------The methods below are called by the UI--------------------------*/

    public void GuestLights(int i)
    {
        // send a command to the lighting controller to turn the guest lights on
        TurnOn("Guest", colors[i], 0, 255);
    }

    public void SetLights()
    {
        // send a command to the lighting controller to turn the set lights on
        UseCue("Tour", 3);
    }

    /*------------------The methods below have not been tested--------------------------*/

    public void MoveVulture(int pan, int tilt, int finePan, int fineTilt)
    {
        osc.SendMessage("SendOSCMessage", "/lighting move vulture " + pan.ToString() + " " + tilt.ToString() + " " + finePan.ToString() + " " + fineTilt.ToString());
    }

    /*thisColor is an integer from 0-255 with ranges that cover about 5 Colors. */

    public void TurnOnWaterLight(int thisColor, int rotation, int dimmer)
    {
        osc.SendMessage("SendOSCMessage", "/lighting color h20 " + thisColor.ToString() + " " + rotation.ToString() + " 0 0 " + dimmer.ToString());
    }

    public void UseCue(string cueName, int cueNumber)
    {
        osc.SendMessage("SendOSCMessage", "/lighting cue " + cueName + " " + cueNumber.ToString());
    }

    public void UseShow(string showName)
    {
        osc.SendMessage("SendOSCMessage", "/lighting show " + showName);
    }
}
