// Inspired and modfied from OceanEmission.hlsl from Crest ocean renderer.
// File path: /Crest/Shaders/OceanEmission.hlsl
// https://github.com/crest-ocean/crest

Shader "Custom/Underwater Fog"
{
	Properties
	{
        [Header(Ocean Info)]
        // The y pos of the ocean surface in world space
        _OceanSurfaceHeight("Ocean Surface Height", Float) = 10
        _DepthOutscatterFactor("Depth Outscatter Factor", Range(0, 1)) = 0.25
        // Scattering coefficient within water volume, per channel
        _DepthFogDensity("Fog Density", Vector) = (0.08, 0.05, 0.07, 1.0)

        // Base light scattering settings which give water colour
        [Header(Scattering)]
        // Base colour when looking straight down into water
        _Diffuse("Diffuse", Color) = (0.0, 0.0124, 0.566, 1.0)
        // Base colour when looking into water at shallow/grazing angle
        _DiffuseGrazing("Diffuse Grazing", Color) = (0.184, 0.393, 0.519, 1)

        [Header(Subsurface Scattering)]
        // Colour tint for primary light contribution
		_SubsurfaceColor("Color", Color) = (0.0, 0.48, 0.36)
		// Amount of primary light contribution that always comes in
		_SubsurfaceBase("Base Mul", Range(0.0, 4.0)) = 1.0
		// Primary light contribution in direction of light to emulate light passing through waves
		_SubsurfaceSun("Sun Mul", Range(0.0, 10.0)) = 4.5
		// Fall-off for primary light scattering to affect directionality
		_SubsurfaceSunFalloff("Sun Falloff", Range(1.0, 16.0)) = 5.0

        [Toggle] _CompileShaderWithDebugInfo("Compile Shader With Debug Info (D3D11)", Float) = 0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

            #pragma shader_feature _COMPILESHADERWITHDEBUGINFO_ON

            #if _COMPILESHADERWITHDEBUGINFO_ON
			#pragma enable_d3d11_debug_symbols
			#endif

			#include "UnityCG.cginc"
            #include "Lighting.cginc"

            // Ocean information
            float _OceanSurfaceHeight;
            float _DepthOutscatterFactor;
            float4 _DepthFogDensity;

            // Scattering
            half3 _Diffuse;
            half3 _DiffuseGrazing;

            // subsurface scattering
            half3 _SubsurfaceColor;
            half  _SubsurfaceBase;
            half  _SubsurfaceSun;
            half  _SubsurfaceSunFalloff;

            // Sunlight properties, set by us
            float4 _SunLightWorldSpaceDirection;
            half3 _SunLightColor;

            // Textures, set by Unity
            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            // Camera properties, set by us
            // Frustum corn idx expected (2 * uv.x + uv.y)
            // 1 -- 3
            // |    |
            // 0 -- 2
            float4 _FrustumCorners[4];


			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
                float3 frustumRay : TEXCOORD1;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

                int frustumCornerIdx = 2 * (int)v.uv.x + (int)v.uv.y;
                o.frustumRay = _FrustumCorners[frustumCornerIdx].xyz;

				return o;
			}

            float3 ScatterColor(
                in const float3 i_cameraPos,
                in const float3 i_lightDir,
                in const half3  i_lightColor,
                in const float3 i_viewDir
            )
            {
                float depth = _OceanSurfaceHeight - i_cameraPos.y;
                depth = max(depth, 0.0);

                // base color
                float v = abs(i_viewDir.y);
                half3 col = lerp(_Diffuse, _DiffuseGrazing, 1.0 - pow(v, 1.0));

                // light
                // use the constant term (0th order) of SH stuff - this is the average.
                // it seems to give the right kind of colour
                // col *= half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);

                // Approx subsurface scattering -> add light when surface faces viewer.
                half towardsSun = pow(max(0, dot(i_lightDir, -i_viewDir)), _SubsurfaceSunFalloff);
                half3 subsurface = (_SubsurfaceBase + _SubsurfaceSun * towardsSun) * _SubsurfaceColor.rgb * i_lightColor;
                col += subsurface;

                // outscatter light - attenuate the final colour by the camera depth under the water, to approximate less
	            // throughput due to light scatter as the camera gets further under water.
                col *= exp(-_DepthFogDensity.xyz * _DepthFogDensity.w * depth * _DepthOutscatterFactor);

                return col;
            }

			float4 frag (v2f i) : SV_Target
			{
				float4 sceneColor = tex2D(_MainTex, i.uv);
				// // just invert the colors
				// col.rgb = 1 - col.rgb;

                float depthRaw = tex2D(_CameraDepthTexture, i.uv).r;
                float sceneZ = LinearEyeDepth(depthRaw);

                float3 viewRay = normalize(i.frustumRay);

                float3 scatterCol = ScatterColor(
                    _WorldSpaceCameraPos,
                    _SunLightWorldSpaceDirection,
                    _SunLightColor,
                    viewRay
                );

                float scatterFactor = 1.0 - exp(-_DepthFogDensity.xyz * _DepthFogDensity.w * sceneZ);
                fixed3 col3 = lerp(sceneColor.rgb, scatterCol, scatterFactor);

                fixed4 col = fixed4(col3, sceneColor.a);

                return col;
                // return sceneColor;
			}
			ENDCG
		}
	}
}
