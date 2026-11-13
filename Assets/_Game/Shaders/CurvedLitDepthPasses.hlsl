#ifndef OOO_CURVED_LIT_DEPTH_PASSES_INCLUDED
#define OOO_CURVED_LIT_DEPTH_PASSES_INCLUDED

// Gölge, derinlik ve derinlik+normal geçişleri. Hepsi aynı bükmeyi uygular; yoksa gölgeler
// ve ortam kararması (SSAO) bükülmemiş konumda kalırdı.

#include "CurvedLitInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float3 normalWS : TEXCOORD0;
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

// ---------------------------------------------------------------- Gölge
float3 _LightDirection;
float3 _LightPosition;

Varyings CurvedShadowVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    float3 positionWS = BendWorldPosition(TransformObjectToWorld(input.positionOS.xyz));
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
    output.positionCS = ApplyShadowClamping(positionCS);
    return output;
}

half4 CurvedShadowFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    return 0;
}

// ---------------------------------------------------------------- Derinlik
Varyings CurvedDepthVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.positionCS = TransformWorldToHClip(BendWorldPosition(TransformObjectToWorld(input.positionOS.xyz)));
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    return output;
}

half CurvedDepthFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    return input.positionCS.z;
}

half4 CurvedDepthNormalsFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#if defined(_GBUFFER_NORMALS_OCT)
    float3 normalWS = normalize(input.normalWS);
    float2 octNormalWS = PackNormalOctQuadEncode(normalWS);
    float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
    half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
    return half4(packedNormalWS, 0.0);
#else
    return half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
#endif
}

#endif
