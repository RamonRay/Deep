using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TentacleDirector : MonoBehaviour
{
    public List<TentacleGroup> tentacleGroups;
    public float waitBetweenGroups = 1.0f;
    public float perTentacleRespawnVariance = 1.0f;
    public Action onAllTentacleGroupsKilled;


    public bool tentacleRespawned {get; private set;}
    private List<TentacleController> m_currentAliveTentacles;
    private WaitUntil m_waitUntilNoAliveTentacles;

    [System.Serializable]
    public class TentacleGroup
    {
        public List<TentacleController> tentacles;
    }

    private void ForAllTentacleControllers(Action<TentacleController> tentacleCallback)
    {
        foreach(var tentacleGroup in tentacleGroups)
        {
            foreach(var tentacle in tentacleGroup.tentacles)
            {
                if(tentacle != null && tentacleCallback != null)
                {
                    tentacleCallback(tentacle);
                }
            }
        }
    }

    private void OnEnable()
    {
        m_currentAliveTentacles = new List<TentacleController>();
        m_waitUntilNoAliveTentacles = new WaitUntil(() => m_currentAliveTentacles.Count == 0);

        onAllTentacleGroupsKilled += GameWin;

        tentacleRespawned = false;

        ForAllTentacleControllers((TentacleController tentacle) => {
            tentacle.enabled = false;
        });
    }

    private void GameWin()
    {
        GameManager.instance.GameWin();
    }

    private void OnDisable()
    {
        onAllTentacleGroupsKilled -= GameWin;
    }

    public void RespawnTentacleGroups()
    {
        if(!tentacleRespawned)
        {
            tentacleRespawned = true;
            StartCoroutine(RespawnTentacleGroupsIE());
        }
    }

    private IEnumerator RespawnTentacleIE(TentacleController tentacle, float delay)
    {
        yield return new WaitForSeconds(delay);
        tentacle.RespawnTentacle();
    }

    private IEnumerator RespawnTentacleGroupsIE()
    {
        foreach(var tentacleGroup in tentacleGroups)
        {
            var tentacles = tentacleGroup.tentacles;

            m_currentAliveTentacles.Clear();
            m_currentAliveTentacles.AddRange(tentacles);

            foreach(var tentacle in tentacles)
            {
                tentacle.enabled = true;
                StartCoroutine(RespawnTentacleIE(tentacle, UnityEngine.Random.value * perTentacleRespawnVariance));
                tentacle.onTentacleKillComplete += TentacleKilledCallback;
            }

            yield return m_waitUntilNoAliveTentacles;
            yield return new WaitForSeconds(waitBetweenGroups);
        }


        if(onAllTentacleGroupsKilled != null)
        {
            onAllTentacleGroupsKilled();
        }
    }

    private void TentacleKilledCallback(TentacleController tentacle)
    {
        Debug.Assert(m_currentAliveTentacles.Contains(tentacle));
        m_currentAliveTentacles.Remove(tentacle);
        tentacle.enabled = false;
        tentacle.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            RespawnTentacleGroups();
        }
    }

}
