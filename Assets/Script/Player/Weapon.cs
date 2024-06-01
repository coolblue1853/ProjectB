using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using AnyPortrait;
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
    public int addDmg = 0; // 최종뎀%  
    PlayerHealthManager phm;



    public int[] damgeArray = new int[10];
    public float[] attckSpeed;   // 공격 주기, 짧을수록 더 빠르게 공격 가능.
    public bool isAttackWait = true;
    public bool isSkillAttackWait = true;
    public bool isWeaponStopMove = false;
    public float time;
    // public GameObject attackPivot;

    public bool isOverHand =false;

    public GameObject[] attackPrefab;
    public GameObject[] attackPivot;
    public string[] attackAnimName;

    public Skill skillA;
    public Skill skillB;
    public Skill skillC;
    public Skill skillD;

    public GameObject leftWeqponSprite;
    public GameObject RightWeqponSprite;

    void SetDmgArray()
    {
        damgeArray[0] = minDmg;
        damgeArray[1] = maxDmg;
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
           isAttackWait = true;
        isSkillAttackWait = true;
        SetDmgArray();
    }
   public void EquipWeqpon(apPortrait portrait)
    {
        if(RightWeqponSprite!= null)
        {
            Transform socketR = portrait.GetBoneSocket("RightWeapon");
            // 검 (Weapon_Sword)을 오른손 본의 소켓의 자식으로 등록합니다.
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
            // 검 (Weapon_Sword)을 오른손 본의 소켓의 자식으로 등록합니다.
            leftWeqponSprite.transform.parent = socketL;
            if (!isOverHand)
                leftWeqponSprite.transform.localPosition = new Vector3(0,0,35f);
            else
            {
                leftWeqponSprite.transform.localPosition = new Vector3(0, 0, 5f);
            }
            leftWeqponSprite.transform.localRotation = Quaternion.identity;
        }

    }

    public void CheckSkill()
    {
        if (skillA != null)
            skillA.ActiveMainSkill();
        if (skillB != null)
            skillB.ActiveMainSkill();
        if (skillC != null)
            skillC.ActiveSideSkill();
        if (skillD != null)
            skillD.ActiveSideSkill();
    }

    public void ActiveASkill()
    {
        if(skillA != null)
        {
            skillA.ActiveLeft();
        }

    }
    public void ActiveBSkill()
    {
        if (skillB != null)
        {
            skillB.ActiveRight();
        }
    }
    public void ActiveCSkill()
    {
        if (skillC != null)
        {
            skillC.ActiveSideLeft();
        }

    }
    public bool isSkillCancel = false;
    public void ActiveSideRightSkill()
    {
        if (skillB != null)
        {
            skillB.ActiveSideRight();
        }
    }

    public void MeleeAttack()
    {
        time = 0;
        if (nowConboCount == 0 && (isAttackWait == true && (isSkillAttackWait || isSkillCancel)))//|| isSkillCancel == true)
        {
            nowConboCount++;
            //프리팹 소환
            GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot[nowConboCount - 1].transform.position, attackPivot[nowConboCount - 1].transform.rotation,this.transform);
            pC.ActiveAttackAnim(attackAnimName[nowConboCount - 1], attckSpeed[nowConboCount - 1]  / (1 + (DatabaseManager.attackSpeedBuff / 100)));
            DamageObject dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.SetDamge(damgeArray);
            CheckAttackWait();
        }
        else
        {
            if (nowConboCount < maxComboCount && time <= maxComboTime && (isAttackWait == true && (isSkillAttackWait || isSkillCancel)))//|| isSkillCancel == true
            {
                nowConboCount++;
                GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot[nowConboCount - 1].transform.position, attackPivot[nowConboCount - 1].transform.rotation, this.transform);
                pC.ActiveAttackAnim(attackAnimName[nowConboCount - 1], attckSpeed[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
                DamageObject dmOb = damageObject.GetComponent<DamageObject>();
                dmOb.SetDamge(damgeArray);
                CheckAttackWait();
            }
            else if (nowConboCount >= maxComboCount && time <= maxComboTime && (isAttackWait == true && (isSkillAttackWait || isSkillCancel))) // 콤보수 초기화 및 다시 카운트 1로 내려옴. || isSkillCancel == true
            {
                nowConboCount = 1;
                GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot[nowConboCount - 1].transform.position, attackPivot[nowConboCount - 1].transform.rotation, this.transform);
                pC.ActiveAttackAnim(attackAnimName[nowConboCount - 1], attckSpeed[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
                DamageObject dmOb = damageObject.GetComponent<DamageObject>();
                dmOb.SetDamge(damgeArray);
                CheckAttackWait();
            }
        }

    }
    private void CheckAttackWait()
    {
        if(isWeaponStopMove == true)
        {
            DatabaseManager.weaponStopMove = true;
        }
        DatabaseManager.checkAttackLadder = true;
        isAttackWait = false;
        Sequence sequence = DOTween.Sequence()
        .AppendInterval(attckSpeed[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100))) // 사전에 지정한 공격 주기만큼 대기.
        .AppendCallback(() => DatabaseManager.weaponStopMove = false)
        .AppendCallback(() => DatabaseManager.checkAttackLadder = false)
        .AppendCallback(() => isAttackWait = true);
    }

    private void Update()
    {
        if(nowConboCount!= 0 && time < maxComboTime)
        {
            time += Time.deltaTime;
        }
        else if(time > maxComboTime && time != 0)
        {
            nowConboCount = 0;
               time = 0;
        }
    }




}
