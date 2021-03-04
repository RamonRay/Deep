using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineFloorControl : MonoBehaviour
{

    public static SubmarineFloorControl instance;
    [SerializeField] float floorMoveInterval = 1f;
    [Range(1f, 5f)] [SerializeField] float tiltRange = 2f;
    /// <summary>
    /// The index of floor contollers according to its orientation.
    /// 2 3
    /// 0 1
    /// </summary>
    private Vector4 floorVoltages;
    private float _tiltAngle = 0f;
    public float tiltAngle
    {
        get
        {
            return _tiltAngle;
        }
        set
        {
            float oldTiltAngle = _tiltAngle;
            _tiltAngle = value;
            if(_tiltAngle>=0.5f&&oldTiltAngle<0.5f)
            {
                TiltRight();
            }
            else if(_tiltAngle<=-0.5f&&oldTiltAngle>-0.5f)
            {
                TiltLeft();
            }
            else if( Mathf.Abs(_tiltAngle)<=0.1f&&Mathf.Abs(oldTiltAngle)>0.1f)
            {
                NoTilt();
            }
        }
    }
    // Use this for initialization
    void Start()
    {
        if(instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        floorVoltages = new Vector4(5f, 5f, 5f, 5f);
        //SetFloorHeight();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SetFloorHeight()
    {
        FloorController.instance.enable();
        try
        {
            FloorController.instance.moveOne(0, floorVoltages.x);
            FloorController.instance.moveOne(1, floorVoltages.y);
            FloorController.instance.moveOne(2, floorVoltages.z);
            FloorController.instance.moveOne(3, floorVoltages.w);
        }
        catch
        {
            Debug.LogError("Cave platform not connected");
        }
        Debug.Log(floorVoltages[0] + "," + floorVoltages[1] + "," + floorVoltages[2] + "," + floorVoltages[3]);
    }
    private void TiltLeft()
    {
        FloorController.instance.enable();
        FloorController.instance.raiseRight(tiltRange);
        
    }
    private void TiltRight()
    {
        FloorController.instance.enable();
        FloorController.instance.raiseLeft(tiltRange);
    }
    private void NoTilt()
    {
        FloorController.instance.enable();
        FloorController.instance.lowerFloor();
    }

    public void Shake()
    {
        FloorController.instance.enable();
        FloorController.instance.moveAll(5f);
        Invoke("NoTilt", 1f);
    }
}
