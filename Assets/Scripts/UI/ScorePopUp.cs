using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScorePopUp : MonoBehaviour {
    [SerializeField] private GameObject PopUpPrefab;
    [SerializeField] private GameObject DamagePrefab;
    [SerializeField] float showTime;
    [SerializeField] Vector2 verticalRange = new Vector2(1f, 10f);
    [SerializeField] float horizontalOffset = -3f;
    [SerializeField] float floatintgHeight = 10f;
    private Rect rect;
    private float width;
    private float height;
    private void Start()
    {
        rect = GetComponent<RectTransform>().rect;
        width = rect.width / 4f;
        height = rect.height / 4f;
        rect = PopUpPrefab.GetComponent<RectTransform>().rect;
        height = height + rect.height/2f;
    }


    public void Create(int score)
    {
        float heightDifference = Random.Range(verticalRange.x, verticalRange.y);
        Vector3 offset = new Vector3(Random.Range(-width+horizontalOffset, width+horizontalOffset), -height - heightDifference, 0f);
        GameObject popUpGo = Instantiate(score>=0?PopUpPrefab:DamagePrefab, transform.position+offset, Quaternion.identity);
        popUpGo.transform.parent = transform;
        TextMeshProUGUI text = popUpGo.GetComponent<TextMeshProUGUI>();
        if (score >= 0)
            text.SetText("+" + score.ToString());
        else
            text.SetText(score.ToString());
        text.fontSize = 30f * (1f+(float)(Mathf.Abs(score) - 1) / 400f);
        StartCoroutine(GraduallyDisappear(popUpGo,heightDifference));
        
    }

    IEnumerator GraduallyDisappear(GameObject go,float heightDifference)
    {
        float startTime = Time.time;
        float timeStep = 0.04f;
        float alphaStep = timeStep / showTime;
        float heightStep = Random.Range(0.9f*heightDifference,heightDifference) / showTime*timeStep;
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        Color color = text.color;
        while (Time.time<startTime+showTime)
        {
            go.transform.Translate(Vector3.up * heightStep);
            color.a -= alphaStep;
            text.color = color;
            yield return new WaitForSeconds(timeStep);
        }
        Destroy(go);
        
    }
}
