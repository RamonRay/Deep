using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineVacuumSound : MonoBehaviour {
    [SerializeField] AudioClip[] collectedSoundFX;
    [Range(0f, 1f)]
    [SerializeField] float[] soundPitch;
    public static SubmarineVacuumSound instance;
    private AudioSource[] audioSources;
	// Use this for initialization
	void Start () {
		if(instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        Init();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlaySFX(int i)
    {
        if(!audioSources[i].isPlaying)
        {
            audioSources[i].Play();
        }
        else
        {
            audioSources[i].time = 0f;
        }
    }

    private void Init()
    {
        audioSources = new AudioSource[collectedSoundFX.Length];

        for (int index = 0; index < collectedSoundFX.Length; index++)
        {
            audioSources[index] = gameObject.AddComponent<AudioSource>();
            audioSources[index].clip = collectedSoundFX[index];
            try
            {
                audioSources[index].pitch = soundPitch[index];
            }
            catch
            {
                //Do nothing
            }
        }
    }




}
