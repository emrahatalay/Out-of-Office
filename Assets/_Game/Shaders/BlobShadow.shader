// Damla gölge: karakterin, kovalayanın ve engellerin altına konan yumuşak, yuvarlak koyu leke.
// Gerçek zamanlı gölge haritasının yerini alır (gölge geçişi yok, ek çizim yalnızca bir quad).
// Rengi altındaki zeminle ÇARPILIR (Blend DstColor Zero); sıralama gerekmez, üst üste binen
// gölgeler doğal biçimde koyulaşır. Köşe noktalarını CurvedWorld.hlsl ile büker; kıvrılan yolda
// zemine yapışık kalır. Uzakta sise karışarak kaybolur.
Shader "Out of Office/Blob Shadow"
{
    Properties
    {
        _ShadowColor("Gölge rengi (merkezde)", Color) = (0.38, 0.4, 0.58, 1)
        _Strength("Koyuluk", Range(0, 1)) = 0.75
        _Softness("Kenar yumuşaklığı", Range(0.05, 1)) = 0.65
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent-50"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "BlobShadow"
            Tags { "LightMode" = "UniversalForward" }

            Blend DstColor Zero
            ZWrite Off
            ZTest LEqual
            Cull Back
            // Zeminle aynı düzlemde titremesin.
            Offset -1, -1

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex BlobVertex
            #pragma fragment BlobFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include "CurvedWorld.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShadowColor;
                half _Strength;
                half _Softness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 offset : TEXCOORD0; // merkezden uzaklık: kenarda 1
                half fogFactor : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings BlobVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionWS = BendWorldPosition(TransformObjectToWorld(input.positionOS.xyz));
                output.positionCS = TransformWorldToHClip(positionWS);
                output.offset = input.uv * 2.0 - 1.0;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 BlobFragment(Varyings input) : SV_Target
            {
                half distance = length(input.offset);
                half amount = (1.0h - smoothstep(1.0h - _Softness, 1.0h, distance)) * _Strength;
                half3 color = lerp(half3(1, 1, 1), _ShadowColor.rgb, amount);
                // Sis gölgeyi beyaza (çarpmada "etkisiz") çeker: uzaktaki gölge sisle birlikte kaybolur.
                color = MixFogColor(color, half3(1, 1, 1), input.fogFactor);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
