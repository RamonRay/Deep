using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombInstantiation : MonoBehaviour {
    public static BombInstantiation instance;

    [SerializeField] GameObject bombPrefab;
    [SerializeField] Transform submarine;
    [SerializeField] int numberofBombs=12;
    [SerializeField] float bombDistance=30f;
    [SerializeField] float timeBeforeNewBomb=3f;
    [SerializeField] float verticalDistance = 10f;
    [SerializeField] float verticalDropTime = 5f;

    private Vector3[] positions;
    private GameObject[] bombs;
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

        positions = new Vector3[numberofBombs];
        for (int index = 0; index < numberofBombs; index++)
        {
            positions[index] = Quaternion.Euler(0f, Mathf.Lerp(0f, 360f, (float)index / (float)(numberofBombs + 1)), 0f) * Vector3.forward * bombDistance;
            positions[index].y += verticalDistance;
        }


        bombs = new GameObject[numberofBombs];
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.U))
        {
            CreateAll();
        }
        if(Input.GetKeyDown(KeyCode.I))
        {
            DestroyAll();
        }
	}

    private GameObject CreateBomb(int index)
    {
        //Check if index is within range
        if(index>=numberofBombs||index<0)
        {
            return null;
        }

        GameObject go = Instantiate(bombPrefab, submarine.position + positions[index], Quaternion.identity);
        BombRegenerateTrigger _bgt = go.AddComponent<BombRegenerateTrigger>();
        _bgt.bombIndex = index;
        StartCoroutine(MoveDownBomb(go));
        return go;
    }

    //Function called when former bomb is destroyed for whatever reason.
    public void CreateBombWait(int index)
    {
        StartCoroutine(CreateBombAfterTime(index, timeBeforeNewBomb));
    }

    IEnumerator CreateBombAfterTime(int index,float time)
    {
        yield return new WaitForSeconds(time);
        bombs[index]=CreateBomb(index);
        yield break;
    }
    // Call this function when it's time to instantiate all the bombs
    public void CreateAll()
    {
        for (int index = 0; index < numberofBombs; index++)
        {
            bombs[index]=CreateBomb(index);
        }
        Debug.Log("Bombs are instantiated");
    }

    //Called when no new bombs needs to be instantiated.
    public void DestroyAll()
    {
        for (int index = 0; index < numberofBombs; index++)
        {
            try
            {
                bombs[index].GetComponent<BombRegenerateTrigger>().instantiateOnDestroy = false;
            }
            catch
            {
                Debug.LogError("Bombs don't have required regenerate component");
            }
        }
    }

    IEnumerator MoveDownBomb(GameObject bomb)
    {
        float startTime = Time.time;
        float timeStep = 0.02f;
        float dropStep = verticalDistance / verticalDropTime*timeStep;
        while (Time.time<startTime+verticalDropTime)
        {
            float rate =2f* (Time.time - startTime) / verticalDropTime-2f;
            bomb.transform.Translate(new Vector3(0f, rate*dropStep,0f));
            yield return new WaitForSeconds(timeStep);
        }
        yield break;
    }



}
