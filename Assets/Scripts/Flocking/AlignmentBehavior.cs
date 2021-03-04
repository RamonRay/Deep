using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behavior/Alignment")]
public class AlignmentBehavior : FlockBehavior
{
    public override Vector3 CalculateSteering(FlockAgent agent, List<Transform> neighbors, FlockController controller)
    {
        if(neighbors.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 sumVelocity = Vector3.zero;
        foreach(var neighbor in neighbors)
        {
            FlockAgent neighborAgent = neighbor.GetComponent<FlockAgent>();
            sumVelocity += neighborAgent.velocity;
        }
        sumVelocity /= neighbors.Count;

        sumVelocity = sumVelocity.normalized * controller.maxSpeed;
        Vector3 steering = sumVelocity - agent.velocity;
        if(steering.magnitude >= controller.maxAcceleration)
        {
            steering = steering.normalized * controller.maxAcceleration;
        }

        return steering;
    }
}
