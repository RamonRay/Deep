using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterRandomHowl : MonoBehaviour {
    public static MonsterRandomHowl instance;

    [SerializeField] AudioClip[] monsterSFXs;
    [SerializeField] Vector2 timeInterval = new Vector2(5f, 8f);
    private AudioSource audioSource;
    private Coroutine coro;
	// Use this for initialization
	void Start ()
    {
        if(instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    private void playRandomHowl()
    {
        audioSource.clip = monsterSFXs[Random.Range(0, monsterSFXs.Length)];
        audioSource.Play();
    }

    public void StartMonsterHowl()
    {
        coro = StartCoroutine(PlayWithInterval());
    }
    public void StopMonsterHowl()
    {
        audioSource.Stop();
        StopCoroutine(coro);
    }

    IEnumerator PlayWithInterval()
    {
        while (true)
        {
            playRandomHowl();
            yield return new WaitForSeconds(Random.Range(timeInterval.x, timeInterval.y));
        }
    }

}
