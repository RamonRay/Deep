using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombEffectManager : MonoBehaviour {
    public static BombEffectManager instance;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float explosionTime=5f;
    [SerializeField] AudioClip explosionSFX;


    private void Awake()
    {
        if(instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("There are more than one Bomb Effect Manager in the scene");
        }
    }

    public static void Explode(Vector3 position)
    {
        if (instance != null)
        {
            instance.CreateExplosion(position);
        }
        else
        {
            Debug.LogError("Can't find Bomb Effect Manager in the scene");
        }
    }

    public void CreateExplosion(Vector3 position)
    {
        GameObject go = Instantiate(explosionPrefab, position, Quaternion.identity);
        AddSFX(explosionSFX);
        StartCoroutine(SelfDestroy(go));
    }

    IEnumerator SelfDestroy(GameObject go)
    {
        yield return new WaitForSeconds(explosionTime);
        Destroy(go);
        yield break;
    }


    // Update is called once per frame
    void Update () {
		
	}

    private void AddSFX(AudioClip clip)
    {
        AudioSource _as = gameObject.AddComponent<AudioSource>();
        _as.clip = clip;
        _as.Play();
        StartCoroutine(SelfDestroy(_as));
    }
    IEnumerator SelfDestroy(AudioSource _as)
    {
        yield return new WaitForSeconds(_as.clip.length);
        Destroy(_as);
        yield break;
    }
}

