using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class keys : MonoBehaviour {

    public Camera camL;
    public Camera camC;
    public Camera camR;
    public Text posText;
    public Text rotText;
    public Text xText;
    public Text widthText;
    public Text yText;
    public Text heightText;
    public Text posYText;
    public Transform camNode;

    private Camera cam;



    // Use this for initialization
    void Start () {
        cam = camL;
    }
	
	// Update is called once per frame
	void Update () {

        // QUIT
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // MOVE THE CAMER RIG
        if (Input.GetKey(KeyCode.W))
        {
            camNode.Translate(Vector3.forward * 0.1f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            camNode.Translate(Vector3.forward * -0.1f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            camNode.Rotate(Vector3.up * -0.5f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            camNode.Rotate(Vector3.up * 0.5f);
        }

        if (Input.GetKey(KeyCode.R))
        {
            camNode.Translate(Vector3.up * 0.1f);
        }
        if (Input.GetKey(KeyCode.F))
        {
            camNode.Translate(Vector3.up * -0.1f);
        }

        posText.text = "Pos : " + camNode.position.x.ToString() + " , " + camNode.position.y.ToString() + " , " + camNode.position.z.ToString();
        rotText.text = "Rot : " + camNode.eulerAngles.x.ToString() + " , " + camNode.eulerAngles.y.ToString() + " , " + camNode.eulerAngles.z.ToString(); 


        // CAMERA RECT CONTROLS
        var newT = new Rect(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);

        
        if (Input.GetKeyDown(KeyCode.T))
        {
            newT.x -= 0.001f;
        }
        if(Input.GetKeyDown(KeyCode.Y))
        {
            newT.x +=  0.001f ;
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            newT.width -= 0.001f ;
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            newT.width += 0.001f;
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            newT.y -= 0.001f;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            newT.y += 0.001f;
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            newT.height -= 0.001f;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            newT.height += 0.001f;
        }

        cam.rect = newT;

        xText.text = "X : " + cam.rect.x.ToString();
        widthText.text = "Width : " + cam.rect.width.ToString();
        yText.text = "Y : " + cam.rect.y.ToString();
        heightText.text = "Height : " + cam.rect.height.ToString();

        // POSITION THE CAMERA ON Y
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            cam.transform.Translate(Vector3.up * 0.01f);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            cam.transform.Translate(Vector3.up * -0.01f);
        }

        posYText.text = "posY : " + cam.transform.localPosition.y.ToString();

    }

    public void SelectCamera(string which)
    {
        // called by the UI Camera Select buttons to select which camera to adjust
        switch (which)
        {
            case "left":
                cam = camL;
                break;
            case "center":
                cam = camC;
                break;
            case "right":
                cam = camR;
                break;
        }

    }
}
