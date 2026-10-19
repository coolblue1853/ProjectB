using UnityEngine;

/// <summary>
/// 스프라이트 피격 연출: 흰 플래시(All In 1 HITEFFECT), 국소 히트스톱(Animator 정지), 디졸브(FADE).
/// 몬스터에 런타임으로 붙는다(프리팹 수정 불필요). 원래 머티리얼이 All In 1이 아니면 M_EnemySprite 인스턴스로 교체한다.
/// 개체당 머티리얼 인스턴스 1개 → SRP Batcher 유지 (MaterialPropertyBlock 미사용).
/// </summary>
[DisallowMultipleComponent]
public class SpriteHitFx : MonoBehaviour
{
    static readonly int HitBlendId = Shader.PropertyToID("_HitEffectBlend");
    static readonly int HitColorId = Shader.PropertyToID("_HitEffectColor");
    static readonly int HitGlowId = Shader.PropertyToID("_HitEffectGlow");
    static readonly int FadeId = Shader.PropertyToID("_FadeAmount");
    const string FxShaderName = "AllIn1SpriteShader/AllIn1Urp2dRenderer";

    static Material enemyTemplate;
    static Material eliteTemplate;
    static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    static readonly Color EliteOutline = new Color(1f, 0.78f, 0.3f, 1f);

    SpriteRenderer spriteRenderer;
    Material material;
    Animator animator;

    float flashTimer;
    float hitStopTimer;
    float savedAnimSpeed = 1f;
    float fadeFrom, fadeTo, fadeDuration, fadeTime = -1f;

    public bool IsReady => material != null;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (spriteRenderer == null)
        {
            enabled = false;
            return;
        }
        if (spriteRenderer.sharedMaterial != null && spriteRenderer.sharedMaterial.shader.name == FxShaderName)
        {
            material = spriteRenderer.material; // 이미 FX 셰이더면 인스턴스만 만든다
            return;
        }
        if (enemyTemplate == null)
        {
            enemyTemplate = Resources.Load<Material>("FX/M_EnemySprite");
        }
        if (enemyTemplate != null)
        {
            material = new Material(enemyTemplate);
            spriteRenderer.material = material;
        }
    }

    void OnDisable()
    {
        // 풀 반환 시 상태 원복 (굳은 애니·흰색 잔상 방지)
        flashTimer = 0f;
        fadeTime = -1f;
        if (hitStopTimer > 0f && animator != null)
        {
            animator.speed = savedAnimSpeed;
        }
        hitStopTimer = 0f;
        if (material != null)
        {
            material.SetFloat(HitBlendId, 0f);
            material.SetFloat(FadeId, -0.1f);
        }
    }

    void OnDestroy()
    {
        if (material != null)
        {
            Destroy(material);
        }
    }

    /// <summary>
    /// 정예 금빛 외곽선 (All In 1 OUTBASE·픽셀 퍼펙트 1px). 키워드가 빌드에 남도록 키워드를 켠 재질 에셋
    /// Resources/FX/M_EnemySpriteElite 로 갈아 끼운다(Tools/probes/make_elite_material.cs). 해제하면 기본 재질로 돌아간다.
    /// </summary>
    public void SetElite(bool on)
    {
        if (spriteRenderer == null)
        {
            return;
        }
        if (on && eliteTemplate == null)
        {
            eliteTemplate = Resources.Load<Material>("FX/M_EnemySpriteElite");
        }
        Material template = on ? eliteTemplate : enemyTemplate;
        if (template == null)
        {
            return;
        }
        if (material != null)
        {
            Destroy(material);
        }
        material = new Material(template);
        if (on)
        {
            material.SetColor(OutlineColorId, EliteOutline);
        }
        spriteRenderer.material = material;
    }

    /// <summary>흰색(또는 color) 플래시. 도트는 하드 컷이 선명하므로 duration 동안 유지 후 즉시 끈다.</summary>
    public void Flash(float duration, float blend = 1f, float glow = 1f, Color? color = null)
    {
        if (material == null || !HitFeedback.FlashEnabled)
        {
            return;
        }
        material.SetColor(HitColorId, color ?? Color.white);
        material.SetFloat(HitGlowId, glow);
        material.SetFloat(HitBlendId, blend);
        flashTimer = Mathf.Max(flashTimer, duration);
    }

    /// <summary>국소 히트스톱: 이 개체의 애니메이션만 잠깐 멈춘다(전역 timeScale 미사용). 누적하지 않고 max로 갱신.</summary>
    public void HitStop(float seconds)
    {
        if (animator == null || seconds <= 0f)
        {
            return;
        }
        if (hitStopTimer <= 0f)
        {
            savedAnimSpeed = animator.speed;
            animator.speed = 0f;
        }
        hitStopTimer = Mathf.Max(hitStopTimer, seconds);
    }

    /// <summary>디졸브 (-0.1 = 온전, 1 = 소멸)</summary>
    public void Dissolve(float duration, float from = -0.1f, float to = 1f)
    {
        if (material == null)
        {
            return;
        }
        fadeFrom = from;
        fadeTo = to;
        fadeDuration = Mathf.Max(0.01f, duration);
        fadeTime = 0f;
        material.SetFloat(FadeId, from);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (flashTimer > 0f)
        {
            flashTimer -= dt;
            if (flashTimer <= 0f && material != null)
            {
                material.SetFloat(HitBlendId, 0f);
            }
        }
        if (hitStopTimer > 0f)
        {
            hitStopTimer -= dt;
            if (hitStopTimer <= 0f && animator != null)
            {
                animator.speed = savedAnimSpeed;
            }
        }
        if (fadeTime >= 0f && material != null)
        {
            fadeTime += dt;
            float t = Mathf.Clamp01(fadeTime / fadeDuration);
            material.SetFloat(FadeId, Mathf.Lerp(fadeFrom, fadeTo, t));
            if (t >= 1f)
            {
                fadeTime = -1f;
            }
        }
    }

    /// <summary>프로브·테스트용</summary>
    public float CurrentFlashBlend => material != null ? material.GetFloat(HitBlendId) : -1f;
    public float CurrentAnimSpeed => animator != null ? animator.speed : -1f;
}
