using UnityEngine;

/// <summary>
/// 플레이어 공격·스킬 스프라이트에 ProjectB/SkillEnergy2D 셰이더를 입히고 수명 끝 디졸브를 구동한다.
/// 디졸브 진행 = Animator 현재 상태 normalizedTime 0.7→1 (애니가 없으면 생성 후 0.5초 기준).
/// 머티리얼은 전체 공유 1개, 개체별 값은 MaterialPropertyBlock (배칭·할당 최소).
/// </summary>
public class SkillEnergyFx : MonoBehaviour
{
    const float FadeFrom = 0.7f, NoAnimLife = 0.5f;
    static readonly int GlowId = Shader.PropertyToID("_GlowColor");
    static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
    static Material shared;

    SpriteRenderer sr;
    Animator animator;
    MaterialPropertyBlock block;
    Color glow;
    float born;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        shared = null;
    }

    public static void Apply(GameObject go, Color glowColor)
    {
        if (go == null || go.GetComponent<SkillEnergyFx>() != null)
        {
            return;
        }
        var sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            return;
        }
        if (shared == null)
        {
            // Resources 머티리얼로 참조해 빌드에 셰이더가 포함되게 한다 (Shader.Find 의존 회피, B-211)
            shared = Resources.Load<Material>("FX/M_SkillEnergy");
            if (shared == null)
            {
                GameLog.Warn(GameLog.Cat.Combat, "Resources/FX/M_SkillEnergy 없음");
                return;
            }
        }
        sr.sharedMaterial = shared;
        var fx = go.AddComponent<SkillEnergyFx>();
        fx.sr = sr;
        fx.animator = go.GetComponentInChildren<Animator>();
        fx.glow = glowColor;
        fx.born = Time.time;
        fx.block = new MaterialPropertyBlock();
        fx.Push(0f);
    }

    void LateUpdate()
    {
        float progress;
        if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
        {
            progress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (animator.GetCurrentAnimatorStateInfo(0).loop)
            {
                progress = 0f; // 반복 애니(장판 등)는 디졸브하지 않음
            }
        }
        else
        {
            progress = (Time.time - born) / NoAnimLife;
        }
        Push(Mathf.Clamp01((progress - FadeFrom) / (1f - FadeFrom)));
    }

    void Push(float dissolve)
    {
        sr.GetPropertyBlock(block);
        block.SetColor(GlowId, glow);
        block.SetFloat(DissolveId, dissolve);
        sr.SetPropertyBlock(block);
    }
}
