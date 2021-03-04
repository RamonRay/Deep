using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockAgent : MonoBehaviour {

    public float lookAtSpeedThreshold = 0.5f;
    public Collider agentCollider { get; private set; }

    public System.Action<FlockController> onInitialized;

    public Vector3 velocity
    {
        get
        {
            return m_rigidbody.velocity;
        }
    }

    private FlockController m_controller;
    private Rigidbody m_rigidbody;



    public void Initialize(FlockController controller)
    {
        if(agentCollider == null)
        {
            agentCollider = GetComponent<Collider>();
        }
        m_rigidbody = GetComponent<Rigidbody>();

        m_controller = controller;

        if(onInitialized != null)
            onInitialized(controller);
    }

    public void ApplySteering(Vector3 linearSteering)
    {
        m_rigidbody.AddForce(linearSteering * m_controller.driveFactor, ForceMode.Acceleration);
        if(m_rigidbody.velocity.sqrMagnitude > m_controller.sqrMaxSpeed)
        {
            m_rigidbody.velocity = m_rigidbody.velocity.normalized * m_controller.maxSpeed;
        }

        if(m_rigidbody.velocity.magnitude > lookAtSpeedThreshold)
        {
            var rotation = Quaternion.LookRotation(m_rigidbody.velocity.normalized, Vector3.up);
            m_rigidbody.rotation = Quaternion.Slerp(m_rigidbody.rotation, rotation, 0.1f);
        }
    }


	// Update is called once per frame
	void Update ()
    {

	}

    void OnDrawGizmosSelected()
    {
        var avoidanceColor = Color.red;
        avoidanceColor.a = 0.25f;
        var neighborColor = Color.green;
        neighborColor.a = 0.25f;

        // if(m_controller)
        // {
        //     Gizmos.color = avoidanceColor;
        //     Gizmos.DrawSphere(transform.position, Mathf.Sqrt(m_controller.sqrAvoidanceRadius));
        //     Gizmos.color = neighborColor;
        //     Gizmos.DrawSphere(transform.position, m_controller.neighborRadius);
        // }
    }
}
