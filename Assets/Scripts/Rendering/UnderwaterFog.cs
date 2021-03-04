using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class UnderwaterFog : MonoBehaviour
{

    private Camera m_Camera;
    private CommandBuffer m_CmdBuffer;
    [SerializeField] private Material m_FogMaterial;
    [SerializeField] private Light m_sunLight;

    void OnEnable()
    {
        m_Camera = GetComponent<Camera>();
        if(m_Camera.actualRenderingPath == RenderingPath.Forward)
        {
            m_Camera.depthTextureMode = DepthTextureMode.Depth;
        }

        string fogShaderName = "Custom/Underwater Fog";
        if(m_FogMaterial == null)
        {
            var shader = Shader.Find(fogShaderName);
            if(shader == null)
                throw new Exception("Error: " + fogShaderName + " is missing.");
            m_FogMaterial = new Material(shader);
        }


        m_CmdBuffer = new CommandBuffer { name = "Underwater Fog" };

        m_Camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, m_CmdBuffer);
    }

    void OnDisable()
    {
        if(m_Camera != null)
        {
            if(m_CmdBuffer != null)
            {
                m_Camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, m_CmdBuffer);
            }
        }
    }

    void OnPreCull()
    {
    }

    void BuildCommandBuffers(RenderTexture source, RenderTexture target)
    {
        m_CmdBuffer.Clear();

        // var cameraTarget = new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive);
        // m_CmdBuffer.SetRenderTarget(cameraTarget);
    }

    [ImageEffectOpaque]
    public void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        // send view space frustum corner pos to shader
        // NOTE: frustum corner idx from CalculateFrustumCorners (from testing)
        //       1 -- 2
        //       |    |
        //       0 -- 3
        //       frustum corner idx expected from shader (2 * uv.x + uv.y)
        //       1 -- 3
        //       |    |
        //       0 -- 2
        Vector3[] frustumCorners = new Vector3[4];
        m_Camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), 1.0f, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        Vector4[] frustumCornersToShader = new Vector4[4];
        frustumCornersToShader[0] = frustumCorners[0];
        frustumCornersToShader[1] = frustumCorners[1];
        frustumCornersToShader[2] = frustumCorners[3];
        frustumCornersToShader[3] = frustumCorners[2];

        // set camera properties
        m_FogMaterial.SetVectorArray("_FrustumCorners", frustumCornersToShader);

        // set light properties
        Debug.Assert(m_sunLight.type == LightType.Directional);
        Vector4 direction = m_sunLight.transform.forward;
        direction.w = 0f;
        Color color = m_sunLight.color;
        m_FogMaterial.SetVector("_SunLightWorldSpaceDirection", direction);
        m_FogMaterial.SetColor("_SunLightColor", color);

        m_FogMaterial.SetTexture("_MainTex", source);

        Graphics.Blit(source, dest, m_FogMaterial, 0);
    }

}