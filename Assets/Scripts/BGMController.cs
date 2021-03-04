using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMController : MonoBehaviour {
    public static BGMController instance;


    [SerializeField] AudioClip[] bgmBeginnings;
    [SerializeField] AudioClip[] bgmLoops;
    [SerializeField] float[] transitionTimes;
    AudioSource oldBGM;
    AudioSource currentBGM;

    private int index = 0;
	// Use this for initialization
	void Start () {
		if(instance==null)
        {
            instance = this;
        }
        else
        {
            //
        }

        currentBGM = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Y))
        {
            NextBGM();
        }
	}

    public void NextBGM()
    {
        try
        {
            StartFade(bgmBeginnings[index], bgmLoops[index], transitionTimes[index]);
        }
        catch
        {
            index = -1;
        }
        index++;
    }
    private void StartFade(AudioClip clip,AudioClip loopClip, float transitionTime)
    {
        StopAllCoroutines();

        if(oldBGM!=null)
        {
            Destroy(oldBGM);
        }

        StartCoroutine(FadeTransit(clip, loopClip,transitionTime));
    }

    IEnumerator FadeTransit(AudioClip newClip, AudioClip loopClip, float transitionTime)
    {
        float timeStep = 0.02f;
        float initialVolumn = currentBGM.volume;
        oldBGM = currentBGM;
        currentBGM = AddAudio(newClip);
        float startTime = Time.time;
        while (Time.time < startTime + transitionTime)
        {
            oldBGM.volume = 1f - (Time.time - startTime) / transitionTime;
            currentBGM.volume = (Time.time - startTime) / transitionTime;
            yield return new WaitForSeconds(timeStep);
        }
        Destroy(oldBGM);
        yield return new WaitForSeconds(newClip.length - transitionTime);
        currentBGM.clip = loopClip;
        currentBGM.Play();
        yield break;
    }

    private AudioSource AddAudio(AudioClip clip)
    {
        AudioSource _as = gameObject.AddComponent<AudioSource>();
        _as.clip = clip;
        _as.loop = true;
        _as.Play();
        return _as;
    }

}
