using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class CoinGenerator : MonoBehaviour {

    public GameObject coinPrefab;
    public float width = 5f;
    public float height = 5f;

    [Range(0.1f, 10f)]
    public float radiusFactor = 1.0f;
    public int numCoins = 10;

    public bool alwaysDrawGizmos = false;

    private PoissonDiscSampler m_sampler;

    void DrawRangePlaneGizmo()
    {
        var planeColor = Color.yellow;
        planeColor.a = 0.5f;
        Gizmos.color = planeColor;

        Vector3 localSize = new Vector3(width, 0f, height);
        Vector3 worldSize = transform.TransformVector(localSize);
        Gizmos.DrawCube(transform.position, worldSize);
    }

    public void GenerateCoins()
    {
        CleanCurrentCoins();

        var coinRadius = GetCoinRadius();
        m_sampler = new PoissonDiscSampler(width, height, 2.0f * coinRadius * radiusFactor);

        int coinId = 0;
        Vector3 offset = new Vector3(-width / 2.0f, 0f, -height / 2.0f);
        foreach(Vector2 sample in m_sampler.Samples())
        {
            if(coinId >= numCoins)
            {
                break;
            }
            var localPos = new Vector3(sample.x, 0f, sample.y);
            localPos += offset;

            var worldPos = transform.TransformPoint(localPos);
            var coinGO = Instantiate(coinPrefab, worldPos, Quaternion.identity);
            var coin = coinGO.transform;
            coin.parent = transform;

            coinId++;
        }
    }

    public void CleanCurrentCoins()
    {
        Transform[] children = new Transform [transform.childCount];
        for(int i = 0; i < transform.childCount; ++i)
        {
            children[i] = transform.GetChild(i);
        }

        foreach(Transform child in children)
        {
#if UNITY_EDITOR
            if(!EditorApplication.isPlaying)
            {
                DestroyImmediate(child.gameObject);
                continue;
            }
#endif
            Destroy(child.gameObject);
        }
    }

    private float GetCoinRadius()
    {
        var renderer = coinPrefab.GetComponentInChildren<Renderer>();
        var extents = renderer.bounds.extents;  // in world space
        return Mathf.Max(extents.x, extents.z);
    }

    void OnDrawGizmosSelected()
    {
        if(!alwaysDrawGizmos)
        {
            DrawRangePlaneGizmo();
        }
    }

    void OnDrawGizmos()
    {
        if(alwaysDrawGizmos)
        {
            DrawRangePlaneGizmo();
        }
    }

    private void Start()
    {
        //GenerateCoins();
    }
}
