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
        if(skill[num] != null) skill[num].ActiveSkill(num);
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
            float soundDelay = WeaponFx.Enabled && WeaponFx.IsMeleeSwing(swingVfx) ? delay + WeaponFx.StrikeDelay / StatMath.AttackSpeedMul : 0f;
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
        GameObject damageObject = Instantiate(attackPrefab[step - 1], attackPivot[step - 1].transform.position, attackPivot[step - 1].transform.rotation, this.transform);
        DamageObject dmOb = damageObject.GetComponent<DamageObject>();
        dmOb.SetDamge(damgeArray);
        if (_meleeActiveRatio > 0f && dmOb.hitCount <= 1)
        {
            dmOb.activeEnd = attckSpeed[step - 1] * _meleeActiveRatio;
        }
        dmOb.isSkill = false; // 평타는 스킬뎀% 를 받지 않는다(공유 프리팹 플래그 무시, W-04)
        Vector3 fxPos = attackPivot[step - 1].transform.position;
        bool fxLeft = pC.transform.localScale.x < 0f;
        // 근접 평타는 동작별 새 도트 궤적 + 무기 테마 색(GameFeelPlan 10.7), 그 외(지팡이 등)는 기존 이펙트
        if (WeaponFx.TryMelee(attackAnimName[step - 1], swingVfx, out VfxId meleeFx, out bool fxFlipY))
        {
            // 궤적은 타격 포즈 순간에(예비 포즈 동안은 칼만 보인다) — 리그 공격 클립의 타격 프레임 0.07초, 공속 배율 적용
            PixelRampId ramp = PixelRamp.FromSwing(swingVfx);
            Transform pivot = attackPivot[step - 1].transform;
            // 콤보 마지막 타(3타 이상 무기의 끝)는 궤적을 크게 — 마무리감(10-26)
            float scale = swingVfxScale * (step >= ComboFinisherMinSteps && step == attackAnimName.Length ? ComboFinisherScale : 1f);
            DOVirtual.DelayedCall(WeaponFx.StrikeDelay / StatMath.AttackSpeedMul, () =>
            {
                Vector3 at = pivot != null ? pivot.position : fxPos;
                // 찌르기 궤적은 칼날 높이(손 소켓)에 — 판정 위치는 예전 캐릭터 기준이라 칼날보다 아래로 지나갔다(10-26)
                Transform hand = meleeFx == VfxId.WThrust && pC.Rig != null ? pC.Rig.GetBoneSocket("RightWeapon") : null;
                if (hand != null)
                {
                    at.y = hand.position.y;
                }
                Vfx.PlayThemed(meleeFx, at, fxLeft, scale, ramp, fxFlipY);
            }).SetLink(gameObject);
            // 판정 프리팹에 붙은 예전 베기 그림은 끈다(새 궤적과 두 겹으로 보였다). 판정·충돌은 그대로
            damageObject.GetComponentsInChildren(true, hideRenderers);
            foreach (SpriteRenderer r in hideRenderers)
            {
                r.enabled = false;
            }
        }
        else
        {
            Vfx.Play(swingVfx, fxPos, fxLeft, swingVfxScale);
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
