using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Valve.VR;

public class SubmarineVacuum : MonoBehaviour {
    [SerializeField] float vacuumForce=1f;
    [SerializeField] SteamVR_Action_Boolean vacuumTrigger;
    [SerializeField] SteamVR_Input_Sources hand;
    [SerializeField] SteamVR_Action_Vibration hapticAction;
    [SerializeField] Vector2 cutOffValueRange=new Vector2(0.2f,0.5f);
    [SerializeField] float tornadoShowUpTime = 1f;
    [SerializeField] float repairTime;
    [SerializeField] AudioSource vacuumStart;
    [SerializeField] AudioSource vacuumLoop;
    public int handInt;//Left 0 Right 1

    public static SubmarineVacuum[] vacuums=new SubmarineVacuum[2];
    

    private Action<Transform,float> onVacuum;
    private Action<int> onVacuumState;
    private Action<int> onUnVacuumState;
    private Material mat;
    private float currentCutOff;
    private bool isWorking = true;
    private bool isPlayingAudio= false;
    private Coroutine audioTransit;
    private Coroutine lightFlicker;
    private Coroutine autoRepair;

	// Use this for initialization
	void Awake () {
        /*
		switch(hand)
        {
            case SteamVR_Input_Sources.LeftHand:
                handInt = 0;
                vacuums[0] = this;
                break;
            case SteamVR_Input_Sources.RightHand:
                handInt = 1;
                vacuums[1] = this;
                break;
            default:
                handInt = 0;
                break;
        }
        */
        MeshCollider mc = GetComponent<MeshCollider>();
        mc.sharedMesh = GetComponent<ConeGenerator>().coneMesh;
        mat = GetComponent<MeshRenderer>().material;
        currentCutOff = cutOffValueRange.y;
        SetTornadoMat(currentCutOff);
	}

    private void FixedUpdate()
    {
        if (isWorking&& vacuumTrigger.GetLastState(hand))// Input.GetKey(KeyCode.K))//
        {
            UpdateTornadoMat(true);
            AudioPlay(true);
            if (onVacuum != null)
            {
                try
                {
                    onVacuum(transform, vacuumForce);
                    onVacuumState(handInt);
                }
                catch
                {
                    onVacuum = null;
                    onVacuumState = null;
                }
            }

        }
        else 
        {
            AudioPlay(false);
            UpdateTornadoMat(false);
            if (isWorking && onUnVacuumState != null)
            {
                try
                { onUnVacuumState(handInt); }
                catch
                {
                    onUnVacuumState = null;
                }
            }

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        VacuumedObject _vo = other.gameObject.GetComponent<VacuumedObject>();
        if(_vo!=null)
        {
            RegisterObject(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        VacuumedObject _vo = other.gameObject.GetComponent<VacuumedObject>();
        if (_vo != null)
        {
            UnRegisterObject(other.gameObject);
        }
    }


    private void RegisterObject(GameObject go)
    {
        VacuumedObject _vo = go.GetComponent<VacuumedObject>();
        onVacuum += _vo.Vacuumed;
        onVacuumState += _vo.OnVacuumed;
        onUnVacuumState += _vo.OnUnVacuumed;
    }
    public void UnRegisterObject(GameObject go)
    {
        VacuumedObject _vo = go.GetComponent<VacuumedObject>();
        _vo.OnUnVacuumed(handInt);
        onVacuum -= _vo.Vacuumed;
        onVacuumState -= _vo.OnVacuumed;
        onUnVacuumState -= _vo.OnUnVacuumed;
    }

    private void SetTornadoMat(float cutOff)
    {
        mat.SetFloat("_CutOffValue", cutOff);
    }

    private void UpdateTornadoMat(bool _isOn)
    {
       if(_isOn)
        {
            if(currentCutOff>cutOffValueRange.x)
            {
                currentCutOff -= Time.fixedDeltaTime / tornadoShowUpTime;
                SetTornadoMat(currentCutOff);
            }
        }
       else
        {
            if (currentCutOff < cutOffValueRange.y)
            {
                currentCutOff += Time.fixedDeltaTime / tornadoShowUpTime;
                SetTornadoMat(currentCutOff);
            }
        }
    }

    private void AudioPlay(bool play)
    {
        if(play&&!isPlayingAudio)
        {
            isPlayingAudio = true;
            vacuumStart.Play();
            audioTransit = StartCoroutine(AudioTransit());
        }
        else if(!play&&isPlayingAudio)
        {
            StopCoroutine(audioTransit);
            isPlayingAudio = false;
            vacuumStart.Stop();
            vacuumLoop.Stop();

        }
    }
    IEnumerator AudioTransit()
    {
        yield return new WaitForSeconds(vacuumStart.clip.length-0.2f);
        vacuumStart.Stop();
        vacuumLoop.Play();
        yield break;
    }

    public void Damaged()
    {
        try
        {
            StopCoroutine(autoRepair);
            StopCoroutine(lightFlicker);
        }
        catch
        {
            Debug.Log("No coroutine Running");
        }
        isWorking = false;
        SetTornadoMat(cutOffValueRange.y);
        autoRepair = StartCoroutine(AutoRepair());
        lightFlicker = StartCoroutine(LightFlicker(false));
        hapticAction.Execute(0f, 1f, 200f, 1f, hand);
    }
    IEnumerator AutoRepair()
    {
        yield return new WaitForSeconds(repairTime);
        Repaired();
        yield break;
    }

    public void Repaired()
    {
        StopCoroutine(lightFlicker);
        isWorking = true;
        lightFlicker = StartCoroutine(LightFlicker(true));
    }

    IEnumerator LightFlicker(bool isOn)
    {
        bool currentOn = false;
        Light spotLight = GetComponent<Light>();
        float startTime = Time.time;
        while(Time.time<startTime+1f)
        {
            spotLight.enabled = currentOn;
            currentOn = !currentOn;
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f,0.1f));
        }
        spotLight.enabled = isOn;
        yield break;
    }
}
