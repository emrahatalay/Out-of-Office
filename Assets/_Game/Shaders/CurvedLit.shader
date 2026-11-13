// Kıvrımlı dünya için ışıklı (PBR) yüzey. URP/Lit ile aynı aydınlatmayı kullanır; tek farkı
// köşe noktalarını CurvedWorld.hlsl ile bükmesidir. Oyun dünyasındaki tüm modeller bu
// shader'ı (ya da CurvedWorld.hlsl kullanan başka bir shader'ı) kullanmalıdır; yoksa
// kıvrılan yolun üstünde düz kalırlar.
Shader "Out of Office/Curved Lit"
{
    Properties
    {
        [MainTexture] _BaseMap("Doku", 2D) = "white" {}
        [MainColor] _BaseColor("Renk", Color) = (1, 1, 1, 1)
        _Smoothness("Parlaklık", Range(0, 1)) = 0.25
        _Metallic("Metalik", Range(0, 1)) = 0
        [HDR] _EmissionColor("Işıma", Color) = (0, 0, 0, 1)
        _GlowStrength("Doku ışıması (alfa maskesi: 0 = ışır)", Range(0, 8)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex CurvedLitVertex
            #pragma fragment CurvedLitFragment

            // URP anahtar kelimeleri (URP/Lit ile aynı aydınlatma yolları)
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #pragma multi_compile_instancing

            #include "CurvedLitForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex CurvedShadowVertex
            #pragma fragment CurvedShadowFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "CurvedLitDepthPasses.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex CurvedDepthVertex
            #pragma fragment CurvedDepthFragment
            #pragma multi_compile_instancing

            #include "CurvedLitDepthPasses.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex CurvedDepthVertex
            #pragma fragment CurvedDepthNormalsFragment
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_instancing

            #include "CurvedLitDepthPasses.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
