Shader "Custom/Cone Tornado"
{
	Properties
	{
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _WaveSpeed ("Wave Speed", Float) = 1
        _NumWaves ("Number of Waves", Float) = 3
        _MaxReelSwell ("Max Reel Swell", Float) = 1
        _ObliqueFactor ("Oblique Factor", Float) = 1
        _TextureSpeed ("Texture Speed", Vector) = (1, 1, 0, 0)

        _CutOffValue("Cut Off Value", Range(0, 1)) = 0.2
        _TransparencyFactor("TransparencyFactor", Range(0, 1)) = 0.5
        _FresnelScale("Fresnel Scale", Range(0, 1)) = 0.5
        [PowerSlider(10)] _FresnelExponent("Fresnel Exponenet", Range(0.01, 10)) = 1
	}
	SubShader
	{
		// Tags { "RenderType"="Opaque" }

        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        // Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }

		LOD 100

		Pass
		{
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
                float swellFactor : TEXCOORD1;
                float3 worldViewDir : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
			};

            static const float TWO_PI = 6.283185;

			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
            float _WaveSpeed;
            float _NumWaves;
            float _MaxReelSwell;
            float _ObliqueFactor;

            float4 _TextureSpeed;
            float _CutOffValue;
            float _TransparencyFactor;
            float _FresnelExponent;
            float _FresnelScale;


			v2f vert (appdata v)
			{
				v2f o;

                // _Time: Time since level load (t/20, t, t*2, t*3), use to animate things inside the shaders.
                float time = _Time.y;
                float offset = time * _WaveSpeed;
                // assume cone is placed along the z axis
                // +z: radius increases
                float wavePos = v.vertex.z + offset;
                wavePos *= TWO_PI;
                wavePos *= _NumWaves;

                float swellFactor = sin(wavePos);
                swellFactor *= v.uv.y;

                v.vertex += float4(v.normal * swellFactor * _MaxReelSwell, 0);
                o.swellFactor = swellFactor;


				o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv + float2(v.uv.y * _ObliqueFactor, 0);
                o.worldViewDir = WorldSpaceViewDir(v.vertex);
                o.worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz;

                // NOTE: start uv debug, remove in production
                // if(o.uv.x != floor(o.uv.x))
                // {
                //     o.uv.x = frac(o.uv.x);
                // }
                // if(o.uv.y != floor(o.uv.y))
                // {
                //     o.uv.y = frac(o.uv.y);
                // }
                // NOTE: end of uv debug

				// o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{


                _NoiseTex_ST.zw += _TextureSpeed.xy * _Time.y;
				float2 noiseUv = TRANSFORM_TEX(i.uv, _NoiseTex);
                fixed4 col = tex2D(_NoiseTex, noiseUv);

                float3 worldNormal = normalize(i.worldNormal);
                float3 worldViewDir = normalize(i.worldViewDir);
                float fresnel = abs(dot(i.worldNormal, i.worldViewDir));
                fresnel = saturate(1 - fresnel);
                fresnel = pow(fresnel, _FresnelExponent);
                fresnel = _FresnelScale + (1 - _FresnelScale) * fresnel;
                // col = max(col, fixed4(fresnel, fresnel, fresnel, fresnel));

                // clip(col.r - _CutOffValue);
                if(col.r < _CutOffValue)
                {
                    col.a = 0;
                }
                else
                {
                    float transparency = col.a;
                    // col.rgb = fixed3(1,1,1);
                    col.a = transparency * _TransparencyFactor;
                }

                // col = fixed4(1, 1, 1, fresnel);

                // col.a = 0.5;
                // col = fixed4(0,i.uv.y, 0, 1);

				return col;
			}
			ENDCG
		}
	}
}
