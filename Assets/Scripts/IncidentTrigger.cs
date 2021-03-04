using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IncidentTrigger : MonoBehaviour {

    [SerializeField] UnityEvent incidentsTrigger;
    [SerializeField] float waitTime = 10f;
    [SerializeField] string tag;
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag==this.tag)
        {
            Debug.Log("Incident on " + gameObject.name + " triggered");
            StartCoroutine(InvokeAfterTime());
           
        }
        

    }

    IEnumerator InvokeAfterTime()
    {
        yield return new WaitForSeconds(waitTime);
        incidentsTrigger.Invoke();
        Destroy(this);
        yield break;
    }

}
