Shader "Original Water/Caustics Projector"
{
 Properties
 {
  _CausticsColor ("Caustics Color", Color) = (0.28,0.72,0.88,1)
  _Intensity ("Intensity", Range(0,5)) = 1.4
  _Scale ("Scale", Float) = 0.35
  _Speed ("Speed", Float) = 0.9
  _WaterHeight ("Water Height", Float) = 68
  _FadeDepth ("Fade Depth", Float) = 35
 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-1" "RenderType"="Transparent" }
  Pass
  {
   Tags { "LightMode"="SRPDefaultUnlit" }
   ZWrite Off
   ZTest Always
   Cull Front
   Blend One One
   ColorMask RGB
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma target 3.0
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
   CBUFFER_START(UnityPerMaterial)
    float4 _CausticsColor;
    float _Intensity, _Scale, _Speed, _WaterHeight, _FadeDepth;
   CBUFFER_END
   struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
   struct Varyings { float4 positionCS : SV_POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO };
   Varyings Vert(Attributes i)
   {
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_TRANSFER_INSTANCE_ID(i,o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
    return o;
   }
   half4 Frag(Varyings i) : SV_Target
   {
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float2 uv = GetNormalizedScreenSpaceUV(i.positionCS);
    float depth = SampleSceneDepth(uv);
    #if UNITY_REVERSED_Z
     if(depth < 0.00001) discard;
    #else
     if(depth > 0.99999) discard;
     depth = lerp(UNITY_NEAR_CLIP_VALUE,1,depth);
    #endif
    float3 world = ComputeWorldSpacePosition(uv,depth,UNITY_MATRIX_I_VP);
    clip(0.5-abs(TransformWorldToObject(world)));
    clip(_WaterHeight-0.1-world.y);
    float t = _Time.y*_Speed;
    float2 p = world.xz*_Scale;
    float2 a = p+float2(t*0.35,t*0.22);
    float2 b = p-float2(t*0.28,-t*0.31);
    float c1 = sin(a.x*2.8+sin(a.y*1.8+t))*0.5+0.5;
    float c2 = cos(b.x*2.2+cos(b.y*3.1-t))*0.5+0.5;
    float c3 = sin((a.y+b.x)*2+t*0.8)*0.5+0.5;
    float c = pow(min(min(c1,c2),c3),1.8)*3.5;
    float attenuation = saturate(1-(_WaterHeight-world.y)/max(_FadeDepth,0.001));
    return half4(_CausticsColor.rgb*c*_Intensity*attenuation,0);
   }
   ENDHLSL
  }
 }
 FallBack Off
}
