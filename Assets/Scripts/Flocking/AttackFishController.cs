using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttackFishController : MonoBehaviour, IKillable
{
	public FlockAgent m_flockAgent;
    public float detachRangeFactor = 1.5f;

    public float maxDurationBeforeAttack = 3f;
    public float attackMaxDist = 10f;
    public float attackTime = 4f;
    public float stopTime = 2f;
    public LayerMask windowLayer;

    private Transform m_chaseTarget;
    private ChaseBehavior m_chaseBehavior;
    private FlockController m_flockController;
    private Rigidbody m_rigidbody;
    private bool m_attackStopped = false;
    private Renderer m_renderer;
    // private
    private float m_targetRadius
    {
        get
        {
            return m_chaseBehavior.targetRadius;
        }
    }

    private void OnEnable()
    {
        m_flockAgent = GetComponent<FlockAgent>();
        m_flockAgent.onInitialized += Initialize;
        m_rigidbody = GetComponent<Rigidbody>();
        m_renderer = GetComponent<Renderer>();
    }

    private void Initialize(FlockController controller)
    {
        m_flockController = controller;
        m_chaseTarget = controller.chaseTarget;
        m_chaseBehavior = controller.GetFlockBehavior<ChaseBehavior>();
        Debug.Assert(m_chaseBehavior != null);

        StartCoroutine(DetachFromFlockIE());
    }

    private IEnumerator DetachFromFlockIE()
    {
        yield return new WaitUntil(() => {
            var selfToTarget = m_chaseTarget.position - transform.position;
            var dist = selfToTarget.magnitude;
            return dist <= m_targetRadius * detachRangeFactor;
        });

        m_flockController.RemoveAgent(m_flockAgent);

        Debug.Log("detached!");

        StartCoroutine(RepeatAttackIE());
    }

    Sequence attackSequence = null;

    private IEnumerator RepeatAttackIE()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, attackMaxDist, windowLayer, QueryTriggerInteraction.Collide))
        {
            var window = hit.collider.transform;

            transform.LookAt(hit.point, Vector3.up);


            m_rigidbody.isKinematic = true;
            // var colliders = GetComponentsInChildren<Collider>();
            // foreach(var collider in colliders)
            // {
            //     // collider.isTrigger = true;
            // }

            var submarineController = GameObject.FindObjectOfType<SubmarineController>();
            var fishParent = submarineController.transform;
            transform.parent = fishParent;

            var startLocalPos = transform.localPosition;
            var endLocalPos = fishParent.InverseTransformPoint(hit.point);

            yield return new WaitForSeconds(Random.value * maxDurationBeforeAttack);

            attackSequence = DOTween.Sequence();
            attackSequence.Append(transform.DOLocalMove(endLocalPos, attackTime).SetEase(Ease.InCubic));
            attackSequence.AppendInterval(stopTime);
            attackSequence.Append(transform.DOLocalMove(startLocalPos, attackTime).SetEase(Ease.InCubic));
            attackSequence.AppendInterval(stopTime);

            attackSequence.SetLoops(-1);

            // while(true)
            // {
            //     transform.LookAt(hit.point, Vector3.up);
            //     yield return new WaitForFixedUpdate();
            // }
        }
        else
        {
            isDead = true;
            var colliders = GetComponentsInChildren<Collider>();
            foreach(var collider in colliders)
            {
                collider.isTrigger = true;
            }

           StartCoroutine(FloatDeathIE());
        }



        yield return null;
    }

    public IEnumerator FloatDeathIE()
    {
        var tween = transform.DOMove(transform.position + new Vector3(0f, 100f, 0f), 20f)
                    .SetEase(Ease.Linear);
        yield return tween.WaitForCompletion();
        Destroy(gameObject);
    }


    public bool isDead = false;

    public void Kill()
    {
        if(!isDead)
        {
            isDead = true;
            StopAttack();
            m_rigidbody.isKinematic = true;
            StartCoroutine(FloatDeathIE());
        }
    }


    public void StopAttack()
    {
        if(!m_attackStopped)
        {
            m_attackStopped = true;

            if(attackSequence != null)
            {
                attackSequence.Pause();
                attackSequence.Kill(false);
                attackSequence = null;
            }
            StopAllCoroutines();

            transform.parent = null;
            m_rigidbody.isKinematic = false;

            StartCoroutine(DestroyWhenNotVisibleIE());
        }
    }

    private IEnumerator DestroyWhenNotVisibleIE()
    {
        yield return new WaitUntil(() => !m_renderer.isVisible);
        Destroy(gameObject);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.End))
        {
            StopAttack();
        }
    }
}
