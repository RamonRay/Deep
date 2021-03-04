using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behavior/Avoidance")]
public class AvoidanceBehavior : FlockBehavior
{
    public override Vector3 CalculateSteering(FlockAgent agent, List<Transform> neighbors, FlockController controller)
    {
        // if no neighbors, return no steering
        if(neighbors.Count == 0)
        {
            return Vector3.zero;
        }

        // add all points together and average
        Vector3 steering = Vector3.zero;
        int numToAvoid = 0;

        foreach(var neighbor in neighbors)
        {
            float sqrDistance = Vector3.SqrMagnitude(neighbor.transform.position - agent.transform.position);
            if(sqrDistance <= controller.sqrAvoidanceRadius)
            {
                var diff =  agent.transform.position - neighbor.position;
                diff.Normalize();
                diff /= Mathf.Sqrt(sqrDistance);

                steering += diff;

                numToAvoid++;
            }
        }

        if(numToAvoid > 0)
        {
            steering /= numToAvoid;
        }

        // steering.Normalize();
        steering *= controller.maxSpeed;
        steering -= agent.velocity;
        if(steering.magnitude > controller.maxAcceleration)
        {
            steering = steering.normalized * controller.maxAcceleration;
        }

        return steering;
    }
}
