using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour {
    [SerializeField] UnityEvent onDeadAction;
    public void Dead()
    {
        onDeadAction.Invoke();
    }
}
