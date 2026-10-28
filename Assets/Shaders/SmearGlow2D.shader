// 칼 스미어 받침 글로우(10-28 사용자: 도트로 큰 형태, 세부는 셰이더·파티클로 감싸기). 미리보기: Tools/pixelfx/smear_preview.py glow_map 과 같은 식.
// 입력: 스미어 SpriteRenderer 와 같은 회색조 프레임(SmearAura 가 매 프레임 복사) → 텍셀 중심 고정 원판 17탭 블러(밝기 × 알파) → 세기 → 단계 양자화(도트 계단) → 가산.
// _UVRect: 시트에서 이 프레임 영역(옆 프레임이 번져 들어오지 않게 자른다). _Arc: 호 중심(텍셀, 스프라이트 왼쪽 아래 기준)·안쪽 반지름(_ArcAxes 타원 기준) — 이보다 안쪽(머리·몸)은 꺼서 뿌옇지 않게. w = 0 이면 끔.
Shader "ProjectB/SmearGlow2D"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (0.784, 0.824, 0.902, 0.75)
        _Intensity ("Intensity", Range(0, 5)) = 2.2
        _Radius ("Radius (texels)", Range(0, 10)) = 5.5
        _Steps ("Steps (0 = smooth)", Range(0, 8)) = 4
        _UVRect ("UV Rect (min xy, max zw)", Vector) = (0, 0, 1, 1)
        _Arc ("Arc center xy (texels) / inner radius z / enable w", Vector) = (0, 0, 0, 0)
        _ArcAxes ("Ellipse radius x, y (texels) / tilt rad z", Vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }

        Pass
        {
            Name "SmearGlow"
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                half4 _GlowColor;
                half _Intensity, _Radius, _Steps;
                float4 _UVRect;
                float4 _Arc;
                float4 _ArcAxes;
            CBUFFER_END
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            half Tap(float2 uv)
            {
                float2 c = clamp(uv, _UVRect.xy, _UVRect.zw);
                half4 t = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, c);
                half inside = step(abs(c.x - uv.x) + abs(c.y - uv.y), 1e-5);
                return t.a * max(t.r, max(t.g, t.b)) * inside;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 ts = _MainTex_TexelSize.xy;
                float2 uvc = (floor(i.uv * _MainTex_TexelSize.zw) + 0.5) * ts; // 텍셀 중심 — 글로우도 도트 격자에 맞는다
                half acc = Tap(uvc);
                half wsum = 1.0h;
                [unroll] for (int k = 0; k < 8; k++)
                {
                    float a = k * 0.78539816;
                    float2 d = float2(cos(a), sin(a));
                    acc += Tap(uvc + round(d * _Radius * 0.5) * ts) * 0.7h;
                    acc += Tap(uvc + round(d * _Radius) * ts) * 0.4h;
                    wsum += 1.1h;
                }
                half g = saturate(acc / wsum * _Intensity);
                if (_Arc.w > 0.5)
                {
                    float2 texel = (uvc - _UVRect.xy) * _MainTex_TexelSize.zw;
                    // 타원 정규 거리(세로 반지름 단위) — 10-28 v7 타원 스미어
                    float2 d = texel - _Arc.xy;
                    float ct = cos(_ArcAxes.z), st = sin(_ArcAxes.z);
                    float2 l = float2(d.x * ct + d.y * st, -d.x * st + d.y * ct);
                    float nd = length(l / max(_ArcAxes.xy, 0.001)) * _ArcAxes.y;
                    g *= saturate((nd - _Arc.z) / 4.0);
                }
                g = _Steps > 0.5 ? floor(g * _Steps + 0.35) / _Steps : g;
                return half4(_GlowColor.rgb, g * _GlowColor.a * i.color.a);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
