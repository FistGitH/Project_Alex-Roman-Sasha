Shader "Game/Skybox/Milky Way"
{
    Properties
    {
        _Exposure ("Exposure", Range(0, 4)) = 1
        _Rotation ("Rotation", Range(0, 360)) = 27
        _BandTilt ("Band tilt", Range(-80, 80)) = 24
        _BandWidth ("Band width", Range(0.04, 0.4)) = 0.16
        _BandIntensity ("Band intensity", Range(0, 3)) = 0.85
        _StarIntensity ("Star intensity", Range(0, 4)) = 1.2
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Exposure, _Rotation, _BandTilt, _BandWidth, _BandIntensity, _StarIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 direction : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
                return output;
            }

            float Hash(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            float Noise(float3 p)
            {
                float3 cell = floor(p);
                float3 t = frac(p);
                t = t * t * (3.0 - 2.0 * t);
                float4 low = float4(Hash(cell), Hash(cell + float3(1,0,0)),
                    Hash(cell + float3(0,1,0)), Hash(cell + float3(1,1,0)));
                cell.z += 1.0;
                float4 high = float4(Hash(cell), Hash(cell + float3(1,0,0)),
                    Hash(cell + float3(0,1,0)), Hash(cell + float3(1,1,0)));
                float4 corners = lerp(low, high, t.z);
                return lerp(lerp(corners.x, corners.y, t.x), lerp(corners.z, corners.w, t.x), t.y);
            }

            float Stars(float3 p, float threshold)
            {
                float3 cell = floor(p);
                float pick = Hash(cell);
                float3 center = 0.23 + 0.54 * float3(Hash(cell + 7), Hash(cell + 19), Hash(cell + 41));
                float distance = length(frac(p) - center);
                float footprint = min(0.13, max(length(ddx(p)), length(ddy(p))) * 0.5);
                float radius = lerp(0.06, 0.17, pick);
                return step(threshold, pick) * (1.0 - smoothstep(radius * 0.28, radius + footprint, distance));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 d = normalize(input.direction);
                float yaw = radians(_Rotation);
                d.xz = mul(float2x2(cos(yaw), -sin(yaw), sin(yaw), cos(yaw)), d.xz);
                float tilt = radians(_BandTilt);
                float latitude = dot(d, float3(sin(tilt), cos(tilt), 0));
                float bandOffset = latitude / _BandWidth;
                float band = exp(-bandOffset * bandOffset);

                float cloud = Noise(d * 8.0) * 0.52 + Noise(d * 21.0) * 0.30 + Noise(d * 53.0) * 0.18;
                float core = smoothstep(-0.65, 0.85, d.z);
                float dustOffset = (latitude + (Noise(d * 13.0) - 0.5) * 0.13) / 0.035;
                float dustLane = exp(-dustOffset * dustOffset);
                float3 nebula = lerp(float3(0.047, 0.043, 0.092), float3(0.21, 0.143, 0.10), core * cloud);
                float3 color = lerp(float3(0.0018, 0.0027, 0.006), float3(0.004, 0.006, 0.014), saturate(d.y));
                color += nebula * band * (0.25 + cloud * cloud) * (1.0 - dustLane * 0.58) * _BandIntensity;
                color += Stars(d * 77.0, 0.86) * float3(0.59, 0.71, 1.0) * _StarIntensity;
                color += Stars(d * 177.0 + 31.0, 0.80 - band * 0.22) * (0.13 + band * 0.21) * _StarIntensity;
                return half4(color * _Exposure, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
