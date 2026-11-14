// Gündüz gökyüzü: tepe-ufuk geçişi, güneş ve yavaşça kayan pamuk bulutlar.
// Doku kullanmaz; her şey bakış yönünden hesaplanır. Kıvrımlı dünyadan etkilenmez (gökyüzü sabittir).
// Ufkun altı ufuk renginde kalır ve sis rengi de aynıdır: yol aşağı kıvrılırken uzaktaki
// nesneler gökyüzüne karışır, hiçbir şey havada asılı görünmez. Bu yüzden gökyüzüne bina,
// dağ gibi "yere basan" şekil çizilmez.
Shader "Out of Office/Sky"
{
    Properties
    {
        _ZenithColor("Tepe rengi", Color) = (0.2, 0.52, 0.96, 1)
        _HorizonColor("Ufuk rengi (sis ile aynı)", Color) = (0.8, 0.92, 1, 1)
        _Exponent("Geçiş eğrisi", Range(0.1, 2)) = 0.55
        _SunDirection("Güneş yönü", Vector) = (0.4, 0.75, -0.5, 0)
        [HDR] _SunColor("Güneş rengi", Color) = (1.6, 1.45, 1.1, 1)
        _SunSize("Güneş yarıçapı (derece)", Range(0.5, 12)) = 3
        _CloudColor("Bulut rengi", Color) = (1, 1, 1, 1)
        _CloudShadeColor("Bulut gölgesi", Color) = (0.86, 0.92, 1, 1)
        _CloudCount("Ufuk çevresindeki bulut hücresi (tam sayı)", Range(4, 32)) = 14
        _CloudHeightScale("Bulutların dikey sıklığı", Range(1, 20)) = 8
        _CloudCoverage("Bulut miktarı", Range(0, 1)) = 0.45
        _CloudSoftness("Bulut kenar yumuşaklığı", Range(0.01, 0.5)) = 0.1
        _CloudSpeed("Bulut hızı", Range(0, 0.2)) = 0.012
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
                half4 _SunColor;
                half4 _CloudColor;
                half4 _CloudShadeColor;
                float4 _SunDirection;
                half _Exponent;
                half _SunSize;
                half _CloudCount;
                half _CloudHeightScale;
                half _CloudCoverage;
                half _CloudSoftness;
                half _CloudSpeed;
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

            // Sinüssüz karma: mobil GPU'larda da aynı sonucu verir.
            float Hash2(float2 p)
            {
                float3 q = frac(float3(p.xyx) * 0.1031);
                q += dot(q, q.yzx + 33.33);
                return frac((q.x + q.y) * q.z);
            }

            // x yönünde "period" hücrede bir kendini tekrar eder: ufuk çevresinde dikiş olmaz.
            float ValueNoise(float2 p, float period)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float x0 = cell.x - period * floor(cell.x / period);
                float x1 = x0 + 1.0;
                x1 -= period * step(period, x1);
                float a = Hash2(float2(x0, cell.y));
                float b = Hash2(float2(x1, cell.y));
                float c = Hash2(float2(x0, cell.y + 1.0));
                float d = Hash2(float2(x1, cell.y + 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Üç katman yeter: bulutlar tombul ve yumuşak, ince ayrıntı gerekmez.
            float CloudNoise(float2 p, float period)
            {
                float n = ValueNoise(p, period) * 0.55;
                n += ValueNoise(p * 2.0 + float2(0.0, 17.0), period * 2.0) * 0.3;
                n += ValueNoise(p * 4.0 + float2(0.0, 41.0), period * 4.0) * 0.15;
                return n;
            }

            half4 SkyFragment(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.direction);
                float height = dir.y;
                float up = saturate(height);

                // Tepe-ufuk geçişi. Ufkun altında "up" sıfırdır: renk ufuk renginde kalır.
                half3 color = lerp(_HorizonColor.rgb, _ZenithColor.rgb, pow(up, _Exponent));

                // Güneş: disk + geniş yumuşak hale.
                float3 sunDir = normalize(_SunDirection.xyz);
                float sunDot = dot(dir, sunDir);
                float sunRadius = cos(radians(_SunSize));
                float disc = smoothstep(sunRadius, lerp(sunRadius, 1.0, 0.25), sunDot);
                float halo = pow(saturate(sunDot), 24.0) * 0.25 + pow(saturate(sunDot), 300.0) * 0.5;
                color += _SunColor.rgb * (disc + halo);

                // Bulutlar: ufkun üstündeki kuşakta, yön (azimut) ve yüksekliğe göre dizilir. Böylece
                // koşarken görünen alçak gökyüzünde tek tek, yatık pamuk bulutlar olur; düz bir
                // katmana izdüşürmek ufka doğru onları çizgi gibi gererdi. Tepede bulut yoktur.
                float period = floor(_CloudCount + 0.5);
                float azimuth = atan2(dir.x, dir.z);
                float2 cloudUV = float2((azimuth / TWO_PI + 0.5) * period + _Time.y * _CloudSpeed, height * _CloudHeightScale);
                float noise = CloudNoise(cloudUV, period);
                float edge = 1.0 - _CloudCoverage;
                float band = smoothstep(0.03, 0.12, height) * (1.0 - smoothstep(0.3, 0.55, height));
                float density = smoothstep(edge, edge + _CloudSoftness, noise) * band;
                // Bulutun ortası aydınlık, kenarı ve altı gökyüzünün gölge tonunda.
                half3 cloud = lerp(_CloudShadeColor.rgb, _CloudColor.rgb, smoothstep(edge, edge + _CloudSoftness * 3.5, noise));
                color = lerp(color, cloud, density * 0.92);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
