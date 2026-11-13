#ifndef OOO_CURVED_LIT_INPUT_INCLUDED
#define OOO_CURVED_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "CurvedWorld.hlsl"

// SRP Batcher için tüm materyal değerleri tek blokta ve her geçişte aynı olmalı.
CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _EmissionColor;
    half _Smoothness;
    half _Metallic;
    half _GlowStrength;
CBUFFER_END

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

// URP'nin GetVertexPositionInputs işlevinin bükülmüş hali.
VertexPositionInputs GetCurvedVertexPositionInputs(float3 positionOS)
{
    VertexPositionInputs input;
    input.positionWS = BendWorldPosition(TransformObjectToWorld(positionOS));
    input.positionVS = TransformWorldToView(input.positionWS);
    input.positionCS = TransformWorldToHClip(input.positionWS);

    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;
    return input;
}

#endif
