Shader "Original Water/Surface FX"
{
 Properties
 {
  _BaseColor ("Base Color", Color) = (0.05,0.35,0.45,0.85)
  _DeepColor ("Deep Color", Color) = (0.01,0.12,0.20,1)
  [Normal] _PrimaryNormal ("Primary Normal Map", 2D) = "bump" {}
  [Normal] _SecondaryNormal ("Secondary Normal Map", 2D) = "bump" {}
  _NormalStrength ("Normal Strength", Range(0,2)) = 0.6
  _FresnelPower ("Fresnel Power", Range(0.1,8)) = 3
  _Smoothness ("Smoothness", Range(0,1)) = 0.85
  _PrimaryFlowOffset ("Primary Flow Offset", Vector) = (0,0,0,0)
  _SecondaryFlowOffset ("Secondary Flow Offset", Vector) = (0,0,0,0)
  [Header(Shore Foam)]
  _FoamColor ("Foam Color", Color) = (0.90,0.97,1,0.95)
  _FoamDistance ("Foam Distance", Range(0.05,5)) = 1.5
  _FoamNoiseCutoff ("Foam Noise Factor", Range(0,2)) = 0.5
  _FoamSpeed ("Foam Speed", Float) = 0.8
 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
  Pass
  {
   Name "WaterForward"
   Tags { "LightMode"="UniversalForwardOnly" }
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma target 3.0
   #pragma multi_compile_fog
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
   TEXTURE2D(_PrimaryNormal); SAMPLER(sampler_PrimaryNormal);
   TEXTURE2D(_SecondaryNormal); SAMPLER(sampler_SecondaryNormal);
   CBUFFER_START(UnityPerMaterial)
    float4 _PrimaryNormal_ST, _SecondaryNormal_ST;
    float4 _BaseColor, _DeepColor, _FoamColor;
    float4 _PrimaryFlowOffset, _SecondaryFlowOffset;
    float _NormalStrength, _FresnelPower, _Smoothness;
    float _FoamDistance, _FoamNoiseCutoff, _FoamSpeed;
   CBUFFER_END
   struct Attributes
   {
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
   };
   struct Varyings
   {
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    half3 normalWS : TEXCOORD1;
    half4 tangentWS : TEXCOORD2;
    float2 uv : TEXCOORD3;
    half fog : TEXCOORD4;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
   };
   Varyings Vert(Attributes input)
   {
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input,o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs basis = GetVertexNormalInputs(input.normalOS,input.tangentOS);
    o.positionCS = pos.positionCS;
    o.positionWS = pos.positionWS;
    o.normalWS = basis.normalWS;
    o.tangentWS = half4(basis.tangentWS,input.tangentOS.w*GetOddNegativeScale());
    o.uv = input.uv;
    o.fog = ComputeFogFactor(pos.positionCS.z);
    return o;
   }
   half4 Frag(Varyings i, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
   {
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float2 uvA = i.uv*_PrimaryNormal_ST.xy+_PrimaryNormal_ST.zw+_PrimaryFlowOffset.xy;
    float2 uvB = i.uv*_SecondaryNormal_ST.xy+_SecondaryNormal_ST.zw+_SecondaryFlowOffset.xy;
    half3 nA = UnpackNormal(SAMPLE_TEXTURE2D(_PrimaryNormal,sampler_PrimaryNormal,uvA));
    half3 nB = UnpackNormal(SAMPLE_TEXTURE2D(_SecondaryNormal,sampler_SecondaryNormal,uvB));
    half3 nTS = normalize(half3((nA.xy+nB.xy)*_NormalStrength,max(nA.z*nB.z,0.01)));
    half3 normal = normalize(i.normalWS);
    half3 tangent = normalize(i.tangentWS.xyz);
    half3 bitangent = cross(normal,tangent)*i.tangentWS.w;
    normal = normalize(tangent*nTS.x+bitangent*nTS.y+normal*nTS.z);
    normal *= IS_FRONT_VFACE(face,1.0,-1.0);
    half3 view = GetWorldSpaceNormalizeViewDir(i.positionWS);
    half fresnel = pow(1-saturate(dot(view,normal)),_FresnelPower);
    half4 col = lerp(_DeepColor,_BaseColor,fresnel);
    float rawDepth = SampleSceneDepth(GetNormalizedScreenSpaceUV(i.positionCS));
    float sceneDepth = unity_OrthoParams.w > 0.5 ? LinearDepthToEyeDepth(rawDepth) : LinearEyeDepth(rawDepth,_ZBufferParams);
    float depthDiff = sceneDepth+TransformWorldToView(i.positionWS).z;
    half foam = 0;
    if(depthDiff > 0 && depthDiff < _FoamDistance)
    {
     float wave = sin(_Time.y*_FoamSpeed*PI+(uvA.x+uvA.y)*20)*0.15;
     foam = smoothstep(0.15,0.65,saturate(1-depthDiff/max(_FoamDistance,0.001)+(nA.x*nB.y*1.5+wave)*_FoamNoiseCutoff))*_FoamColor.a;
     col.rgb = lerp(col.rgb,_FoamColor.rgb,foam);
     col.a = saturate(col.a+foam*0.35);
    }
    Light sun = GetMainLight();
    half3 ambient = max(SampleSH(normal),half3(0.08,0.08,0.08));
    half smoothness = lerp(_Smoothness,0.25,foam);
    half specular = pow(saturate(dot(normal,SafeNormalize(sun.direction+view))),exp2(3+smoothness*8));
    col.rgb = col.rgb*(ambient+sun.color*saturate(dot(normal,sun.direction)))+sun.color*specular*0.4;
    col.rgb = MixFog(col.rgb,i.fog);
    return col;
   }
   ENDHLSL
  }
 }
 FallBack Off
}
