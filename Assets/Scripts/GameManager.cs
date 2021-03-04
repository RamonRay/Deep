using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    [SerializeField] Image leftCoin, rightCoin;
    private int scoreNum = 0;
    private int ranking = 0;
    private LeaderBoard leaderBoard;
    private bool isEnd = false;

	// Use this for initialization
	void Awake () {
		if(instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(gameObject);
        leaderBoard = SaveLeaderBoard.Load();
	}

	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.P))
        {
            GameWin();
        }
	}


    public void GameWin()
    {
        Debug.Log("game win!");
        BGMController.instance.NextBGM();
        SubmarineController.instance.EmergencyFloat(15f);
        StartCoroutine(AfterWinning());
    }

    IEnumerator AfterWinning()
    {
        yield return new WaitForSeconds(5f);
        GameEnd();
        yield break;
    }

    public void GameEnd()
    {
        if (isEnd)
        { return; }
        else
        {
            isEnd = true;
        }
        scoreNum = GetScore();
        Score score = new Score("", scoreNum);
        ranking=leaderBoard.AddScore(score);
        SaveLeaderBoard.Save(leaderBoard);
        StartCoroutine(UITurnTransparent(1f));
        StartCoroutine(GameEndAndLoad());;
    }
    IEnumerator GameEndAndLoad()
    {
        yield return new WaitForSeconds(5f);
        AutoExposureController.instance.FadeToTarget();
        yield return new WaitForSeconds(5.1f);
        AsyncOperation operation = SceneManager.LoadSceneAsync("GameEnd");
        while(!operation.isDone)
        {
            yield return new WaitForSeconds(1f);
        }
        AutoExposureController.instance.FadeToOriginal();
        yield break;
    }

    IEnumerator UITurnTransparent(float time)
    {
        float a = 255f;
        float timeStep = 0.1f;
        Color colorLeft = leftCoin.color;
        Color colorRight = rightCoin.color;
        float startTime = Time.time;
        float alphaStep = 255f / time*timeStep;

        while(Time.time<startTime+time)
        {
            a -= alphaStep;
            colorLeft.a = (int)a;
            colorRight.a = (int)a;
            leftCoin.color = colorLeft;
            rightCoin.color = colorRight;
            yield return new WaitForSeconds(timeStep);
        }
        yield break;
    }

    public int GetScore()
    {
        int _score=0;
        SubmarineVaccumCollector[] _collectors = GameObject.FindWithTag("Submarine").GetComponentsInChildren<SubmarineVaccumCollector>();
        foreach(var collector in _collectors)
        {
            _score += collector.score;
        }
        return _score;
    }
    public void TypeName(string name)
    {
        leaderBoard.GetRankingScore(ranking).name = name;
        SaveLeaderBoard.Save(leaderBoard);
    }

    public Score RankingScore(int ranking)
    {
        if(leaderBoard.scores.Count<ranking)
        {
            return null;
        }
        else
        {
            return leaderBoard.GetRankingScore(ranking);
        }
    }

    public int GetRanking()
    {
        return ranking;
    }
}
