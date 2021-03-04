using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineGrabber : MonoBehaviour {
    [SerializeField] LayerMask grabLayer;
    [SerializeField] Transform controller;
    [SerializeField] Transform grabber;
    [SerializeField] Transform grabberReturn;
    [SerializeField] Transform presentPosition;
    [SerializeField] GameObject trailObject;
    [SerializeField] float maxDetectDistance=80f;
    [SerializeField] AudioClip grabSFX;

    private bool grabberAvailable = true;
    private RaycastHit hit;
	// Use this for initialization
	void Start () {
        trailObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		if(grabberAvailable&&Input.GetKeyDown(KeyCode.Space))
        {
            DetectTarget();
        }
	}

    private void DetectTarget()
    {
        if(Physics.Raycast(transform.position,controller.forward,out hit,maxDetectDistance,grabLayer))
        {
            //Target Detected
            Grab(hit.collider.gameObject);
            Debug.Log("Grabbing!");
        }
        else
        {
            Debug.Log("Nothing to grab.");
            if(Physics.Raycast(transform.position,controller.forward,out hit,maxDetectDistance))
            {
                NothingToGrab(hit.point);
            }
            else
            {
                NothingToGrab(transform.position + controller.forward * maxDetectDistance);
            }
        }
    }

    private void Grab(GameObject target)
    {
        grabberAvailable = false;
        StartCoroutine(GrabAndShow(target));
    }


    private void NothingToGrab(Vector3 destination)
    {

        grabberAvailable = false;
        GameObject _go = new GameObject();
        _go.transform.position = destination;
        StartCoroutine(GrabAndNoShow(_go));
    }
    IEnumerator GrabAndShow(GameObject target)
    {
        StartCoroutine(PlayClip(grabSFX));
        grabber.LookAt(target.transform.position);
        trailObject.SetActive(true);
        yield return StartCoroutine(GrabberMove(grabber,target.transform));
        //Play Hand Movement animation
        trailObject.SetActive(false);
        target.transform.parent = grabber;
        yield return StartCoroutine(GrabberMove(grabber,grabberReturn));
        target.transform.parent = transform.parent;
        //adjust the scale of the item
        Vector3 lossyScale = target.transform.lossyScale;
        Vector3 colliderSize = target.GetComponent<BoxCollider>().size;
        float scaleMultiplier = 3.5f / Mathf.Max(lossyScale.x * colliderSize.x, lossyScale.y * colliderSize.y, lossyScale.z * colliderSize.z);
        target.transform.localScale = target.transform.localScale * scaleMultiplier;
        //Move the Item up
        yield return StartCoroutine(GrabberMove(target.transform, presentPosition));
        CollectibleObject _co=null;
        //play the animation when collected
        try
        {
            _co = target.GetComponent<CollectibleObject>();
            _co.AnimTrigger();

        }
        catch
        {
            Debug.Log(target.name + "'s CollectibleObject script went wrong.");
        }
        if(_co!=null)
        {
            yield return new WaitForSeconds(1f+_co.animTime);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }
        //destroy target
        Destroy(target);
        grabberAvailable = true;
        yield break;
    }

    IEnumerator GrabAndNoShow(GameObject target)
    {
        StartCoroutine(PlayClip(grabSFX));
        //Move hands to destination
        grabber.LookAt(target.transform.position);
        trailObject.SetActive(true);
        yield return StartCoroutine(GrabberMove(grabber, target.transform));
        //Play Hand Movement animation
        //Hand return
        trailObject.SetActive(false);
        yield return StartCoroutine(GrabberMove(grabber, grabberReturn));
        Destroy(target);
        grabberAvailable = true;
        yield break;
    }

    IEnumerator GrabberMove(Transform movingObject,Transform destination)
    {
        Vector3 startingPoint = movingObject.position;
        float percentage = 0f;
        while(percentage<=1f)
        {
            movingObject.position = Vector3.Lerp(startingPoint, destination.position, percentage);
            percentage += 0.02f;
            yield return new WaitForSeconds(0.02f);
        }
        yield break;
    }


    IEnumerator PlayClip(AudioClip clip)
    {
        AudioSource _as = gameObject.AddComponent<AudioSource>();
        _as.clip = clip;
        _as.Play();
        yield return new WaitForSeconds(clip.length);
        Destroy(_as);
        yield break;
    }


}
