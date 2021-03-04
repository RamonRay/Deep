using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FlockBehavior : ScriptableObject {
    public abstract Vector3 CalculateSteering(FlockAgent agent, List<Transform> neighbors, FlockController controller);

}
