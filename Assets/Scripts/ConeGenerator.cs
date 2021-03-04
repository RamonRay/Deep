using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ConeGenerator : MonoBehaviour {

    [Header("Cone Properties")]
    public int sideSections = 10;
	public float radiusTop = 0f;
	public float radiusBottom = 1f;
	public float height = 1f;
    public int spans = 1;

    public bool useConeAngle = false;
    [Range(0f, 180f)]
    public float coneAngle = 45f;

    [Header("Sync Component Settings")]
    public bool syncMeshFilter = false;
    public bool syncMeshCollider = false;

    [Header("Gizmos Settings")]
    public bool alwaysDrawGizmos = false;

    public Mesh coneMesh { get; private set; }

    public System.Action onConeMeshUpdated;

    private MeshFilter m_MeshFilter;
    private MeshCollider m_MeshCollider;


    public ConeFactory.ConeDesc GetConeDesc()
    {
        var desc = new ConeFactory.ConeDesc();
        desc.sideSections = sideSections;
        desc.radiusTop = radiusTop;
        desc.radiusBottom = radiusBottom;
        desc.height = height;
        desc.spans = spans;
        desc.useConeAngle = useConeAngle;
        desc.coneAngle = coneAngle;
        desc.includeBase = true;
        return desc;
    }

    void UpdateConeMesh()
    {
        if(coneMesh == null)
        {
            coneMesh = new Mesh();
        }
        else
        {
            coneMesh.Clear();
        }

        if(useConeAngle)
        {
            var angleRad = coneAngle * Mathf.Deg2Rad;
            radiusBottom = radiusTop + height * Mathf.Tan(angleRad);
        }

        var desc = GetConeDesc();
        coneMesh = ConeFactory.CreateCone(desc);

        if(onConeMeshUpdated != null)  onConeMeshUpdated();
    }

    void SyncComponents()
    {
        // Debug.Log("sync");
        if(m_MeshCollider == null)
        {
            m_MeshCollider = GetComponent<MeshCollider>();
        }
        if(m_MeshFilter == null)
        {
            m_MeshFilter = GetComponent<MeshFilter>();
        }


        if(syncMeshFilter)
        {
            m_MeshFilter.sharedMesh = coneMesh;
        }
        if(syncMeshCollider)
        {
            m_MeshCollider.sharedMesh = coneMesh;
        }
    }

    void DrawConeGizmo()
    {
        var coneColor = Color.yellow;
        coneColor.a = 0.5f;
        Gizmos.color = coneColor;

        Gizmos.DrawMesh(coneMesh, 0, transform.position, transform.rotation, transform.lossyScale);
    }

    #region Unity MonoBehavior Messages
	void OnEnable ()
    {
        UpdateConeMesh();
        EditorUtilites.SafeEditorCall(SyncComponents);
        // Debug.Log("enable");
	}

    void OnDisable()
    {

    }

    void OnValidate()
    {
        if(sideSections < 3)
        {
            sideSections = 3;
        }
        if(spans < 1)
        {
            spans = 1;
        }

        UpdateConeMesh();
        EditorUtilites.SafeEditorCall(SyncComponents);
    }

    void OnDrawGizmos()
    {
        if(alwaysDrawGizmos)
        {
            DrawConeGizmo();
        }
    }

    void OnDrawGizmosSelected()
    {
        if(!alwaysDrawGizmos)
        {
            DrawConeGizmo();
        }
    }

	#endregion
}