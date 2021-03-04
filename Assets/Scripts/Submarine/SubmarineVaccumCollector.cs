using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SubmarineVaccumCollector : MonoBehaviour {
    public static SubmarineVaccumCollector[] collectors = new SubmarineVaccumCollector[2];

    [SerializeField] Text scoreText;
    [SerializeField] GameObject explosion;
    [SerializeField] ScorePopUp popUp;
    private int handInt;
    private int _score;
    public int score
    {
        get
        {
            return _score;
        }
        set
        {
            _score = value;
            scoreText.text =  _score.ToString();
        }
    }
    private SubmarineVacuum[] sv;
	// Use this for initialization
	void Start () {
        sv = transform.parent.parent.parent.gameObject.GetComponentsInChildren<SubmarineVacuum>();
        if(GetComponentInParent<SubmarineVacuum>().handInt==0)
        {
            collectors[0] = this;
            handInt = 0;
        }
        else
        {
            collectors[1] = this;
            handInt = 1;
        }
        score = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    private void OnTriggerEnter(Collider other)
    {
        VacuumedObject _vo = other.gameObject.GetComponent<VacuumedObject>();
        if(_vo!=null)
        {
            foreach(SubmarineVacuum _s in sv)
            {
                try
                { _s.UnRegisterObject(other.gameObject); }
                catch
                {
                    Debug.Log("Not found");
                }
            }
            score += _vo.score;
            popUp.Create(_vo.score);
            if(score<0)
            {
                score = 0;
            }
            if(_vo.damage)
            {
                GetComponentInParent<SubmarineVacuum>().Damaged();
                BombEffectManager.Explode(other.transform.position);
                SubmarineShaker.instance.Shake();
                //go.transform.parent = transform;
                //StartCoroutine(DestroyAfterTime(go, 5f));
            }
            SubmarineVacuumSound.instance.PlaySFX(_vo.soundIndex);

            Destroy(other.gameObject);
        }
    }

    IEnumerator DestroyAfterTime(GameObject go, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(go);
        yield break;
    }

    public void Damage(int scoreDamage)
    {
        score -= scoreDamage;
        if(score<0)
        {
            score = 0;
        }
        popUp.Create(-scoreDamage);
    }
}
