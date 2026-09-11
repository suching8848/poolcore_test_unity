Shader "Poolcore/Porcelain"
{
    Properties
    {
        _BaseColor("Porcelain", Color) = (0.78,0.82,0.79,1)
        _BaseMap("Base", 2D) = "white" {}
        _TileSize("Tile size (metres)", Float) = 0.32
        _Grout("Grout", Color) = (0.32,0.42,0.40,1)
        _Smoothness("Glaze", Range(0,1)) = 0.55
        _Caustics("Water light", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor, _Grout;
                float4 _BaseMap_ST;
                float _TileSize, _Smoothness, _Caustics;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv1:TEXCOORD1; };
            struct V { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 2);
                half fog:TEXCOORD3;
            };
            V Vert(A a)
            {
                V o=(V)0; VertexPositionInputs p=GetVertexPositionInputs(a.positionOS.xyz);
                o.positionCS=p.positionCS; o.positionWS=p.positionWS;
                o.normalWS=TransformObjectToWorldNormal(a.normalOS);
                OUTPUT_LIGHTMAP_UV(a.uv1,unity_LightmapST,o.lightmapUV);
                OUTPUT_SH(o.normalWS,o.vertexSH);
                o.fog=ComputeFogFactor(p.positionCS.z); return o;
            }
            half4 Frag(V i):SV_Target
            {
                half3 n=normalize(i.normalWS); float3 an=abs(n);
                float2 uv= an.y>0.5 ? i.positionWS.xz : (an.x>0.5 ? i.positionWS.zy : i.positionWS.xy);
                uv/=max(_TileSize,0.02);
                float2 edge=min(frac(uv),1-frac(uv));
                float2 aa=max(fwidth(uv),0.0001);
                float grout=1-min(smoothstep(0.009,0.009+aa.x,edge.x),smoothstep(0.009,0.009+aa.y,edge.y));
                float variation=frac(sin(dot(floor(uv),float2(127.1,311.7)))*43758.5453);
                half3 color=lerp(_BaseColor.rgb*(0.98+variation*0.04),_Grout.rgb,grout*0.30);
                float2 w=i.positionWS.xz;
                float waves=sin(w.x*3.3+sin(w.y*2.4+_Time.y*0.4))+sin(w.y*3.7+sin(w.x*2.8-_Time.y*0.3));
                float caustic=pow(saturate(1-abs(waves)*1.8),5);
                SurfaceData s=(SurfaceData)0; s.albedo=color; s.alpha=1; s.smoothness=lerp(_Smoothness,0.12,grout); s.occlusion=1;
                s.emission=half3(0.10,0.24,0.22)*caustic*_Caustics;
                InputData d=(InputData)0; d.positionWS=i.positionWS; d.normalWS=n;
                d.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.positionWS);
                d.shadowCoord=TransformWorldToShadowCoord(i.positionWS);
                d.bakedGI=SAMPLE_GI(i.lightmapUV,i.vertexSH,n);
                d.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);
                d.shadowMask=SAMPLE_SHADOWMASK(i.lightmapUV);
                half4 c=UniversalFragmentPBR(d,s); c.rgb=MixFog(c.rgb,i.fog); return c;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
        UsePass "Universal Render Pipeline/Lit/Meta"
    }
}
