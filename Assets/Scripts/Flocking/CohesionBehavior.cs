using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behavior/Cohesion")]
public class CohesionBehavior : FlockBehavior
{
    // public float maxAcceleration = 1f;

    public override Vector3 CalculateSteering(FlockAgent agent, List<Transform> neighbors, FlockController controller)
    {
        if(neighbors.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 neighborCenter = Vector3.zero;
        foreach(var neighbor in neighbors)
        {
            neighborCenter += neighbor.position;
        }
        neighborCenter /= neighbors.Count;

        Vector3 targetVelocity = neighborCenter - agent.transform.position;
        Vector3 steering = targetVelocity - agent.velocity;
        if(steering.magnitude >= controller.maxAcceleration)
        {
            steering = steering.normalized * controller.maxAcceleration;
        }

        return steering;
    }
}
