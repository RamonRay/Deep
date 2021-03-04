 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
[RequireComponent(typeof(Rigidbody))]
public class SubmarineController : MonoBehaviour {

    public static SubmarineController instance;

    [SerializeField] public float maxVelocity = 10f;
    [SerializeField] float accelerationRate = 1f;
    [SerializeField] float angularSpeed = 10f;
    [SerializeField] Vector2 heightRange = new Vector2(0f, 10f);
    [SerializeField] Transform tiltComponent;
    [SerializeField] float maxTiletedAngle = 10f;
    [SerializeField] float floatSpeed = 2f;

    private float speedAmplifier = 1f;
    private float verticalOverride = 0f;
    private float floatOverride = 0f;
    private float fixedDeltaAngle;
    private float subHorizontal = 0f;
    private float waterTankGravity = 0f;
    private Rigidbody rb;
    private Vector3 realtimeAcceleration;
    private SubmarineFloorControl subFloor;
    private SubmarineSoundController sound;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        fixedDeltaAngle = angularSpeed * Time.fixedDeltaTime;
        subFloor = GetComponent<SubmarineFloorControl>();
        sound = GetComponent<SubmarineSoundController>();
        if(instance==null)
        {
            instance = this;
        }
        else
        {
            //?
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            EmergencyButton(5f);
        }
        Dive();
        Rotate();
        Move();
    }

    //Move the submarine by adding force to its rigidbody.
    private void Move()
    {
        float _vertical = 0f;
        _vertical = Input.GetAxisRaw("Vertical");
        if (verticalOverride != 0f)
        {
            _vertical = verticalOverride;
        }
        float flag = 1f;
        if (_vertical == 0f)
        {
            rb.drag = 1f;
        }
        else
        {
            rb.drag = 0f;
        }
        if (_vertical < 0f)
        {
            flag = -1f;
        }
        Vector3 _acc = _vertical * transform.forward * accelerationRate * Time.fixedDeltaTime;//*(1f-0.5f*flag*rb.velocity.magnitude/maxVelocity);
        Debug.Log(_acc);
        if (rb.velocity.magnitude < maxVelocity * speedAmplifier)
        {
            rb.AddForce(_acc * speedAmplifier, ForceMode.VelocityChange);
        }
        //Sound
        if (_vertical != 0f)
        {
            sound.isStart = true;
            sound.velocity = rb.velocity.magnitude;
            sound.power = Mathf.Abs(_vertical);
        }
        else
        {
            sound.isStart = false;
        }
    }

    //Rotate the submarine and make its velocity along its direction.
    private void Rotate()
    {
        float _horizontal = 0f;
        _horizontal = Input.GetAxisRaw("Horizontal");
        _horizontal = updateHorizontal(_horizontal);
        try
        {
            subFloor.tiltAngle = _horizontal;
        }
        catch
        {
            Debug.LogError("Cannot tilted floor while turning around");
        }

        if (_horizontal == 0f)
        {
            rb.constraints = (RigidbodyConstraints)112;//Freeze all axis rotation
        }
        else
        {
            rb.constraints = (RigidbodyConstraints)80;//Freeze X and Z axis rotation
        }
        float deltaAngle = fixedDeltaAngle * _horizontal;
        Quaternion euler = Quaternion.Euler(0, deltaAngle * speedAmplifier, 0);
        tiltComponent.localRotation = Quaternion.Euler(0, 0, -_horizontal * maxTiletedAngle);
        Vector3 deltaV = euler * rb.velocity - rb.velocity;
        rb.AddForce(deltaV, ForceMode.VelocityChange);
        transform.Rotate(0, deltaAngle * speedAmplifier, 0);
    }
    //Needs rewriting
    private void Dive()
    {
        
        float up = Input.GetAxisRaw("XBOX Y");
        if(floatOverride!=0f)
        {
            up = floatOverride;
        }
        rb.velocity = new Vector3(rb.velocity.x, up * floatSpeed*speedAmplifier, rb.velocity.z);
    }
    /*
    private float BuoyancyForce()
    {
        return (heightRange.y - transform.position.y) /(heightRange.y-heightRange.x);
    }
    */
    private float updateHorizontal(float _horizontal)
    {
        if (Mathf.Abs(_horizontal - subHorizontal) < 0.02f)
        {
            subHorizontal = _horizontal;
        }
        else if (_horizontal > subHorizontal)
        {
            subHorizontal += 0.02f;
        }
        else
        {
            subHorizontal -= 0.02f;
        }
        return subHorizontal;
    }

    public void EmergencyButton(float amplifier)
    {
        if (verticalOverride != 0f)
            return;
        speedAmplifier = amplifier;
        StartCoroutine(AmpDecay());

    }

    IEnumerator AmpDecay()
    {
        verticalOverride = 1f;
        yield return new WaitForSeconds(5f);
        while (speedAmplifier > 1f)
        {
            speedAmplifier -= Time.fixedDeltaTime * 10f;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        speedAmplifier = 1f;
        verticalOverride = 0f;
        yield break;
    }

    public void EmergencyFloat(float amplifier)
    {
        if (floatOverride != 0f)
            return;
        speedAmplifier = amplifier;
        StartCoroutine(FloatDecay());
    }

    IEnumerator FloatDecay()
    {
        floatOverride = 1f;
        yield return new WaitForSeconds(5f);
        float rate = speedAmplifier - 1f;
        while (speedAmplifier > 1f)
        {
            speedAmplifier -= Time.fixedDeltaTime * 10f;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        speedAmplifier = 1f;
        floatOverride = 0f;
        yield break;
    }
}
