using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FlockController : MonoBehaviour
{
    [System.Serializable]
    public class WeightedFlockBehavior
    {
        public FlockBehavior behavior;
        [Range(0f, 2f)]
        public float weight = 1f;
    }

    public FlockAgent agentPrefab;

    [Header("Agent Generation")]
    [Range(10, 500)]
    public int startCount = 20;
    public float agentDensity = 0.08f;

    [Header("Agent Properties")]
    [Range(1f, 100f)]
    public float driveFactor = 10f;
    [Range(1f, 100f)]
    public float maxSpeed = 5f;
    [Range(0.1f, 100f)]
    public float maxAcceleration = 1f;
    [Range(1f, 10f)]
    public float neighborRadius = 1.5f;
    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier = 0.5f;
    public LayerMask agentLayerMask;

    [Header("Flock Behaviors")]
    public List<WeightedFlockBehavior> weightedFlockBehaviors;

    [Header("Flock Properties")]
    public Transform chaseTarget;

#region Dependent Variables
    public float sqrMaxSpeed { get; private set; }
    public float sqrNeighborRadius { get; private set; }
    public float sqrAvoidanceRadius { get; private set; }
#endregion

    private List<FlockAgent> m_Agents = new List<FlockAgent>();

    private float GetGenerateSphereRadius()
    {
        var sphereVolume = startCount * agentDensity;
        var sphereRadius = Mathf.Pow(0.75f * 1.0f / Mathf.PI * sphereVolume, 1.0f / 3.0f);
        return sphereRadius;
    }

    public void RegenerateAgents()
    {
        RemoveAgents();

        var sphereRadius = GetGenerateSphereRadius();

        for(int agentId = 0; agentId < startCount; ++agentId)
        {
            var agentPosOffset = Random.insideUnitSphere * sphereRadius;
            var agentPos = agentPosOffset + transform.position;


            FlockAgent newAgent = Instantiate(
                agentPrefab,
                agentPos,
                Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)),
                transform
            ) as FlockAgent;

            newAgent.gameObject.name += "Agent" + agentId;
            newAgent.Initialize(this);
            m_Agents.Add(newAgent);
        }

    }

    public void RemoveAgents()
    {
        m_Agents.Clear();
        var agents = GetComponentsInChildren<FlockAgent>(true);

        foreach(var agent in agents)
        {
#if UNITY_EDITOR
            if(!EditorApplication.isPlaying)
            {
                DestroyImmediate(agent);
                continue;
            }
#endif
            Destroy(agent);
        }
    }

    public T GetFlockBehavior<T>() where T : FlockBehavior
    {
        foreach(var weightedBehavior in weightedFlockBehaviors)
        {
            var behavior = weightedBehavior.behavior;
            if(behavior is T)
            {
                return behavior as T;
            }
        }
        return null;
    }

    public void RemoveAgent(FlockAgent agent)
    {
        agent.transform.parent = null;
        m_Agents.Remove(agent);
    }

    private void UpdateDependentVariables()
    {
        sqrMaxSpeed = maxSpeed * maxSpeed;
        sqrNeighborRadius = neighborRadius * neighborRadius;
        sqrAvoidanceRadius = sqrNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;
    }

    private List<Transform> GetNeighbors(FlockAgent agent)
    {
        var neighbors = new List<Transform>();
        Collider[] neighborColliders = Physics.OverlapSphere(
            agent.transform.position,
            neighborRadius,
            agentLayerMask,
            QueryTriggerInteraction.UseGlobal
        );

        foreach(var collider in neighborColliders)
        {
            if(collider != agent.agentCollider)
            {
                neighbors.Add(collider.transform);
            }
        }

        return neighbors;
    }

	// Use this for initialization
	void OnEnable ()
    {
        UpdateDependentVariables();
        RegenerateAgents();
	}

    void OnDisable()
    {

    }

    void OnValidate()
    {
        UpdateDependentVariables();
    }

	void FixedUpdate ()
    {
        foreach(FlockAgent agent in m_Agents)
        {
            var neighbors = GetNeighbors(agent);
            var steering = CalculatedBlendedSteering(agent, neighbors);
            agent.ApplySteering(steering);
        }
	}

    Vector3 CalculatedBlendedSteering(FlockAgent agent, List<Transform> neighbors)
    {
        Vector3 blendedSteering = Vector3.zero;
        foreach(var weightedBehavior in weightedFlockBehaviors)
        {
            var behavior = weightedBehavior.behavior;
            var weight = weightedBehavior.weight;

            Vector3 steering = behavior.CalculateSteering(agent, neighbors, this);

            blendedSteering += steering * weight;
        }

        return blendedSteering;
    }

    void OnDrawGizmosSelected()
    {
        var generateSphereColor = Color.red;
        generateSphereColor.a = 0.25f;
        Gizmos.color = generateSphereColor;

        var radius = GetGenerateSphereRadius();
        Debug.Log(radius);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
