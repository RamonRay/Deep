using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class LeaderBoard {
    public List<Score> scores;
    public LeaderBoard()
    {
        scores = new List<Score>();
    }

    public int AddScore(Score score)
    {
        Debug.Log(scores);
        if (scores.Count == 0)
        {
            scores.Add(score);
            return 1;
        }
        else
        {
            for (int index = 0; index < scores.Count; index++)
            {
                if (score.score >=scores[index].score)
                {
                    scores.Insert(index, score);
                    return index + 1;
                }
            }
            scores.Add(score);
            return scores.Count + 1;
        }
    }

    public Score GetRankingScore(int ranking)
    {
        if(ranking>scores.Count&&ranking<1)
        {
            return null;
        }
        else
        {
            return scores[ranking - 1];
        }
    }



}

[System.Serializable]
public class Score
{
    public int score;
    public string name;

    public Score(string name, int score)
    {
        this.name = name;
        this.score = score;
    }
}
