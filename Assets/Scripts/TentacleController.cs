using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TentacleController : MonoBehaviour, IKillable
{
    public enum State
    {
        Dormant,
        Respawning,
        Alive,
        Dying,
    }

    public Vector3 deltaRotationEuler;
    public float rotateTentacleDuration = 5f;
    public Transform tentacle;
    public Ease rotateInEase = Ease.InExpo;
    public Ease rotateOutEase = Ease.OutExpo;
    public State state { get; private set; }

    public AudioClip hitWindowClip;
    public float hitPlayTimeBeforeHit = 0.75f;

    public System.Action<TentacleController> onTentacleRespawnComplete;
    public System.Action<TentacleController> onTentacleKillComplete;

    private Collider[] m_tentacleColliders;
    // private Vector3 m_initRotationEuler;

    private AudioSource m_audioSource;

    public void Awake()
    {
        state = State.Dormant;
        m_audioSource = GetComponent<AudioSource>();
        m_tentacleColliders = tentacle.GetComponentsInChildren<Collider>();

        // m_initRotationEuler = transform.localRotation.eulerAngles;
        tentacle.gameObject.SetActive(false);

        onTentacleRespawnComplete += TryShakeSubmarine;
        // onTentacleRespawnComplete += PlayHitWindowClip;
    }

    public void RespawnTentacle()
    {
        if(state == State.Dormant)
        {
            state = State.Respawning;
            tentacle.gameObject.SetActive(true);
            foreach(var collider in m_tentacleColliders)
            {
                collider.isTrigger = true;
                collider.enabled = false;
            }

            StartCoroutine(RotateTentacleIE(deltaRotationEuler, rotateInEase, RespawnTentacleOnRotateComplete));

            float playHitDelay = Mathf.Clamp(rotateTentacleDuration - hitPlayTimeBeforeHit, 0f, Mathf.Infinity);
            StartCoroutine(DelayActionIE(playHitDelay, () => PlayHitWindowClip(this)));
        }
    }

    private IEnumerator DelayActionIE(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        if(action != null)
        {
            action();
        }
    }

    private void TryShakeSubmarine(TentacleController controller)
    {
        SubmarineShaker shaker = GameObject.FindObjectOfType<SubmarineShaker>();
        if(shaker)
        {
            shaker.Shake();
        }
    }

    private void PlayHitWindowClip(TentacleController controller)
    {
        if(hitWindowClip != null && m_audioSource != null)
        {
            m_audioSource.PlayOneShot(hitWindowClip);
        }
    }

    private void RespawnTentacleOnRotateComplete()
    {
        state = State.Alive;
        foreach(var collider in m_tentacleColliders)
        {
            collider.enabled = true;
        }

        if(onTentacleRespawnComplete != null)
        {
            onTentacleRespawnComplete(this);
        }


    }

    public void Kill()
    {
        KillTentacle();
    }

    public void KillTentacle()
    {
        if(state == State.Alive)
        {
            state = State.Dying;
            foreach(var collider in m_tentacleColliders)
            {
                collider.enabled = false;
            }

            TryShakeSubmarine(this);
            StartCoroutine(RotateTentacleIE(-deltaRotationEuler, rotateOutEase, KillTentacleOnRotateComplete));
        }
    }

    private void KillTentacleOnRotateComplete()
    {
        state = State.Dormant;
        if(onTentacleKillComplete != null)
        {
            onTentacleKillComplete(this);
        }
    }

    private IEnumerator RotateTentacleIE(Vector3 deltaRotation, Ease ease, System.Action onRotateComplete)
    {
        var tween = transform.DORotate(deltaRotation, rotateTentacleDuration, RotateMode.LocalAxisAdd);
        tween.SetEase(ease);
        yield return tween.WaitForCompletion();

        if(onRotateComplete != null)
        {
            onRotateComplete();
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            switch(state)
            {
                case State.Dormant:
                    RespawnTentacle();
                    break;
                case State.Alive:
                    KillTentacle();
                    break;
            }
        }
    }

}
