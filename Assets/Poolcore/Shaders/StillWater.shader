Shader "Poolcore/Still Water"
{
    Properties
    {
        _Deep("Water scattering tint", Color)=(0.045,0.24,0.23,1)
        _Absorption("Absorption per metre (RGB)", Vector)=(0.55,0.14,0.10,0)
        _Reflection("Fallback reflection", Cube)="" {}
        _ReflectionFloor("Minimum reflection", Range(0,0.3))=0.065
        _ReflectionBlur("Reflection softness", Range(0,3))=0.65
        [HideInInspector] _PlanarReflection("Planar reflection", 2D)="black" {}
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            TEXTURECUBE(_Reflection); SAMPLER(sampler_Reflection);
            TEXTURE2D(_PlanarReflection); SAMPLER(sampler_PlanarReflection);
            CBUFFER_START(UnityPerMaterial)
                half4 _Deep; float4 _Absorption;
                float _ReflectionFloor, _ReflectionBlur;
            CBUFFER_END
            float4x4 _ReflectionVP;
            float _HasPlanarReflection;
            struct A {float4 positionOS:POSITION;};
            struct V {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;};
            V Vert(A a) {V o;o.positionWS=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);return o;}
            half4 Frag(V i):SV_Target
            {
                float2 uv=GetNormalizedScreenSpaceUV(i.positionCS);
                float depth=SampleSceneDepth(uv);
                #if !UNITY_REVERSED_Z
                    depth=lerp(UNITY_NEAR_CLIP_VALUE,1,depth);
                #endif
                float3 behind=ComputeWorldSpacePosition(uv,depth,UNITY_MATRIX_I_VP);
                float path=min(distance(behind,i.positionWS),30);
                half3 transmission=exp(-_Absorption.rgb*path);
                half3 transmitted=SampleSceneColor(uv)*transmission+_Deep.rgb*(1-transmission);
                half3 v=GetWorldSpaceNormalizeViewDir(i.positionWS);
                float fresnel=_ReflectionFloor+(1-_ReflectionFloor)*pow(1-saturate(v.y),5);
                half3 reflected=SAMPLE_TEXTURECUBE_LOD(_Reflection,sampler_Reflection,reflect(-v,half3(0,1,0)),_ReflectionBlur).rgb;
                float4 projected=mul(_ReflectionVP,float4(i.positionWS,1));
                float2 reflectionUV=projected.xy/max(projected.w,0.0001)*0.5+0.5;
                if(_HasPlanarReflection>.5 && projected.w>0 && all(reflectionUV>=0) && all(reflectionUV<=1))
                    reflected=SAMPLE_TEXTURE2D_LOD(_PlanarReflection,sampler_PlanarReflection,reflectionUV,_ReflectionBlur).rgb;
                float fogDepth=max(-TransformWorldToView(i.positionWS).z-_ProjectionParams.y,0);
                return half4(MixFog(lerp(transmitted,reflected,fresnel),ComputeFogFactorZ0ToFar(fogDepth)),1);
            }
            ENDHLSL
        }
    }
}
