using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using AnyPortrait;
using DarkTonic.MasterAudio;
public class Weapon : MonoBehaviour
{
    public PlayerController pC;
    public int maxComboCount = 0 ;
    public int nowConboCount = 0;
    public float maxComboTime = 0; // 콤보 최대 유지시간

    public int minDmg=0;        // 최소댐
    public int maxDmg=0;       // 최대댐
    public int critPer = 0;   //  치명 확률%
    public int critDmg = 0;   //  치명피해%
    public int incDmg = 0;   // 데미지증가%
    public int ignDef = 0; // 방어력 무시%
    public int skillDmg = 0; // 스킬 공격력%
    public int finDmg = 0; // 최종뎀%  
    public int addDmg = 0; // 추뎀%  
    PlayerHealthManager phm;
    public string[] weaponSound;
    public int[] damgeArray = new int[10];
    public float[] attckSpeed;   // 공격 주기, 짧을수록 더 빠르게 공격 가능.
    public bool isAttackWait = true;
    public bool isSkillAttackWait = true;
    public bool isWeaponStopMove = false;
    [Tooltip("평타 후딜 중 좌우 이동 속도 배율(1=평소). isWeaponStopMove가 꺼진 무기에만 의미가 있다")]
    [SerializeField, Range(0f, 1f)] private float _attackMoveSpeedMul = 1f;
    public float time;
    Sequence comboSequence;
    public bool isOverHand =false;

    [Tooltip("평타 휘두를 때 공격 지점에 띄울 베기 이펙트 (Resources/FX/VfxLibrary)")]
    public VfxId swingVfx = VfxId.SlashWhite;
    public float swingVfxScale = 1f;
    [Tooltip("공격·스킬 스프라이트 에너지 셰이더(ProjectB/SkillEnergy2D) 글로우 색")]
    [ColorUsage(true, true)] public Color energyColor = new Color(1f, 0.85f, 0.45f, 1f);
    public bool useEnergyShader = true;
    public GameObject[] attackPrefab;
    public GameObject[] attackPivot;
    public bool isHasDelayTime = false;
    [ConditionalHide("isHasDelayTime")]
    public float[] delayTime;
    [ConditionalHide("isHasDelayTime")]
    public string[] delayAnimName;

    public string[] attackAnimName;
    const int ComboFinisherMinSteps = 3;
    const float ComboFinisherScale = 1.25f;
    public bool[] spriteReverse;
    public Skill[] skill = new Skill[4];

    public GameObject leftWeqponSprite;
    public GameObject RightWeqponSprite;
    [Header("셀 동작 무기 프로파일(MotionSpec39 §3·§21, 10-30)")]
    [Tooltip("준비 구간(타격 프레임 앞) 길이 배율 — 무거운 무기 1.2~1.4, 단검 0.7~0.8")]
    [SerializeField, Range(0.3f, 3f)] private float _windupMul = 1f;
    [Tooltip("회복 구간(타격 프레임 뒤) 길이 배율")]
    [SerializeField, Range(0.3f, 3f)] private float _recoverMul = 1f;
    [Tooltip("손잡이 끝에 줄·사슬(WeaponChain)을 단다 — 채찍·사슬낫·유성추·올가미")]
    [SerializeField] private bool _chain;
    [SerializeField] private int _chainLinks = 12;
    [SerializeField] private Color _chainColor = new Color(0.45f, 0.48f, 0.52f, 1f);
    /// <summary>장착된 아이템의 강화 단계 (EquipBoxCheck.ActivePrefab이 넣어 줌)</summary>
    [System.NonSerialized] public int enhanceLevel;

    public void SetEnhance(int level)
    {
        enhanceLevel = level;
        if (phm != null)
        {
            SetDmgArray();
        }
    }

    void SetDmgArray()
    {
        float mul = EnhanceRules.WeaponMultiplier(enhanceLevel);
        damgeArray[0] = Mathf.RoundToInt(minDmg * mul);
        damgeArray[1] = Mathf.RoundToInt(maxDmg * mul);
        damgeArray[2] = critPer;
        damgeArray[3] = critDmg;
        damgeArray[4] = incDmg;
        damgeArray[5] = ignDef;
        damgeArray[6] = skillDmg;
        damgeArray[7] = finDmg;
        damgeArray[8] = addDmg;
        damgeArray[9] = phm.armorAtp;
    }
    /// <summary>현재 장착 무기 (무기 밑에서 분리돼 생성되는 스킬 오브젝트가 테마색을 찾을 때 사용)</summary>
    public static Weapon Equipped { get; private set; }
    /// <summary>지금 이동 속도에 곱할 배율. 평타 후딜 중이면 무기별 배율, 아니면 1</summary>
    public float AttackMoveSpeedMul => isAttackWait ? 1f : _attackMoveSpeedMul;

    private void Start()
    {
        Equipped = this;
        phm = transform.parent.transform.GetChild(0).GetComponent<PlayerHealthManager>();
        if (pC == null)
        {
            pC = PlayerController.instance; // 장착 경로에 따라 주입되지 않는 경우 대비
        }
        isAttackWait = true;
        isSkillAttackWait = true;
        SetDmgArray();
    }
   public void EquipWeqpon(CharacterRig portrait) // 무기 스프라이트를 소켓에 장착합니다.
    {
        if(RightWeqponSprite!= null)
        {
            Transform socketR = portrait.GetBoneSocket("RightWeapon");
            RightWeqponSprite.transform.parent = socketR;
            if (!isOverHand)
                RightWeqponSprite.transform.localPosition = new Vector3(0, 0, 25f);
            else
                RightWeqponSprite.transform.localPosition = new Vector3(0, 0, 0f);

            RightWeqponSprite.transform.localRotation = Quaternion.identity;
            portrait.AdoptWeapon(RightWeqponSprite.transform, false);
            AttachChain(RightWeqponSprite.transform);
        }
        if (portrait is PixelRig pixel)
        {
            pixel.SetTimingProfile(_windupMul, _recoverMul);
        }
        if (leftWeqponSprite != null)
        {
            Transform socketL = portrait.GetBoneSocket("LeftWeapon");
            leftWeqponSprite.transform.parent = socketL;
            if (!isOverHand)
                leftWeqponSprite.transform.localPosition = new Vector3(0,0,35f);
            else
                leftWeqponSprite.transform.localPosition = new Vector3(0, 0, 5f);
            leftWeqponSprite.transform.localRotation = Quaternion.identity;
            portrait.AdoptWeapon(leftWeqponSprite.transform, true);
        }
    }

    private void AttachChain(Transform grip)
    {
        if (!_chain || grip.GetComponentInChildren<WeaponChain>() != null)
        {
            return;
        }
        var go = new GameObject("PXFX_Chain");
        go.transform.SetParent(grip, false);
        go.AddComponent<LineRenderer>();
        go.AddComponent<WeaponChain>().Setup(_chainLinks, 0.1f, _chainColor, 49);
    }

    public void CheckSkill()
    {
        for(int i = 0; i < 4; i++)
        {
            if (skill[i] != null)
            {
                skill[i].ActiveMainSkill(i);
            }
            else
            {
                // 빈 슬롯 UI 끄기 — 스킬이 하나도 없는 무기(구버전 ToxicPickAxe)는 끌 수단이 없으므로 건너뛴다(W-01)
                Skill any = System.Array.Find(skill, s => s != null);
                if (any != null)
                {
                    any.DisactiveMainSkill(i);
                }
            }
        }
    }

    public void ActiveWeaponSkill(int num)
    {
        if (IsAnySkillHolding())
        {
            return; // 홀드 유지 중에는 다른 스킬(취소 가능 스킬 포함)을 발동하지 않는다 (B-113)
        }
        if (skill[num] != null)
        {
            skill[num].ActiveSkill(num);
        }
    }

    private bool IsAnySkillHolding()
    {
        for (int i = 0; i < skill.Length; i++)
        {
            if (skill[i] != null && skill[i].IsHolding)
            {
                return true;
            }
        }
        return false;
    }

    public bool isSkillCancel = false;

    /// <summary>지금 평타를 낼 수 있는지 (후딜·스킬 대기 끝)</summary>
    public bool CanMeleeNow => isAttackWait == true && (isSkillAttackWait || isSkillCancel);

    [Tooltip("후딜이 끝난 뒤 다음 콤보 입력을 기다리는 시간(초). 이 안에 누르면 다음 단계로 이어진다 (W-02/W-03)")]
    [SerializeField] private float _comboGrace = 0.35f;
    [Tooltip("평타 판정이 열려 있는 시간 = 공격 주기 × 이 비율 (단타 판정만). 0이면 프리팹 holdingTime 그대로 (W-07)")]
    [SerializeField] private float _meleeActiveRatio = 0.6f;

    public void MeleeAttack()
    {
        if (isAttackWait == true && (isSkillAttackWait || isSkillCancel))
        {
            comboSequence.Kill(); // 이어지는 입력이면 리셋 타이머 취소 — 리셋 타이머는 후딜 종료 시점부터 돈다
            nowConboCount = nowConboCount < maxComboCount ? nowConboCount + 1 : 1;
            MeleeAttackActive();
        }
    }
    public void MeleeAttackActive()
    {
        int step = nowConboCount; // 지연 생성 중에 콤보가 리셋돼도 이 단계로 만든다 (W-23)
        float delay = isHasDelayTime && delayTime.Length >= step ? delayTime[step - 1] / StatMath.AttackSpeedMul : 0f;
        if (delay > 0f)
        {
            DOVirtual.DelayedCall(delay, () => CreatAttackPrefab(step), false).SetLink(gameObject);
        }
        else
        {
            CreatAttackPrefab(step);
        }
        if (weaponSound != null && weaponSound.Length >= step)
        {
            string sound = weaponSound[step - 1];
            // 근접 평타는 휘두르는 소리를 칼 궤적이 나오는 타격 포즈 순간에(입력 순간이면 예비 동작 동안 먼저 울렸다, 10-26)
            string animName = attackAnimName.Length >= step ? attackAnimName[step - 1] : null;
            float soundDelay = WeaponFx.Enabled && WeaponFx.IsMeleeSwing(swingVfx) ? delay + WeaponFx.StrikeDelayFor(animName, pC.Rig) / StatMath.AttackSpeedMul : 0f;
            if (soundDelay > 0f)
            {
                DOVirtual.DelayedCall(soundDelay, () => MasterAudio.PlaySound(sound), false).SetLink(gameObject);
            }
            else
            {
                MasterAudio.PlaySound(sound);
            }
        }
        CheckAttackWait();
    }
    public void CreatAttackPrefab()
    {
        CreatAttackPrefab(nowConboCount);
    }
    void CreatAttackPrefab(int step)
    {
        if (step < 1 || attackPrefab.Length < step || attackPivot.Length < step || attckSpeed.Length < step || attackAnimName.Length < step)
        {
            GameLog.Warn(GameLog.Cat.Player, "무기 데이터 부족: " + name + " 콤보 " + step + "단계 (W-01)", this);
            return;
        }
        pC.ActiveAttackAnim(spriteReverse.Length >= step && spriteReverse[step - 1], attackAnimName[step - 1], attckSpeed[step - 1] / StatMath.AttackSpeedMul);
        bool celMelee = WeaponFx.TryMelee(attackAnimName[step - 1], swingVfx, out VfxId meleeFx, out bool fxFlipY) && WeaponFx.IsCelSmear(meleeFx);
        // 셀 스미어 무기는 판정도 칼이 실제로 지나가는 순간에(10-28): 입력 순간에 만들면 준비 동작(0.16~0.19초) 중에 먼저 맞고 히트스톱이 걸렸다
        float hitDelay = celMelee ? WeaponFx.StrikeDelayFor(attackAnimName[step - 1], pC.Rig) / StatMath.AttackSpeedMul : 0f;
        if (hitDelay > 0f)
        {
            DOVirtual.DelayedCall(hitDelay, () => SpawnHitbox(step, true), false).SetLink(gameObject);
        }
        else
        {
            SpawnHitbox(step, celMelee);
        }
        Vector3 fxPos = attackPivot[step - 1].transform.position;
        bool fxLeft = pC.transform.localScale.x < 0f;
        // 근접 평타는 동작별 새 도트 궤적 + 무기 테마 색(GameFeelPlan 10.7), 그 외(지팡이 등)는 기존 이펙트
        if (WeaponFx.TryMelee(attackAnimName[step - 1], swingVfx, out meleeFx, out fxFlipY))
        {
            // 궤적은 타격 포즈 순간에(예비 포즈 동안은 칼만 보인다) — 리그 공격 클립의 타격 프레임 0.07초, 공속 배율 적용
            PixelRampId ramp = PixelRamp.FromSwing(swingVfx);
            Transform pivot = attackPivot[step - 1].transform;
            // 콤보 마지막 타(3타 이상 무기의 끝)는 궤적을 크게 — 마무리감(10-26)
            float scale = swingVfxScale * (step >= ComboFinisherMinSteps && step == attackAnimName.Length ? ComboFinisherScale : 1f);
            DOVirtual.DelayedCall(WeaponFx.StrikeDelayFor(attackAnimName[step - 1], pC.Rig) / StatMath.AttackSpeedMul, () =>
            {
                Vector3 at = pivot != null ? pivot.position : fxPos;
                // 찌르기 궤적은 칼날 높이(손 소켓)에 — 판정 위치는 예전 캐릭터 기준이라 칼날보다 아래로 지나갔다(10-26)
                Transform hand = meleeFx == VfxId.WThrust && pC.Rig != null ? pC.Rig.GetBoneSocket("RightWeapon") : null;
                if (hand != null)
                {
                    at.y = hand.position.y;
                }
                // 스미어는 셀과 같은 좌표로 그려 셀 원점(발 가운데)에 크기 1로 — 몸·칼과 정확히 겹친다(10-28)
                Transform celRoot = WeaponFx.IsCelSmear(meleeFx) && pC.Rig != null ? pC.Rig.CelRoot : null;
                if (celRoot != null)
                {
                    // 흰 램프는 거의 단색이라 납작해 보여 기본 칼 스미어는 푸른 강철 램프로(10-28 레퍼런스 비교)
                    PixelRampId smearRamp = ramp == PixelRampId.White ? PixelRampId.Steel : ramp;
                    Vfx.PlayThemed(meleeFx, celRoot.position, fxLeft, Mathf.Abs(celRoot.lossyScale.y), smearRamp, false, false, 1f, true);
                    return;
                }
                Vfx.PlayThemed(meleeFx, at, fxLeft, scale, ramp, fxFlipY);
            }).SetLink(gameObject);
        }
        else
        {
            Vfx.Play(swingVfx, fxPos, fxLeft, swingVfxScale);
        }
    }

    /// <summary>평타 판정 프리팹 생성. hideArt = 새 도트 궤적을 쓰는 근접 평타라 판정 프리팹의 예전 베기 그림을 끈다(두 겹으로 보였다)</summary>
    void SpawnHitbox(int step, bool hideArt)
    {
        if (attackPrefab.Length < step || attackPivot.Length < step || attackPivot[step - 1] == null)
        {
            return;
        }
        GameObject damageObject = Instantiate(attackPrefab[step - 1], attackPivot[step - 1].transform.position, attackPivot[step - 1].transform.rotation, this.transform);
        DamageObject dmOb = damageObject.GetComponent<DamageObject>();
        dmOb.SetDamge(damgeArray);
        if (_meleeActiveRatio > 0f && dmOb.hitCount <= 1)
        {
            dmOb.activeEnd = attckSpeed[step - 1] * _meleeActiveRatio;
        }
        dmOb.isSkill = false; // 평타는 스킬뎀% 를 받지 않는다(공유 프리팹 플래그 무시, W-04)
        if (hideArt || WeaponFx.TryMelee(attackAnimName[step - 1], swingVfx, out _, out _))
        {
            damageObject.GetComponentsInChildren(true, hideRenderers);
            foreach (SpriteRenderer r in hideRenderers)
            {
                r.enabled = false;
            }
        }
    }
    static readonly List<SpriteRenderer> hideRenderers = new List<SpriteRenderer>();
    System.IDisposable moveLockToken, ladderLockToken;
    Sequence attackWaitSequence;

    /// <summary>평타 후딜: 이동정지(옵션)·사다리 잠금을 자기 토큰으로 잡고, 대기 시간이 끝나면 자기 것만 푼다 (B-114 C4, C7)</summary>
    private void CheckAttackWait()
    {
        ReleaseAttackLocks();
        attackWaitSequence.Kill();
        if (isWeaponStopMove == true)
        {
            moveLockToken = PlayerStatus.Acquire(StatusFlag.MoveLock, "Weapon:" + name);
        }
        ladderLockToken = PlayerStatus.Acquire(StatusFlag.LadderLock, "Weapon:" + name);
        isAttackWait = false;
        int step = nowConboCount;
        float baseTime = attckSpeed.Length >= step && step > 0 ? attckSpeed[step - 1] : 0.3f;
        if (isHasDelayTime && delayTime.Length >= step && step > 0)
        {
            baseTime += delayTime[step - 1];
        }
        float waitTime = baseTime / StatMath.AttackSpeedMul;
        attackWaitSequence = DOTween.Sequence()
        .AppendInterval(waitTime) // 사전에 지정한 공격 주기만큼 대기.
        .AppendCallback(() =>
        {
            ReleaseAttackLocks();
            isAttackWait = true;
            // 콤보 유예: 후딜이 끝난 뒤 _comboGrace 안에 다음 입력이 없으면 1단계로
            comboSequence.Kill();
            comboSequence = DOTween.Sequence().AppendInterval(_comboGrace).AppendCallback(() => nowConboCount = 0);
        });
    }

    void ReleaseAttackLocks()
    {
        moveLockToken?.Dispose();
        moveLockToken = null;
        ladderLockToken?.Dispose();
        ladderLockToken = null;
    }

    private void OnDisable()
    {
        if (Equipped == this)
        {
            Equipped = null;
        }
        // 무기 해제·파괴 시 잠금이 남지 않도록 (C9)
        attackWaitSequence.Kill();
        ReleaseAttackLocks();
        isAttackWait = true;
    }
}
