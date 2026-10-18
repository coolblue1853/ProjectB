using DG.Tweening;
using UnityEngine;

/// <summary>
/// 보스 페이즈 (QualityRoadmap C4) — BT 를 건드리지 않고 코드로: HP 50% 에서 2페이즈, 25% 에서 3페이즈.
/// 페이즈마다 애니 속도(=공격 템포) ×1.25/×1.5, 공격 피해 ×1.2/×1.4, 격노 틴트·화면 흔들림·"ENRAGED" 배너.
/// EnemyHealth 가 보스면 자동 부착(isBossMob), 풀 반환 시 초기화.
/// </summary>
public class BossPhase : MonoBehaviour
{
    public static readonly float[] Thresholds = { 0.5f, 0.25f };
    static readonly float[] SpeedMul = { 1f, 1.25f, 1.5f };
    static readonly float[] DamageMul = { 1f, 1.2f, 1.4f };
    static readonly Color[] Tint = { Color.white, new Color(1f, 0.85f, 0.8f), new Color(1f, 0.65f, 0.6f) };

    EnemyHealth health;
    Animator animator;
    SpriteRenderer sr;
    SpriteHitFx fx;
    int phase; // 0 = 1페이즈

    public int Phase => phase;
    public float DamageMultiplier => DamageMul[phase];

    public static BossPhase AttachIfBoss(EnemyHealth enemy)
    {
        if (!enemy.isBossMob)
        {
            return null;
        }
        BossPhase p = enemy.GetComponent<BossPhase>();
        if (p == null)
        {
            p = enemy.gameObject.AddComponent<BossPhase>();
        }
        return p;
    }

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        fx = GetComponent<SpriteHitFx>();
    }

    void OnEnable()
    {
        phase = 0;
        Apply(false);
    }

    /// <summary>피격 후 EnemyHealth 가 호출. 임계값을 넘으면 페이즈 상승(되돌리지 않음)</summary>
    public void OnDamaged()
    {
        if (health == null || health.maxHP <= 0)
        {
            return;
        }
        float ratio = (float)health.nowHP / health.maxHP;
        int target = phase;
        for (int i = 0; i < Thresholds.Length; i++)
        {
            if (ratio <= Thresholds[i])
            {
                target = i + 1;
            }
        }
        if (target > phase)
        {
            phase = target;
            Apply(true);
        }
    }

    void Apply(bool announce)
    {
        if (animator != null)
        {
            animator.speed = SpeedMul[phase];
        }
        if (sr != null)
        {
            sr.color = Tint[phase];
        }
        if (!announce)
        {
            return;
        }
        // 격노 연출: 플래시 2회 + 흔들림 + 배너 + SFX
        if (fx != null)
        {
            fx.Flash(0.12f, 0.8f, 1.6f, new Color(1f, 0.3f, 0.2f));
            DOVirtual.DelayedCall(0.25f, () => fx.Flash(0.12f, 0.8f, 1.6f, new Color(1f, 0.3f, 0.2f)), false).SetLink(gameObject);
        }
        transform.DOKill(true);
        transform.DOPunchScale(Vector3.one * 0.12f, 0.35f, 6, 0.6f).SetLink(gameObject);
        HitFeedback.Shake(HitFeedback.ShakeTier.Heavy);
        Sfx.Play(SfxId.BossSlam, 0.7f);
        string name = health.name.Replace("(Clone)", "").Replace("_BD", "").Trim();
        BossBanner.Show(name, phase == 1 ? "ENRAGED — Phase 2" : "FRENZY — Phase 3", true);
        GameLog.Info(GameLog.Cat.AI, "보스 페이즈 " + (phase + 1) + ": " + health.name + " HP " + health.nowHP + "/" + health.maxHP);
    }

    void Update()
    {
        // 히트스톱(속도 0)이 끝나며 원래 속도로 되돌려도 페이즈 속도를 유지
        if (animator != null && animator.speed > 0.01f && !Mathf.Approximately(animator.speed, SpeedMul[phase]))
        {
            animator.speed = SpeedMul[phase];
        }
    }

    void OnDisable()
    {
        phase = 0;
        if (animator != null)
        {
            animator.speed = 1f;
        }
        if (sr != null)
        {
            sr.color = Color.white;
        }
    }
}
