using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behavior/Chase")]
public class ChaseBehavior : FlockBehavior
{
    public float maxAcceleration = 1f;
    public float targetRadius = 1f;
    public float slowRadius = 5f;
    public float timeToTarget = 0.1f;
    public bool useNeighborCenter = false;

    public override Vector3 CalculateSteering(FlockAgent agent, List<Transform> neighbors, FlockController controller)
    {
        var agentTrans = agent.transform;

        var neighborCenter = Vector3.zero;
        if(neighbors.Count == 0)
        {
            neighborCenter = agent.transform.position;
        }
        else
        {
            foreach(var neighbor in neighbors)
            {
                neighborCenter += neighbor.position;
            }
            neighborCenter /= neighbors.Count;
        }

        Vector3 selfPos = agentTrans.position;
        if(useNeighborCenter)
        {
            selfPos = neighborCenter;
        }

        var direction = controller.chaseTarget.position - selfPos;
        var distance = direction.magnitude;


        // check if we are there, if so, return no steering
        if(distance < targetRadius)
        {
            return Vector3.zero;
        }

        float targetSpeed = 0f;
        if(distance > slowRadius)
        {
            targetSpeed = controller.maxSpeed;
        }
        else
        {
            targetSpeed = controller.maxSpeed * (distance - targetRadius) / slowRadius;
            // targetSpeed = controller.maxSpeed * distance / slowRadius;
        }

        Vector3 targetVelocity = direction.normalized;
        targetVelocity *= targetSpeed;

        Vector3 linearSteering = targetVelocity - agent.velocity;
        linearSteering /= timeToTarget;

        if(linearSteering.magnitude > maxAcceleration)
        {
            linearSteering = linearSteering.normalized * maxAcceleration;
        }

        return linearSteering;
    }
}
