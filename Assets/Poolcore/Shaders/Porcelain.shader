Shader "Poolcore/Porcelain"
{
    Properties
    {
        _BaseColor("Porcelain", Color) = (0.78,0.82,0.79,1)
        _BaseMap("Base", 2D) = "white" {}
        _TileSize("Tile size (metres)", Float) = 0.32
        _Grout("Grout", Color) = (0.32,0.42,0.40,1)
        _GroutStrength("Grout contrast", Range(0,1)) = 0.30
        _TileRatio("Tile aspect", Vector) = (1,1,0,0)
        _GroutWidth("Joint half-width in tile UV", Float) = 0.009
        _TileVariation("Tile tone variation", Range(0,0.1)) = 0.02
        _RoughnessVariation("Glaze variation", Range(0,0.2)) = 0
        _UpperSurface("Use matte upper wall", Range(0,1)) = 0
        _TileHeight("Wainscot height", Float) = 2.4
        _UpperColor("Upper wall", Color) = (0.82,0.82,0.77,1)
        _WetEdge("Damp pool edge", Range(0,1)) = 0
        _WetBounds("Pool XZ min/max", Vector) = (44,2,72,30)
        _WetWidth("Damp edge width", Float) = 0.65
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
                float _TileSize, _Smoothness, _Caustics, _GroutStrength;
                float4 _TileRatio, _UpperColor, _WetBounds;
                float _GroutWidth, _TileVariation, _RoughnessVariation;
                float _UpperSurface, _TileHeight, _WetEdge, _WetWidth;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv1:TEXCOORD1; };
            struct V { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 2);
            };
            V Vert(A a)
            {
                V o=(V)0; VertexPositionInputs p=GetVertexPositionInputs(a.positionOS.xyz);
                o.positionCS=p.positionCS; o.positionWS=p.positionWS;
                o.normalWS=TransformObjectToWorldNormal(a.normalOS);
                OUTPUT_LIGHTMAP_UV(a.uv1,unity_LightmapST,o.lightmapUV);
                OUTPUT_SH(o.normalWS,o.vertexSH);
                return o;
            }
            half4 Frag(V i):SV_Target
            {
                half3 n=normalize(i.normalWS); float3 an=abs(n);
                float2 uv= an.y>0.5 ? i.positionWS.xz : (an.x>0.5 ? i.positionWS.zy : i.positionWS.xy);
                uv/=max(_TileSize*_TileRatio.xy,0.02);
                float2 edge=min(frac(uv),1-frac(uv));
                float2 aa=max(fwidth(uv),0.0001);
                // Filter a fixed-width joint instead of expanding it with distance.
                float2 joint=1-smoothstep(_GroutWidth-aa*.5,_GroutWidth+aa*.5,edge);
                float grout=max(joint.x,joint.y)*(1-smoothstep(.15,.65,max(aa.x,aa.y)));
                float variation=frac(sin(dot(floor(uv),float2(127.1,311.7)))*43758.5453);
                half3 color=lerp(_BaseColor.rgb*(1+(variation*2-1)*_TileVariation),_Grout.rgb,grout*_GroutStrength);
                float upper=_UpperSurface*smoothstep(_TileHeight-.025,_TileHeight+.025,i.positionWS.y);
                color=lerp(color,_UpperColor.rgb,upper);
                float glaze=lerp(saturate(_Smoothness+(variation*2-1)*_RoughnessVariation),.12,grout);
                glaze=lerp(glaze,.16,upper);
                float2 outside=max(max(_WetBounds.xy-i.positionWS.xz,i.positionWS.xz-_WetBounds.zw),0);
                float damp=_WetEdge*(1-smoothstep(.08,max(_WetWidth,.09),length(outside)))*saturate(n.y);
                damp*=smoothstep(-.02,.02,i.positionWS.y);
                color*=1-damp*.055;glaze=lerp(glaze,.68,damp);
                SurfaceData s=(SurfaceData)0; s.albedo=color; s.alpha=1; s.smoothness=glaze; s.occlusion=1;
                s.emission=0; // Scene lighting replaces the artificial animated lines.
                InputData d=(InputData)0; d.positionWS=i.positionWS; d.normalWS=n;
                d.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.positionWS);
                d.shadowCoord=TransformWorldToShadowCoord(i.positionWS);
                d.bakedGI=SAMPLE_GI(i.lightmapUV,i.vertexSH,n);
                d.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);
                d.shadowMask=SAMPLE_SHADOWMASK(i.lightmapUV);
                // Oblique reflection projections distort clip-space Z. Fog must use
                // actual view depth, evaluated per pixel across these large walls.
                float fogDepth=max(-TransformWorldToView(i.positionWS).z-_ProjectionParams.y,0);
                half4 c=UniversalFragmentPBR(d,s);
                c.rgb=MixFog(c.rgb,ComputeFogFactorZ0ToFar(fogDepth)); return c;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
        UsePass "Universal Render Pipeline/Lit/Meta"
    }
}
