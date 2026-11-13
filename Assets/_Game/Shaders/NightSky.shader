// Gece gökyüzü: tepe-ufuk geçişi, şehir ışığı, yıldızlar, ay ve uzak şehir silueti.
// Doku kullanmaz; her şey bakış yönünden hesaplanır. Kıvrımlı dünyadan etkilenmez (gökyüzü sabittir).
// Sis rengi ufuk rengine yakın tutulmalı ki uzaktaki nesneler gökyüzüne karışsın.
Shader "Out of Office/Night Sky"
{
    Properties
    {
        _ZenithColor("Tepe rengi", Color) = (0.015, 0.02, 0.07, 1)
        _HorizonColor("Ufuk rengi", Color) = (0.13, 0.08, 0.27, 1)
        _GroundColor("Ufkun altı", Color) = (0.05, 0.05, 0.12, 1)
        _Exponent("Geçiş eğrisi", Range(0.1, 2)) = 0.42
        _GlowColorA("Şehir ışığı A", Color) = (0.8, 0.25, 0.85, 1)
        _GlowColorB("Şehir ışığı B", Color) = (0.2, 0.75, 1, 1)
        _GlowStrength("Şehir ışığı gücü", Range(0, 2)) = 0.3
        _GlowFalloff("Şehir ışığı yüksekliği (büyük = alçak)", Range(1, 30)) = 11
        _StarDensity("Yıldız sıklığı", Range(0, 0.2)) = 0.06
        _StarBrightness("Yıldız parlaklığı", Range(0, 4)) = 1.6
        _MoonDirection("Ay yönü", Vector) = (0.1, 0.2, 1, 0)
        [HDR] _MoonColor("Ay rengi", Color) = (0.7, 0.88, 1, 1)
        _MoonSize("Ay yarıçapı (derece)", Range(0.5, 12)) = 3.6
        _SkylineHeight("Siluet yüksekliği", Range(0, 0.3)) = 0.085
        _SkylineColor("Siluet rengi", Color) = (0.035, 0.035, 0.09, 1)
        [HDR] _WindowColor("Pencere ışığı", Color) = (1.2, 0.85, 0.45, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex SkyVertex
            #pragma fragment SkyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ZenithColor;
                half4 _HorizonColor;
                half4 _GroundColor;
                half4 _GlowColorA;
                half4 _GlowColorB;
                half4 _MoonColor;
                half4 _SkylineColor;
                half4 _WindowColor;
                float4 _MoonDirection;
                half _Exponent;
                half _GlowStrength;
                half _GlowFalloff;
                half _StarDensity;
                half _StarBrightness;
                half _MoonSize;
                half _SkylineHeight;
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

            Varyings SkyVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
                return output;
            }

            float Hash3(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float Hash1(float n)
            {
                return frac(sin(n * 127.1) * 43758.5453);
            }

            half4 SkyFragment(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.direction);
                float height = dir.y;
                float up = saturate(height);
                float azimuth = atan2(dir.x, dir.z);

                // Tepe-ufuk geçişi.
                half3 color = lerp(_HorizonColor.rgb, _ZenithColor.rgb, pow(up, _Exponent));

                // Şehir ışığı: ufka yapışık, yöne göre iki renk arasında gezinir.
                half band = exp(-up * _GlowFalloff);
                // Koşu yönünde (azimut 0) A rengi baskındır; sis rengi de ona göre seçilir.
                half3 glowColor = lerp(_GlowColorA.rgb, _GlowColorB.rgb, 0.5 + 0.5 * sin(azimuth * 2.0 - 1.2));
                color += glowColor * (band * _GlowStrength);

                // Yıldızlar: yön uzayında hücrelere bölünür, her hücrede en fazla bir yıldız. Ufka doğru söner.
                float3 cellPosition = dir * 70.0;
                float3 cell = floor(cellPosition);
                float random = Hash3(cell);
                float starShape = saturate(1.0 - length(frac(cellPosition) - 0.5) * 3.2);
                float twinkle = 0.7 + 0.3 * sin(_Time.y * (1.5 + random * 3.0) + random * 40.0);
                float star = step(1.0 - _StarDensity, random) * starShape * starShape * twinkle;
                color += star * _StarBrightness * smoothstep(0.08, 0.35, up);

                // Ay: disk + iki katmanlı hale.
                float moonDot = dot(dir, normalize(_MoonDirection.xyz));
                float moonRadius = cos(radians(_MoonSize));
                float disc = smoothstep(moonRadius, lerp(moonRadius, 1.0, 0.12), moonDot);
                float halo = pow(saturate(moonDot), 90.0) * 0.35 + pow(saturate(moonDot), 900.0) * 0.5;
                color = lerp(color, _MoonColor.rgb, disc) + _MoonColor.rgb * halo * (1.0 - disc);

                // Uzak şehir silueti: yön boyunca sütunlar, her sütunun rastgele boyu ve seyrek pencereleri.
                float column = floor(azimuth * 34.0);
                float columnHeight = _SkylineHeight * (0.3 + 0.7 * Hash1(column)) * (0.75 + 0.25 * Hash1(floor(azimuth * 9.0) + 31.0));
                float inside = step(height, columnHeight) * step(0.0, height);
                float2 windowCell = floor(float2(azimuth * 34.0 * 6.0, height * 420.0));
                float lit = step(0.86, Hash1(windowCell.x * 13.7 + windowCell.y * 71.3)) * step(height, columnHeight - 0.004);
                half3 skyline = _SkylineColor.rgb + _WindowColor.rgb * lit * 0.55 + glowColor * 0.05;
                color = lerp(color, skyline, inside);

                // Ufkun altı: zemin rengine yumuşak geçiş (sisle örtülür, yine de dikiş görünmesin).
                color = lerp(color, _GroundColor.rgb, smoothstep(0.0, -0.08, height));

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
