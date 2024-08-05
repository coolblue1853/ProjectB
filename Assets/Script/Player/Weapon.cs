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
            Invoke("CreatAttackPrefab", delayTime[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
        else CreatAttackPrefab();
        MasterAudio.PlaySound(weaponSound[nowConboCount - 1]);
        CheckAttackWait();
    }
    public void CreatAttackPrefab()
    {
        pC.ActiveAttackAnim(spriteReverse[nowConboCount - 1], attackAnimName[nowConboCount - 1], attckSpeed[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
        GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot[nowConboCount - 1].transform.position, attackPivot[nowConboCount - 1].transform.rotation, this.transform);
        DamageObject dmOb = damageObject.GetComponent<DamageObject>();
        dmOb.SetDamge(damgeArray);
    }
    private void CheckAttackWait()
    {
        if (isWeaponStopMove == true) DatabaseManager.weaponStopMove = true;
        DatabaseManager.checkAttackLadder = true;
        isAttackWait = false;
        float waitTime = 0;
        if (delayTime.Length > 0)
            waitTime = (attckSpeed[nowConboCount - 1] + delayTime[nowConboCount - 1]) / (1 + (DatabaseManager.attackSpeedBuff / 100));
        else
            waitTime = (attckSpeed[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
        Sequence sequence = DOTween.Sequence()
        .AppendInterval(waitTime) // 사전에 지정한 공격 주기만큼 대기.
        .AppendCallback(() => DatabaseManager.weaponStopMove = false)
        .AppendCallback(() => DatabaseManager.checkAttackLadder = false)
        .AppendCallback(() => isAttackWait = true);
    }
}
