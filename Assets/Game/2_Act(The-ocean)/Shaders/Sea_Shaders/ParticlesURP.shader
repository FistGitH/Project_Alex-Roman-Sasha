Shader "Original Water/Particles URP"
{
 Properties { _MainTex("Particle Texture",2D)="white" {} _Color("Tint",Color)=(1,1,1,1) }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
  Pass
  {
   Tags { "LightMode"="SRPDefaultUnlit" }
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
   CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST, _Color;
   CBUFFER_END
   struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
   struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; half fog:TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };
   Varyings Vert(Attributes i)
   {
    Varyings o=(Varyings)0;
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.positionCS=TransformObjectToHClip(i.positionOS.xyz);
    o.uv=i.uv*_MainTex_ST.xy+_MainTex_ST.zw;
    o.color=i.color*_Color;
    o.fog=ComputeFogFactor(o.positionCS.z);
    return o;
   }
   half4 Frag(Varyings i):SV_Target
   {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color;
    c.rgb=MixFog(c.rgb,i.fog);
    return c;
   }
   ENDHLSL
  }
 }
 FallBack Off
}
