#ifndef OOO_CURVED_WORLD_INCLUDED
#define OOO_CURVED_WORLD_INCLUDED

// Kıvrımlı dünya: yol aslında dümdüzdür; yalnızca ekrana çizilirken uzaktaki köşe noktaları
// yana ve aşağı kaydırılır. Oynanış, çarpışma ve şeritler bundan etkilenmez.
// Değerleri WorldCurveSystem her karede yazar.
//   _OooCurve.x : yana kıvrım   (m / m^2, + sağ)
//   _OooCurve.y : düşey kıvrım  (m / m^2, - aşağı)
//   _OooCurve.z : kameradan itibaren düz kalan mesafe (m)
float4 _OooCurve;
float4 _OooCurveOrigin;

float3 BendWorldPosition(float3 positionWS)
{
    float ahead = max(0.0, positionWS.z - _OooCurveOrigin.z - _OooCurve.z);
    positionWS.xy += _OooCurve.xy * (ahead * ahead);
    return positionWS;
}

#endif
