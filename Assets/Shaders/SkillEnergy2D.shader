// 스킬·공격 스프라이트 에너지 셰이더 (shader-creator-personal 절차로 제작, 의도 계약: Tools/shader_jobs/skill_energy/intent.json)
// 본체 발광(_Emission) + 흐르는 노이즈 에너지 + 알파 외곽 글로우/후광 + 수명 끝 디졸브(_Dissolve, SkillEnergyFx가 구동)
// LightMode 생략(SRPDefaultUnlit) = 자체발광: 2D Renderer·Forward 양쪽에서 그려지고, 밤(2D 라이트 어두움)에도 선명하다.
Shader "ProjectB/SkillEnergy2D"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Glow)]
        [HDR] _GlowColor ("Glow Color", Color) = (1.0, 0.85, 0.45, 1)
        _Emission ("Body Emission", Range(1, 3)) = 1.35
        _EdgeGlow ("Inner Edge Glow", Range(0, 3)) = 1.6
        _HaloAlpha ("Outer Halo", Range(0, 1)) = 0.85
        _EdgePx ("Edge Width (texels)", Range(0.5, 4)) = 2
        _Recolor ("Recolor To Glow Ramp", Range(0, 1)) = 0.7

        [Header(Energy Flow)]
        _FlowAmount ("Flow Amount", Range(0, 1)) = 0.55
        _FlowSpeed ("Flow Speed", Range(0, 6)) = 2.2
        _FlowScale ("Flow Scale", Range(1, 30)) = 9

        [Header(Dissolve)]
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _DissolveWidth ("Dissolve Edge Width", Range(0.01, 0.3)) = 0.12
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }

        Pass
        {
            Name "SkillEnergy"
            // LightMode 생략 → SRPDefaultUnlit. 2D 라이트를 받지 않는 자체발광이므로 이 쪽이 정답(2D·Forward 호환).
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
                half4 _Color;
                half4 _GlowColor;
                half _Emission, _EdgeGlow, _HaloAlpha, _EdgePx, _Recolor;
                half _FlowAmount, _FlowSpeed, _FlowScale;
                half _Dissolve, _DissolveWidth;
            CBUFFER_END
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };

            float hash21(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float vnoise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash21(i), hash21(i + float2(1, 0)), f.x),
                            lerp(hash21(i + float2(0, 1)), hash21(i + float2(1, 1)), f.x), f.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color; // SpriteRenderer.color (틴트·페이드)
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // 이웃 4탭(대각) 하나로 내부 경계(min)와 바깥 후광(max)을 함께 구한다 → 총 5샘플
                float2 o = _MainTex_TexelSize.xy * _EdgePx;
                half a1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( o.x,  o.y)).a;
                half a2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-o.x,  o.y)).a;
                half a3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( o.x, -o.y)).a;
                half a4 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-o.x, -o.y)).a;
                half nMin = min(min(a1, a2), min(a3, a4));
                half nMax = max(max(a1, a2), max(a3, a4));

                half4 vc = IN.color * _Color;
                half3 body = tex.rgb * vc.rgb * _Emission;

                // 리컬러(gradient-map): 휘도 → 테마색 램프(어두움=테마색 짙게, 밝음=흰빛). 검은 톤 스프라이트도 무기 색으로 빛나게
                // 애니메이터가 SpriteRenderer.color 를 검정으로 칠하는 스킬(어둠 계열)도 있어 tex*정점색의 휘도를 쓰고, 램프에는 정점색을 다시 곱하지 않는다
                half lum = dot(tex.rgb * vc.rgb, half3(0.299, 0.587, 0.114));
                half3 ramp = lerp(_GlowColor.rgb * 0.55, lerp(_GlowColor.rgb * 1.3, half3(1.2, 1.2, 1.15), saturate(lum * 1.4 - 0.4)), saturate(lum * 2.0));
                body = lerp(body, ramp, _Recolor);

                // 흐르는 에너지: 두 옥타브 노이즈를 대각선으로 흘려 밝기를 일렁이고, 밝은 띠는 글로우색으로
                float2 fuv = IN.uv * _FlowScale;
                float t = _Time.y * _FlowSpeed;
                float n = vnoise(fuv + float2(-t, t * 0.35)) * 0.65 + vnoise(fuv * 2.1 + float2(t * 0.6, -t)) * 0.35;
                body *= lerp(1.0, 0.65 + 0.7 * n, _FlowAmount);
                half streak = smoothstep(0.62, 0.85, n) * _FlowAmount;
                body = lerp(body, _GlowColor.rgb * 1.2, streak * 0.45 * tex.a);

                // 내부 경계 글로우: 내 알파는 있는데 이웃이 비는 곳
                half innerEdge = saturate(tex.a - nMin);
                body += _GlowColor.rgb * innerEdge * _EdgeGlow;

                // 디졸브: 노이즈 아래 픽셀을 버리고 경계를 글로우색으로
                float dn = vnoise(IN.uv * 14.0);
                half alive = step(_Dissolve, dn);
                half dEdge = (1.0 - saturate((dn - _Dissolve) / _DissolveWidth)) * step(0.001, _Dissolve);
                body = lerp(body, _GlowColor.rgb * 1.5, dEdge * alive);

                half bodyA = tex.a * alive;

                // 바깥 후광: 내 알파가 없고 이웃에 알파가 있는 곳 (디졸브 진행에 따라 함께 사라짐)
                half halo = saturate(nMax - tex.a) * _HaloAlpha * (1.0 - _Dissolve);

                half outA = saturate(bodyA + halo * (1.0 - bodyA));
                half3 outRgb = (body * bodyA + _GlowColor.rgb * halo * (1.0 - bodyA)) / max(outA, 1e-3);
                return half4(outRgb, outA * vc.a);
            }
            ENDHLSL
        }
    }
    Fallback "Sprites/Default"
}
