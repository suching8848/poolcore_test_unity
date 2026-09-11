Shader "Poolcore/Still Water"
{
    Properties
    {
        _Shallow("Shallow", Color)=(0.31,0.68,0.65,1)
        _Deep("Deep", Color)=(0.035,0.25,0.28,1)
        _Reflection("Reflection", Cube)="" {}
        _Ripple("Ripple strength", Range(0,1))=0.12
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            TEXTURECUBE(_Reflection); SAMPLER(sampler_Reflection);
            CBUFFER_START(UnityPerMaterial)
                half4 _Shallow, _Deep; float _Ripple;
            CBUFFER_END
            float4 _PoolFootRipple;
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float fog:TEXCOORD1; };
            V Vert(A a) { V o; o.positionWS=TransformObjectToWorld(a.positionOS.xyz); o.positionCS=TransformWorldToHClip(o.positionWS); o.fog=ComputeFogFactor(o.positionCS.z); return o; }
            half4 Frag(V i):SV_Target
            {
                float2 p=i.positionWS.xz;
                float t=_Time.y;
                float2 wave=float2(sin(p.x*2.3+p.y*1.6+t*0.55)+sin(p.y*4.4-t*0.4),cos(p.y*2.7-p.x*1.8+t*0.45)+cos(p.x*4.1+t*0.3));
                float dist=distance(p,_PoolFootRipple.xy); float age=t-_PoolFootRipple.z;
                float ring=sin(dist*16-age*11)*exp(-dist*0.9)*saturate(1-age/3)*_PoolFootRipple.w;
                wave+=normalize(p-_PoolFootRipple.xy+0.0001)*ring*2;
                half3 n=normalize(half3(wave.x*_Ripple*0.17,1,wave.y*_Ripple*0.17));
                float2 uv=GetNormalizedScreenSpaceUV(i.positionCS);
                float sceneDepth=LinearEyeDepth(SampleSceneDepth(uv),_ZBufferParams);
                float waterDepth=-TransformWorldToView(i.positionWS).z;
                float thickness=max(0,sceneDepth-waterDepth);
                float2 refracted=uv+n.xz*0.012*saturate(thickness);
                // Avoid pulling foreground geometry into the water at pool edges.
                float behind=LinearEyeDepth(SampleSceneDepth(refracted),_ZBufferParams);
                refracted=behind<waterDepth ? uv : refracted;
                half3 scene=SampleSceneColor(refracted);
                half3 tint=lerp(_Shallow.rgb,_Deep.rgb,1-exp(-thickness*0.20));
                half3 v=GetWorldSpaceNormalizeViewDir(i.positionWS);
                float fresnel=0.045+0.72*pow(1-saturate(dot(n,v)),4);
                half3 reflected=SAMPLE_TEXTURECUBE_LOD(_Reflection,sampler_Reflection,reflect(-v,n),0.8).rgb;
                half3 color=lerp(scene,tint,1-exp(-thickness*0.48));
                color=lerp(color,reflected,fresnel);
                Light light=GetMainLight(); half3 h=normalize(light.direction+v);
                color+=light.color*pow(saturate(dot(n,h)),180)*0.55;
                float foam=(1-smoothstep(0.005,0.085,thickness))*0.08;
                color+=foam;
                return half4(MixFog(color,i.fog),1);
            }
            ENDHLSL
        }
    }
}
