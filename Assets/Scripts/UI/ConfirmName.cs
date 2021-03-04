using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmName : MonoBehaviour {
    [SerializeField] Text text;
    [SerializeField] GameObject inputBoxPanel;
    [SerializeField] GameObject rankings;
    public void Confirm()
    {
        GameManager.instance.TypeName(text.text);
        rankings.SetActive(true);
        inputBoxPanel.SetActive(false);
    }
}
