using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using DG.Tweening;

public class AutoExposureController : MonoBehaviour
{
    public static AutoExposureController instance;
    public float targetCompensation = 40f;
    public float duration = 5f;
    PostProcessVolume m_Volume;
    AutoExposure m_AutoExposure;

    public bool isFading { get; private set; }

	// Use this for initialization
	void Start () {
        if(instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        m_AutoExposure = ScriptableObject.CreateInstance<AutoExposure>();
        m_AutoExposure.enabled.Override(true);

        m_Volume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, m_AutoExposure);
        m_Volume.weight = 0f;
	}


    public bool FadeToTarget()
    {
        if(isFading)
        {
            return false;
        }
        m_AutoExposure.keyValue.Override(targetCompensation);
        isFading = true;
        DOTween.Sequence()
            .Append(DOTween.To(() => m_Volume.weight, x => m_Volume.weight = x, 1f, duration))
            .OnComplete(() => isFading = false);

        return true;
    }

    public bool FadeToOriginal()
    {
        if(isFading)
        {
            return false;
        }
        DOTween.Sequence()
            .Append(DOTween.To(() => m_Volume.weight, x => m_Volume.weight = x, 0f, duration))
            .OnComplete(() => isFading = false);

        return true;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.PageUp))
        {
            Debug.Assert(FadeToTarget());
        }
        if(Input.GetKeyDown(KeyCode.PageDown))
        {
            Debug.Assert(FadeToOriginal());
        }
    }

    void OnDestroy()
    {
        RuntimeUtilities.DestroyVolume(m_Volume, true, true);
    }

}
