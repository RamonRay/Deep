using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SphereVolumetricLightDimmer : MonoBehaviour {

    public float startDimFactor = 1.1f;
    public float maxIntensity = 1.5f;
    private Light m_Light;
    private VolumetricLight m_volumetricLight;
	// Use this for initialization
	void Start () {
        m_Light = GetComponent<Light>();
        m_volumetricLight = GetComponent<VolumetricLight>();
        m_volumetricLight.lightIntensityOveride = maxIntensity;
	}

	// Update is called once per frame
	void Update ()
    {
        var startDimDistance = startDimFactor * m_Light.range;
        var endDimDistance = m_Light.range;

        var activeCameras = Camera.allCameras;
        var minDistance = Mathf.Infinity;
        foreach(var activeCam in activeCameras)
        {
            var toCam = activeCam.transform.position - transform.position;
            var dist = toCam.magnitude;
            if(dist < minDistance)
            {
                minDistance = dist;
            }
        }

        if(minDistance > startDimDistance)
        {
            m_volumetricLight.lightIntensityOveride = maxIntensity;
        }
        else
        {
            var factor = (minDistance - endDimDistance) / (startDimDistance - endDimDistance);
            factor = Mathf.Clamp01(factor);
            m_volumetricLight.lightIntensityOveride = factor * maxIntensity;
        }
	}
}
