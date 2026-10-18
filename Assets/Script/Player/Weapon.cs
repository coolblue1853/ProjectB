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
    private void Start()
    {
        phm = transform.parent.transform.GetChild(0).GetComponent<PlayerHealthManager>();
        if (pC == null)
        {
            pC = PlayerController.instance; // 장착 경로에 따라 주입되지 않는 경우 대비
        }
        isAttackWait = true;
        isSkillAttackWait = true;
        SetDmgArray();
    }
   public void EquipWeqpon(apPortrait portrait) // 무기 스프라이트를 소켓에 장착합니다.
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
        }
    }

    public void CheckSkill()
    {
        for(int i = 0; i < 4; i++)
        {
            if (skill[i] != null) skill[i].ActiveMainSkill(i);
            else skill[0].DisactiveMainSkill(i);
        }
    }

    public void ActiveWeaponSkill(int num)
    {
        if(skill[num] != null) skill[num].ActiveSkill(num);
    }

    public bool isSkillCancel = false;

    /// <summary>지금 평타를 낼 수 있는지 (후딜·스킬 대기 끝)</summary>
    public bool CanMeleeNow => isAttackWait == true && (isSkillAttackWait || isSkillCancel);

    public void MeleeAttack()
    {
        comboSequence.Kill();
        comboSequence = DOTween.Sequence()
        .AppendInterval(maxComboTime)
        .AppendCallback(() => nowConboCount = 0);
        if (isAttackWait == true && (isSkillAttackWait || isSkillCancel))
        {
            if (nowConboCount < maxComboCount)
            {
                nowConboCount++;
                MeleeAttackActive();
            }
            else
            {
                nowConboCount = 1;
                MeleeAttackActive();
            }
        }
    }
    public void MeleeAttackActive()
    {
        if (delayTime.Length > 0)
            Invoke("CreatAttackPrefab", delayTime[nowConboCount - 1] / StatMath.AttackSpeedMul);
        else CreatAttackPrefab();
        MasterAudio.PlaySound(weaponSound[nowConboCount - 1]);
        CheckAttackWait();
    }
    public void CreatAttackPrefab()
    {
        pC.ActiveAttackAnim(spriteReverse[nowConboCount - 1], attackAnimName[nowConboCount - 1], attckSpeed[nowConboCount - 1] / StatMath.AttackSpeedMul);
        GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot[nowConboCount - 1].transform.position, attackPivot[nowConboCount - 1].transform.rotation, this.transform);
        DamageObject dmOb = damageObject.GetComponent<DamageObject>();
        dmOb.SetDamge(damgeArray);
        Vfx.Play(swingVfx, attackPivot[nowConboCount - 1].transform.position, pC.transform.localScale.x < 0f, swingVfxScale);
    }
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
        float waitTime = 0;
        if (delayTime.Length > 0)
            waitTime = (attckSpeed[nowConboCount - 1] + delayTime[nowConboCount - 1]) / StatMath.AttackSpeedMul;
        else
            waitTime = (attckSpeed[nowConboCount - 1] / StatMath.AttackSpeedMul);
        attackWaitSequence = DOTween.Sequence()
        .AppendInterval(waitTime) // 사전에 지정한 공격 주기만큼 대기.
        .AppendCallback(() => { ReleaseAttackLocks(); isAttackWait = true; });
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
        // 무기 해제·파괴 시 잠금이 남지 않도록 (C9)
        attackWaitSequence.Kill();
        ReleaseAttackLocks();
        isAttackWait = true;
    }
}
