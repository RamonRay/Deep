using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class VacuumedObject : MonoBehaviour {
    public int score=1;
    public int soundIndex=0;
    public bool damage = false;


    [SerializeField]
    float maxVelocity = 10f;
    [SerializeField]
    bool useGravity = true;
    [SerializeField]
    bool changeIsTrigger = false;

    private Rigidbody rb;
    //private bool isVacuumed = false;
    private bool[] isVacuumed;
    private GameObject[] vacuumObject;
	// Use this for initialization-
	void Start () {
        rb = GetComponent<Rigidbody>();
        isVacuumed = new bool[2] { false, false };
        vacuumObject = new GameObject[2];
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Vacuumed(Transform vacuumTransform,float vacuumForce)
    {
        Vector3 distance = vacuumTransform.position - transform.position;
        Vector3 force = distance.normalized * vacuumForce/Mathf.Max(distance.magnitude,1f);
        force += Vector3.Cross(distance.normalized,vacuumTransform.forward)/25f;
        rb.AddForce(force);
        rb.AddTorque(vacuumTransform.forward);
    }
    public void OnVacuumed(int hand)
    {
        if(!isVacuumed[0]&&!isVacuumed[1])
        {
            if (useGravity)
            {
                rb.useGravity = false;
            }

            rb.drag = 0f;

            if (!changeIsTrigger)
            {
                GetComponent<BoxCollider>().isTrigger = true;
            }

        }
        isVacuumed[hand] = true;
    }
    public void OnUnVacuumed(int hand)
    {
        isVacuumed[hand] = false;
        if (!isVacuumed[0]&&!isVacuumed[1])
        {
            try
            {
                rb.velocity = rb.velocity*0.1f;
                if(useGravity)
                {
                    rb.useGravity = true;
                }
                rb.drag = rb.mass;
            }
            catch
            {
                //
            }

            if (!changeIsTrigger)
            {
                GetComponent<BoxCollider>().isTrigger = false;
            }
        }
    }
    private void OnDestroy()
    {
        try
        {
            SubmarineVacuum.vacuums[0].UnRegisterObject(gameObject);
        }
        catch
        {
            Debug.Log("Not in 0");
        }
        try
        {
            SubmarineVacuum.vacuums[1].UnRegisterObject(gameObject);
        }
        catch
        {
            Debug.Log("Not in 1");
        }
    }

}
