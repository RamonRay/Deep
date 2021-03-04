using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class PhotonGridGenerator : MonoBehaviour {

    public Light photonLight;
    public Camera photonCamera;
    public float farDistance = 100f;

    public float photonIntensity = 1.0f;
    [Range(0, 0.1f)]
    public float oceanSurfaceAttenuation = 1.0f;
    [Range(0, 0.1f)]
    public float oceanViewAttenuation = 1.0f;

    [Range(0, 1)]
    public float anisotropy = 0.2f;
    public Vector2Int numGrids = new Vector2Int(16, 16);
    public float oceanSurfaceHeight = 100f;
    public float oceanBedHeight = -12f;

    [SerializeField] private Mesh cubeMesh;
    // [SerializeField] private Shader godRayShader;
    [SerializeField] private Transform testQuadTransform;

    private CommandBuffer m_ShadowmapCb;
    private CommandBuffer m_GodRayCb;
    private RenderTexture m_ShadowmapCopy;
    [SerializeField] private Material m_GodRayMaterial;
    private Bounds m_LightSpacePhotonBounds;
    private Mesh m_UnitQuad;

    private void OnEnable()
    {
        RemoveCommandBuffers();
        UpdateUnitQuad();

        m_ShadowmapCb = CreateShadowmapCommandBuffer();
        photonLight.AddCommandBuffer(LightEvent.AfterShadowMap, m_ShadowmapCb);

        m_GodRayCb = CreateGodRayCommandBuffer();
        photonCamera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_GodRayCb);
        // CameraEvent.BeforeImageEffectsOpaque
    }

    private void OnDisable()
    {
        RemoveCommandBuffers();
    }

    private void OnDestroy()
    {
        RemoveCommandBuffers();
        m_ShadowmapCopy.Release();
        m_ShadowmapCb.Release();
        m_GodRayCb.Release();
    }

    private void OnValidate()
    {
        UpdateUnitQuad();
    }

    private void Update()
    {

    }

    private void RemoveCommandBuffers()
    {
        if(photonLight && m_ShadowmapCb != null)
        {
            photonLight.RemoveCommandBuffer(LightEvent.AfterShadowMap,m_ShadowmapCb);
        }
        if(photonCamera && m_GodRayCb != null)
        {
            photonCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_GodRayCb);
        }

    }

    private void UpdateUnitQuad()
    {
        var desc = new UnitQuadFactory.UnitQuadDesc();
        desc.numGrids = numGrids;
        m_UnitQuad = UnitQuadFactory.CreateUnitQuad(desc);
        EditorUtilites.SafeEditorCall(() =>
        {
            if(testQuadTransform)
            {
                var meshFilter = testQuadTransform.GetComponentInChildren<MeshFilter>();
                meshFilter.sharedMesh = m_UnitQuad;
            }
        });
    }

    private void OnPreRender()
    {
        var minDepth = photonCamera.nearClipPlane;
        var maxDepth = farDistance;

        var wFrustumCorners = GetWorldFrustumCorners(minDepth, maxDepth);
        m_LightSpacePhotonBounds = CalculateLightSpaceBounds(wFrustumCorners);
        m_LightSpacePhotonBounds = SquareBoundsXy(m_LightSpacePhotonBounds);

        if(m_GodRayCb != null)
        {
            BuildGodRayCommandBuffer(m_GodRayCb);
        }
    }

    private CommandBuffer CreateGodRayCommandBuffer()
    {
        var cb = new CommandBuffer();
        cb.name = "god ray";

        // m_GodRayMaterial = new Material(godRayShader);
        return cb;
    }

    private void BuildGodRayCommandBuffer(CommandBuffer godRayCb)
    {
        godRayCb.Clear();

        Vector3 lGridCenter = m_LightSpacePhotonBounds.center
            - new Vector3(0f, 0f, m_LightSpacePhotonBounds.extents.z);
        Vector3 lGridScale = m_LightSpacePhotonBounds.size;

        Vector3 wGridCenter = photonLight.transform.TransformPoint(lGridCenter);

        Matrix4x4 gridLocalToWorld = Matrix4x4.TRS(wGridCenter, photonLight.transform.rotation, lGridScale);
        Matrix4x4 gridWorldToLocal = Matrix4x4.Inverse(gridLocalToWorld);

        var worldToCamera = photonCamera.worldToCameraMatrix;
        var gpuProjMatrx = GL.GetGPUProjectionMatrix(photonCamera.projectionMatrix, true);
        var viewProjection = gpuProjMatrx * worldToCamera;

        // var target = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
        // godRayCb.SetRenderTarget(target);

        // NOTE: match UnderwaterGodRayLine.shader
        // x = 1 - g^2, y = 1 + g^2, z = 2g, w = 1 / (4 * PI)
        Vector4 mieParams = Vector4.zero;
        float g = anisotropy;
        mieParams.x = 1.0f - g * g;
        mieParams.y = 1.0f + g * g;
        mieParams.z = 2.0f * g;
        mieParams.w = 1.0f / (4.0f * Mathf.PI);

        m_GodRayMaterial.SetFloat("_PhotonIntensity", photonIntensity);
        m_GodRayMaterial.SetVector("_MieParams", mieParams);

        m_GodRayMaterial.SetFloat("_OceanSurfaceAttenuation", oceanSurfaceAttenuation);
        m_GodRayMaterial.SetFloat("_OceanViewAttenuation", oceanViewAttenuation);
        m_GodRayMaterial.SetFloat("_OceanSurfaceHeight", oceanSurfaceHeight);
        m_GodRayMaterial.SetFloat("_OceanBedHeight", oceanBedHeight);

        m_GodRayMaterial.SetMatrix("_UnitGridObjectToWorld", gridLocalToWorld);
        m_GodRayMaterial.SetMatrix("_UnitGridWorldToObject", gridWorldToLocal);
        m_GodRayMaterial.SetMatrix("_CameraViewProjection", viewProjection);
        m_GodRayMaterial.SetMatrix("_CameraView", worldToCamera);
        m_GodRayMaterial.SetTexture("_ShadowmapCopy", m_ShadowmapCopy);
        m_GodRayMaterial.SetTexture("_ShadowmapCopyUnity", m_ShadowmapCopy);
        godRayCb.DrawMesh(m_UnitQuad, gridLocalToWorld, m_GodRayMaterial, 0, -1);
        if(testQuadTransform)
        {
            testQuadTransform.FromMatrix(gridLocalToWorld);
        }
    }

    private CommandBuffer CreateShadowmapCommandBuffer()
    {
        var cb = new CommandBuffer();
        cb.name = "photon light shadowmap copy";

        RenderTargetIdentifier shadowmap = BuiltinRenderTextureType.CurrentActive;

        // NOTE: hack resolution here
        m_ShadowmapCopy = new RenderTexture(4096, 2048, 16, RenderTextureFormat.ARGB32);
        m_ShadowmapCopy.filterMode = FilterMode.Point;
        cb.SetShadowSamplingMode(shadowmap, ShadowSamplingMode.RawDepth);
        var id = new RenderTargetIdentifier(m_ShadowmapCopy);
        cb.Blit(shadowmap, id);
        cb.SetGlobalTexture("_ShadowmapCopy", id);

        return cb;
    }



    List<Vector3> GetWorldFrustumCorners(float minDepth, float maxDepth)
    {
        var frustumPositions = new List<Vector3>();
        Vector2[] viewportPositions = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
        };
        // Viewport space is normalized and relative to the camera.
        // The bottom-left of the viewport is (0,0); the top-right is (1,1). The z position is in world units from the camera.
        foreach(var viewportPos in viewportPositions)
        {
            var viewportPosWithDepth = new Vector3(viewportPos.x, viewportPos.y, minDepth);
            var worldPos = photonCamera.ViewportToWorldPoint(viewportPosWithDepth);
            frustumPositions.Add(worldPos);

            viewportPosWithDepth.z = maxDepth;
            worldPos = photonCamera.ViewportToWorldPoint(viewportPosWithDepth);
            frustumPositions.Add(worldPos);
        }

        Debug.Assert(frustumPositions.Count == 8);

        return frustumPositions;
    }

    Vector3 GetCenter(List<Vector3> points)
    {
        Vector3 center = Vector3.zero;
        foreach(var point in points)
        {
            center += point;
        }
        if(points.Count > 0)
        {
            center /= points.Count;
        }
        return center;
    }

    Vector3 Abs(Vector3 v)
    {
        return new Vector3(
            Mathf.Abs(v.x),
            Mathf.Abs(v.y),
            Mathf.Abs(v.z)
        );
    }

    Bounds CalculateLightSpaceBounds(List<Vector3> worldSpacePoints)
    {

        // var wPoint0 = worldSpacePoints[0];
        // var wPoint1 = worldSpacePoints[1];
        // var center = 0.5f * (wPoint0 + wPoint1);
        // var size = wPoint1 - wPoint0;
        // size = Abs(size);

        // Vector4 center4 = center;
        // center4.w = 1.0f;
        // Vector4 size4 = size;
        // size4.w = 0f;

        // var lCenter4 = worldToLightMatrix * center4;
        // var lSize4 = worldToLightMatrix * size4;

        // Bounds bounds = new Bounds(lCenter4, lSize4);
        var lightTrans = photonLight.transform;
        var lPoint0 = lightTrans.InverseTransformPoint(worldSpacePoints[0]);
        var lPoint1 = lightTrans.InverseTransformPoint(worldSpacePoints[1]);
        var center = 0.5f * (lPoint0 + lPoint1);
        var size = lPoint1 - lPoint0;
        size = Abs(size);


        Bounds bounds = new Bounds(center, size);

        foreach(var wPoint in worldSpacePoints)
        {
            Vector4 wPoint4 = new Vector4();
            wPoint4 = wPoint;
            wPoint4.w = 1.0f;

            // Vector4 lPoint4 = worldToLightMatrix * wPoint4;
            Vector3 lPoint = photonLight.transform.InverseTransformPoint(wPoint);

            // var wPointNew = photonLight.transform.TransformPoint(lPoint);
            // Gizmos.DrawSphere(wPointNew, 10f);

            bounds.Encapsulate(lPoint);
        }
        return bounds;
    }

    Bounds SquareBoundsXy(Bounds source)
    {
        float xyLength = Mathf.Max(source.size.x, source.size.y);
        var size = source.size;
        size.x = size.y = xyLength;
        source.size = size;
        return source;
    }

    Bounds GetLightSpaceBoundsFromCamera()
    {
        var minDepth = photonCamera.nearClipPlane;
        var maxDepth = farDistance;

        var wFrustumCorners = GetWorldFrustumCorners(minDepth, maxDepth);
        // Gizmos.color = Color.white;
        // foreach(var wFrustumCorner in wFrustumCorners)
        // {
        //     // Gizmos.DrawSphere(wFrustumCorner, 10f);
        // }


        Bounds lFrustumBounds = CalculateLightSpaceBounds(wFrustumCorners);
        float xyLength = Mathf.Max(lFrustumBounds.size.x, lFrustumBounds.size.y);
        var size = lFrustumBounds.size;
        size.x = size.y = xyLength;
        lFrustumBounds.size = size;
        return lFrustumBounds;
    }

    void OnDrawGizmos()
    {
        var lFrustumBounds = GetLightSpaceBoundsFromCamera();

        var size = lFrustumBounds.size;
        var boundsColor = Color.blue;
        // boundsColor.a = 0.25f;
        Gizmos.color = boundsColor;
        Gizmos.DrawWireMesh(cubeMesh, 0, photonLight.transform.TransformPoint(lFrustumBounds.center),
            photonLight.transform.rotation,
            size
        );
    }

}
