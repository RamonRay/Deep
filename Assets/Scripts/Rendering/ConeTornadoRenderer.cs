using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(ConeGenerator))]
public class ConeTornadoRenderer : MonoBehaviour {
    [Header("Cone Params Override")]
    public int sideSections = 32;
    public int spans = 32;
    private ConeGenerator m_ConeGenerator;
    private MeshFilter m_meshFilter;
    private Mesh m_tornadoMesh;

    private void GetRequiredComponents()
    {
        m_meshFilter = GetComponent<MeshFilter>();
        m_ConeGenerator = GetComponent<ConeGenerator>();
    }

    private void OnEnable()
    {
        GetRequiredComponents();

        m_ConeGenerator.onConeMeshUpdated += SyncMeshRenderer;
    }

    private void OnDisable()
    {
        m_ConeGenerator.onConeMeshUpdated -= SyncMeshRenderer;
    }

    private void OnValidate()
    {
        GetRequiredComponents();
        SyncMeshRenderer();
    }

    private void SyncMeshRenderer()
    {
        EditorUtilites.SafeEditorCall(UpdateTornadoMesh);
    }



    private void UpdateTornadoMesh()
    {
        var desc = m_ConeGenerator.GetConeDesc();
        desc.spans = spans;
        desc.sideSections = sideSections;
        desc.includeBase = false;
        m_tornadoMesh = ConeFactory.CreateCone(desc);
        m_meshFilter.mesh = m_tornadoMesh;
    }

}
