Shader "Custom/Underwater God Ray Line"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

        [Header(Debug Options)]
		[Toggle] _CompileShaderWithDebugInfo("Compile Shader With Debug Info (D3D11)", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent"  "Queue"="Transparent"}
		LOD 100

		Pass
		{
            Cull Off
            ZTest On
            ZWrite Off
            Blend SrcAlpha One

			CGPROGRAM
            // #pragma target 4.6
			#pragma vertex vert
			#pragma fragment frag
            #pragma geometry geom

            #pragma shader_feature _COMPILESHADERWITHDEBUGINFO_ON

			#if _COMPILESHADERWITHDEBUGINFO_ON
			#pragma enable_d3d11_debug_symbols
			#endif


			#include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
			};

			// struct v2f
			// {
			// 	float2 uv : TEXCOORD0;
			// 	float4 vertex : SV_POSITION;
			// };

            struct v2g
            {
               float4 wPos : SV_POSITION;
               float2 uv : TEXCOORD0;
               float3 wNormal : NORMAL;
            };

            struct g2f
            {
                float4 clipPos : SV_POSITION;
                float3 viewPos : COLOR0;
                float3 vRayDirection : COLOR1;
                float depthFromSurface : COLOR3;
                float3 worldPos : COLOR4;
            };

			sampler2D _ShadowmapCopy;
            UNITY_DECLARE_SHADOWMAP(_ShadowmapCopyUnity);

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _PhotonIntensity = 1.0;

            float _OceanSurfaceAttenuation;
            float _OceanViewAttenuation;
            float _OceanSurfaceHeight;
            float _OceanBedHeight;

            float4x4 _UnitGridObjectToWorld;
            float4x4 _UnitGridWorldToObject;
            float4x4 _CameraViewProjection;
            float4x4 _CameraView;

            //-----------------------------------------------------------------------------------------
            // GetCascadeWeights_SplitSpheres
            //-----------------------------------------------------------------------------------------
            inline fixed4 GetCascadeWeights_SplitSpheres(float3 wpos)
            {
                float3 fromCenter0 = wpos.xyz - unity_ShadowSplitSpheres[0].xyz;
                float3 fromCenter1 = wpos.xyz - unity_ShadowSplitSpheres[1].xyz;
                float3 fromCenter2 = wpos.xyz - unity_ShadowSplitSpheres[2].xyz;
                float3 fromCenter3 = wpos.xyz - unity_ShadowSplitSpheres[3].xyz;
                float4 distances2 = float4(dot(fromCenter0, fromCenter0), dot(fromCenter1, fromCenter1), dot(fromCenter2, fromCenter2), dot(fromCenter3, fromCenter3));

                fixed4 weights = float4(distances2 < unity_ShadowSplitSqRadii);
                weights.yzw = saturate(weights.yzw - weights.xyz);
                return weights;
            }

            //-----------------------------------------------------------------------------------------
            // GetCascadeShadowCoord
            //-----------------------------------------------------------------------------------------
            inline float4 GetCascadeShadowCoord(float4 wpos, fixed4 cascadeWeights)
            {
                float3 sc0 = mul(unity_WorldToShadow[0], wpos).xyz;
                float3 sc1 = mul(unity_WorldToShadow[1], wpos).xyz;
                float3 sc2 = mul(unity_WorldToShadow[2], wpos).xyz;
                float3 sc3 = mul(unity_WorldToShadow[3], wpos).xyz;

                float4 shadowMapCoordinate = float4(sc0 * cascadeWeights[0] + sc1 * cascadeWeights[1] + sc2 * cascadeWeights[2] + sc3 * cascadeWeights[3], 1);
#if defined(UNITY_REVERSED_Z)
                float  noCascadeWeights = 1 - dot(cascadeWeights, float4(1, 1, 1, 1));
                shadowMapCoordinate.z += noCascadeWeights;
#endif
                return shadowMapCoordinate;
            }


            // x = 1 - g^2, y = 1 + g^2, z = 2g, w = 1 / (4 * PI)
            float4 _MieParams;
            float Mie(float cosAngle)
            {
                return _MieParams.w * _MieParams.x * pow(_MieParams.y - _MieParams.z * cosAngle, -3.0/2.0);
            }

			v2g vert (appdata v)
			{
				v2g o;
				o.wPos = mul(_UnitGridObjectToWorld, v.vertex);
                o.wNormal = mul(v.normal, (float3x3)_UnitGridWorldToObject);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

            [maxvertexcount(2)]
            void geom(triangle v2g input[3], inout LineStream<g2f> lineStream)
            {
                float3 wCenter = 1.0 / 3.0 * (input[0].wPos + input[1].wPos + input[2].wPos);
                float3 wNormal = normalize(input[0].wNormal);

                float3 origin = wCenter;

                // float4 shadowCoord1 = mul(unity_WorldToShadow[1], float4(origin, 1));
                // float depthFromLight = tex2Dlod (_ShadowmapCopy, float4(shadowCoord1.xy, 0, 0)).r;
                // float deltaDepth = depthFromLight - shadowCoord1.z;

                // \vec{o} + \vec{n}t = \vec{start}
                // t = \frac{h - o_y}{n_y}
                float tSurface = (_OceanSurfaceHeight - origin.y) / wNormal.y;
                float tBed = (_OceanBedHeight - origin.y) / wNormal.y;

                float3 wStart = origin + wNormal * tSurface;
                float3 wEnd = origin + wNormal * tBed;

                float3 vRayDirection = mul(_CameraView, float4(wNormal, 0)).xyz;

                g2f o = (g2f)0;
                o.clipPos = mul(_CameraViewProjection, float4(wStart, 1.0));
                o.vRayDirection = vRayDirection;
                o.viewPos = mul(_CameraView, float4(wStart, 1.0)).xyz;
                o.depthFromSurface = 0;
                o.worldPos = wStart;

                lineStream.Append(o);

                o.clipPos = mul(_CameraViewProjection, float4(wEnd, 1.0));
                o.vRayDirection = vRayDirection;
                o.viewPos = mul(_CameraView, float4(wEnd, 1.0)).xyz;
                o.depthFromSurface = _OceanSurfaceHeight - _OceanBedHeight;
                o.worldPos = wEnd;

                lineStream.Append(o);

                lineStream.RestartStrip();
            }

			fixed4 frag (g2f i) : SV_Target
			{

                float3 vRayDirection = normalize(i.vRayDirection);
                float3 viewDir = normalize(i.viewPos);
                float cosTheta = dot(vRayDirection, viewDir);

                float viewAtten = exp(-_OceanViewAttenuation * abs(i.viewPos.z));
                float surfaceAtten = exp(-_OceanSurfaceAttenuation * i.depthFromSurface);

                float shadowAtten = 1;
                float3 worldPos = i.worldPos;


                float4 cascadeWeights = GetCascadeWeights_SplitSpheres(worldPos);
                float3 shadowCoord0 = mul(unity_WorldToShadow[0], float4(i.worldPos, 1)).xyz;
                float3 shadowCoord1 = mul(unity_WorldToShadow[1], float4(i.worldPos, 1)).xyz;
                float3 shadowCoord2 = mul(unity_WorldToShadow[2], float4(i.worldPos, 1)).xyz;
                float3 shadowCoord3 = mul(unity_WorldToShadow[3], float4(i.worldPos, 1)).xyz;

                float3 shadowCoord =
                    shadowCoord0 * cascadeWeights[0] +
                    shadowCoord1 * cascadeWeights[1] +
                    shadowCoord2 * cascadeWeights[2] +
                    shadowCoord3 * cascadeWeights[3]
                ;

                float depthFromLight = tex2Dlod (_ShadowmapCopy, float4(shadowCoord.xy, 0, 0)).r;

                if(depthFromLight > shadowCoord1.z && depthFromLight != 0)
                {
                    shadowAtten = 1;
                }
                else
                {
                    shadowAtten = 0;
                }

                // shadowAtten = 1;
                // // float4 cascadeWeights = GetCascadeWeights_SplitSpheres(worldPos);
                // bool inside = dot(cascadeWeights, float4(1, 1, 1, 1)) < 4;
                // float4 samplePos = GetCascadeShadowCoord(float4(worldPos, 1), cascadeWeights);
                // shadowAtten = inside ? UNITY_SAMPLE_SHADOW(_ShadowmapCopyUnity, samplePos.xyz) : 1.0;

                // shadowAtten = UNITY_SAMPLE_SHADOW(_ShadowmapCopyUnity, samplePos.xyz);

                // if(samplePos.x < 0 || samplePos.x > 1 || samplePos.y < 0 || samplePos.y > 1)
                // {
                //     shadowAtten = 1;
                // }
                // // shadowAtten = _LightShadowData.r + shadowAtten * (1 - _LightShadowData.r);

                float iFinal = _PhotonIntensity * Mie(cosTheta) * viewAtten * surfaceAtten * saturate(1.0 - shadowAtten);


                return fixed4(1,1,1,iFinal);
			}
			ENDCG
		}
	}
}
