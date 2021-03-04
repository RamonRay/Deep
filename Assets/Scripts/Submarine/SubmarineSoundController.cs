using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineSoundController : MonoBehaviour {
    [SerializeField] AudioClip engineSound;
    [SerializeField] float engineDieOutTime = 1f;
    [SerializeField] float engineMaxVolumnTime = 1f;
    private AudioSource engineAudioSource;
    private float maxVelocity;
    private float _velocity=0f;
    public float velocity
    {
        get
        {
            return _velocity;
        }
        set
        {
            _velocity = value;
            SetPitch(_velocity);
        }
    }
    private float _power;
    public float power
    {
        get
        {
            return _power;
        }
        set
        {
            _power = value;
            SetVolumn(_power);
        }
    }
    private bool _isStart = false;
    public bool isStart
    {
        get
        {
            return _isStart;
        }
        set
        {
            bool oldIsStart = _isStart;
            _isStart = value;
            if(!oldIsStart && _isStart)
            {
                StopAllCoroutines();
                if(engineAudioSource.isPlaying)
                {
                    return;
                }
                engineAudioSource.time = Random.Range(0f, engineSound.length);
                engineAudioSource.Play();
                return;
            }
            if (oldIsStart&&!_isStart)
            {
                StartCoroutine(DieOut());
                return;
            }
        }
    }
    private float volumn=0f;
    private float volumnMaxStep;
    private void Awake()
    {
        engineAudioSource = gameObject.AddComponent<AudioSource>();
        engineAudioSource.clip = engineSound;
        engineAudioSource.loop = true;
        maxVelocity = GetComponent<SubmarineController>().maxVelocity;
        volumnMaxStep = 1f / engineMaxVolumnTime * Time.fixedDeltaTime;
    }

    private void SetPitch(float _v)
    {
        engineAudioSource.pitch = Mathf.Clamp( _v / maxVelocity, 0.1f, 1f);
    }

    private void SetVolumn(float power)
    {
        if(Mathf.Abs(volumn)<=Mathf.Abs(power))
        {
            volumn = Mathf.Clamp(volumn + power * volumnMaxStep,-Mathf.Abs(power), Mathf.Abs(power));
        }
        else
        {
            if(volumn<0f)
            {
                volumn += volumnMaxStep;
                if(volumn>0f)
                {
                    volumn = 0f;
                }
            }
            else if(volumn>0f)
            {
                volumn -= volumnMaxStep;
                if(volumn<0f)
                {
                    volumn = 0f;
                }
            }
        }
        engineAudioSource.volume = Mathf.Abs(volumn);
    }

    IEnumerator DieOut()
    {
        float startTime = Time.time;
        float currentV = velocity;
        float currenPower = power;
        while(Time.time<startTime+engineDieOutTime)
        {
            float _percentage = (Time.time - startTime) / engineDieOutTime;
            power = 0f;
            velocity = Mathf.Lerp(0, currentV, 1 - _percentage);
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        engineAudioSource.Pause();
        yield break;
    }
}
