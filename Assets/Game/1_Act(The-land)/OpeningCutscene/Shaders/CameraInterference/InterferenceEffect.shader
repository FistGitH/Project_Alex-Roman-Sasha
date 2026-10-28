Shader "OpeningCutscene/Interference Effect"
{
	Properties
	{
		_Strength("Strength", Range(0, 1)) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Overlay+100"
			"RenderType" = "Transparent"
			"IgnoreProjector" = "True"
		}

		Pass
		{
			Cull Off
			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
			#pragma target 2.0
			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			CBUFFER_START(UnityPerMaterial)
				float _Strength;
			CBUFFER_END

			Varyings Vert(Attributes input)
			{
				Varyings output;
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uv = input.uv;
				return output;
			}

			float Hash(float2 value)
			{
				return frac(sin(dot(value, float2(12.9898, 78.233))) * 43758.5453);
			}

			half4 Frag(Varyings input) : SV_Target
			{
				float strength = saturate(_Strength);
				float frame = floor(_Time.y * lerp(12.0, 30.0, strength));
				float2 noiseResolution = lerp(float2(480.0, 270.0), float2(128.0, 72.0), strength);
				float noise = Hash(floor(input.uv * noiseResolution) + frame);
				float scanline = sin((input.uv.y * _ScreenParams.y + _Time.y * 90.0) * 3.14159) * 0.5 + 0.5;
				float bandNoise = Hash(float2(floor(input.uv.y * 48.0), frame));
				float dropout = step(1.0 - strength * 0.22, bandNoise);

				float luminance = saturate(noise * 0.85 + scanline * 0.15);
				luminance = lerp(luminance, Hash(float2(frame, floor(input.uv.y * 16.0))), dropout * 0.7);

				float alpha = saturate(strength * lerp(0.35, 1.0, strength) + dropout * strength * 0.25);

				return half4(luminance, luminance, luminance, alpha);
			}
			ENDHLSL
		}
	}
}
