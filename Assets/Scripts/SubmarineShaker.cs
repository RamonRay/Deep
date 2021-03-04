using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SubmarineShaker : MonoBehaviour {

    public static SubmarineShaker instance;

    public float duration = 1.0f;
    public Vector3 strength = new Vector3(1.0f, 1.0f, 1.0f);
    public int vibrato = 10;

    [SerializeField]
    private float shakeTimeInterval = 1f;

    [Range(0f, 180f)]
    public float randomness = 90f;

    public bool isShaking { get; private set; }

    private float nextShakeTime = 0f;

	public void Shake()
    {
        Shake(duration, strength, vibrato, randomness);
    }

    public void Shake(float _duration, Vector3 _strength, int _vibrato, float _randomness)
    {
        if(!isShaking&&Time.time>nextShakeTime)
        {
            isShaking = true;
            nextShakeTime = Time.time + shakeTimeInterval;
            if(SubmarineFloorControl.instance != null)
            {
                try
                {
                    SubmarineFloorControl.instance.Shake();
                }
                catch
                {
                    Debug.LogWarning("SubmarineFloorControl Shake() failed.");
                }
            }
            var localStrength = transform.TransformVector(_strength);
            transform.DOShakePosition(_duration, localStrength, _vibrato, _randomness)
                     .OnComplete(() => isShaking = false);
        }
    }

    private void Awake()
    {
        if(instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
}
