// 무기 평타 도트 이펙트 셰이더 (shader-creator-personal 절차로 제작, 의도 계약: Tools/shader_jobs/pixel_slash/intent.json)
// 입력: PixelLab vfx_workbench 로 그린 회색조 프레임(밝기 = 강도) → 테마 램프 5색의 평면 단계색으로 칠한다(그라데이션 없음).
// 바깥 테두리 1텍셀은 램프 진한 색(_Rim). _Dissolve 0..1: 텍셀 단위로 흩어지며 사라지고, 사라지기 직전 텍셀은 밝은 색(_C3)으로 빛난다. Vfx 가 수명 끝 40% 를 구동.
// LightMode 생략(SRPDefaultUnlit) = 자체발광: 밤·숲 2D 조명에도 원색.
Shader "ProjectB/PixelSlash2D"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [Header(Theme Ramp dark to light)]
        _C0 ("Ramp 0 (dark)", Color) = (0.235, 0.235, 0.314, 1)
        _C1 ("Ramp 1", Color) = (0.510, 0.549, 0.667, 1)
        _C2 ("Ramp 2", Color) = (0.784, 0.824, 0.902, 1)
        _C3 ("Ramp 3", Color) = (0.922, 0.941, 0.980, 1)
        _C4 ("Ramp 4 (core)", Color) = (1, 1, 1, 1)
        [Header(Levels)]
        _Lift ("Brightness Lift", Range(-0.3, 0.3)) = 0.05
        [Header(Rim)]
        _Rim ("Outer Rim (1 texel, ramp 1)", Range(0, 1)) = 1
        [Header(Dissolve)]
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _EdgeBand ("Dissolve Edge Band", Range(0, 0.3)) = 0.12
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }

        Pass
        {
            Name "PixelSlash"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                half4 _C0, _C1, _C2, _C3, _C4;
                half _Lift;
                half _Rim;
                half _Dissolve, _EdgeBand;
            CBUFFER_END
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };

            float hash21(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half lum = saturate(dot(tex.rgb, half3(0.299h, 0.587h, 0.114h)) + _Lift);

                // 밝기 → 5단계 평면색 (분기 없이 step 누적)
                half4 col = _C0;
                col = lerp(col, _C1, step(0.25h, lum));
                col = lerp(col, _C2, step(0.50h, lum));
                col = lerp(col, _C3, step(0.72h, lum));
                col = lerp(col, _C4, step(0.90h, lum));

                // 바깥 테두리 1텍셀 = 램프 진한 색(_C1): 흰 테마도 밝은 배경에서 윤곽이 산다(상업 도트 이펙트 관례)
                float2 ts = _MainTex_TexelSize.xy;
                half nb = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(ts.x, 0)).a;
                nb = min(nb, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - float2(ts.x, 0)).a);
                nb = min(nb, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, ts.y)).a);
                nb = min(nb, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - float2(0, ts.y)).a);
                half rim = step(nb, 0.01h) * _Rim;
                col = lerp(col, _C1, rim);

                // 텍셀 단위 디졸브: 어두운 가장자리부터 얇아져 밝은 심만 남았다 꺼진다(밝기 65% + 해시 35% — 해시만 쓰면 고른 점으로 흩어져 싸 보였다)
                float2 texel = floor(i.uv * _MainTex_TexelSize.zw);
                half h = lum * 0.65h + (half)hash21(texel) * 0.35h;
                half keep = step(_Dissolve, h);
                half edge = keep * step(h, _Dissolve + _EdgeBand) * step(0.001h, _Dissolve);
                col = lerp(col, _C3, edge);

                col.a = tex.a * keep * i.color.a;
                col.rgb *= i.color.rgb;
                return col;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
