using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleObject : MonoBehaviour {
    [SerializeField] string trigger;
    [SerializeField] public float animTime;
    private Animator anim;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void AnimTrigger()
    {
        anim.SetTrigger(trigger);
    }
}
