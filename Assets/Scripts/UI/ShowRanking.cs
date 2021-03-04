using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowRanking : MonoBehaviour {
    [SerializeField] int ranking;
    [SerializeField] Text rankingText;
    [SerializeField] Text nameText;
    [SerializeField] Text scoreText;
	// Use this for initialization
	void Start () {
        UpdateInfo();	
	}
    public void UpdateInfo()
    {
        if(ranking==-1)
        {
            ranking = GameManager.instance.GetRanking();
        }
        Score score = GameManager.instance.RankingScore(ranking);
        if(score!=null)
        { 
            rankingText.text = ranking.ToString();
            nameText.text = score.name;
            scoreText.text = score.score.ToString();
        }
        else
        {
            rankingText.text = "";
            nameText.text = "";
            scoreText.text = "";
        }
    }
}
