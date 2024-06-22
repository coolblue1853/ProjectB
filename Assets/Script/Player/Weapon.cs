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
    public float maxComboTime = 0; // �޺� �ִ� �����ð�

    public int minDmg=0;        // �ּҴ�
    public int maxDmg=0;       // �ִ��
    public int critPer = 0;   //  ġ�� Ȯ��%
    public int critDmg = 0;   //  ġ������%
    public int incDmg = 0;   // ����������%
    public int ignDef = 0; // ���� ����%
    public int skillDmg = 0; // ��ų ���ݷ�%
    public int finDmg = 0; // ������%  
    public int addDmg = 0; // �ߵ�%  
    PlayerHealthManager phm;

   

    public int[] damgeArray = new int[10];
    public float[] attckSpeed;   // ���� �ֱ�, ª������ �� ������ ���� ����.
    public bool isAttackWait = true;
    public bool isSkillAttackWait = true;
    public bool isWeaponStopMove = false;
    public float time;
    // public GameObject attackPivot;

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
    public Skill[] skill;


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
            // �� (Weapon_Sword)�� ������ ���� ������ �ڽ����� ����մϴ�.
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
            // �� (Weapon_Sword)�� ������ ���� ������ �ڽ����� ����մϴ�.
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
        if (skill[0] != null)
            skill[0].ActiveMainSkill();
        if (skill[1] != null)
            skill[1].ActiveMainSkill();
        if (skill[2] != null)
            skill[2].ActiveSideSkill();
        if (skill[3] != null)
            skill[3].ActiveSideSkill();
    }

    public void ActiveASkill()
    {
        if(skill[0] != null)
        {
            skill[0].ActiveLeft();
        }

    }
    public void ActiveBSkill()
    {
        if (skill[1] != null)
        {
            skill[1].ActiveRight();
        }
    }
    public void ActiveCSkill()
    {
        if (skill[2] != null)
        {
            skill[2].ActiveSideLeft();
        }

    }
    public void ActiveDSkill()
    {
        if (skill[3] != null)
        {
            skill[3].ActiveSideRight();
        }

    }
    public bool isSkillCancel = false;

    public void MeleeAttack()
    {
        time = 0;
        if (nowConboCount == 0 && (isAttackWait == true && (isSkillAttackWait || isSkillCancel)))//|| isSkillCancel == true)
        {
            nowConboCount++;
 
            //������ ��ȯ
            if (delayTime.Length > 0)
            {
            //    pC.ActiveAttackAnim(spriteReverse[nowConboCount - 1],delayAnimName[nowConboCount - 1], delayTime[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)), true);
                Invoke("CreatAttackPrefab", delayTime[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)));

            }
            else
            {
               // pC.ActiveAttackAnim(spriteReverse[nowConboCount - 1],attackAnimName[nowConboCount - 1], attckSpeed[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
                CreatAttackPrefab();

            }
            CheckAttackWait();
        }
        else
        {
            if (nowConboCount < maxComboCount && time <= maxComboTime && (isAttackWait == true && (isSkillAttackWait || isSkillCancel)))//|| isSkillCancel == true
            {
                nowConboCount++;

                if (delayTime.Length > 0)
                {
                //   pC.ActiveAttackAnim(spriteReverse[nowConboCount - 1], delayAnimName[nowConboCount - 1], delayTime[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)), true);
                    Invoke("CreatAttackPrefab", delayTime[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)));

                }
                else
                {
                   // pC.ActiveAttackAnim(spriteReverse[nowConboCount - 1], attackAnimName[nowConboCount - 1], attckSpeed[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
                    CreatAttackPrefab();

                }
                CheckAttackWait();
            }
            else if (nowConboCount >= maxComboCount && time <= maxComboTime && (isAttackWait == true && (isSkillAttackWait || isSkillCancel))) // �޺��� �ʱ�ȭ �� �ٽ� ī��Ʈ 1�� ������. || isSkillCancel == true
            {
                nowConboCount = 1;

                if (delayTime.Length > 0)
                {
                  //  pC.ActiveAttackAnim(spriteReverse[nowConboCount - 1], delayAnimName[nowConboCount - 1], delayTime[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)), true);
                    Invoke("CreatAttackPrefab", delayTime[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)));

                }
                else
                {
                 //   pC.ActiveAttackAnim(spriteReverse[nowConboCount - 1], attackAnimName[nowConboCount - 1], attckSpeed[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
                    CreatAttackPrefab();
  
                }
                CheckAttackWait();

            }
        }

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
        if (isWeaponStopMove == true)
        {
            DatabaseManager.weaponStopMove = true;
        }
        DatabaseManager.checkAttackLadder = true;
        isAttackWait = false;

        if (delayTime.Length > 0)
        {
            Sequence sequence = DOTween.Sequence()
.AppendInterval((attckSpeed[nowConboCount - 1] + delayTime[nowConboCount - 1]) /(1 + (DatabaseManager.attackSpeedBuff / 100))) // ������ ������ ���� �ֱ⸸ŭ ���.
.AppendCallback(() => DatabaseManager.weaponStopMove = false)
.AppendCallback(() => DatabaseManager.checkAttackLadder = false)
.AppendCallback(() => isAttackWait = true);
        }
        else
        {
            Sequence sequence = DOTween.Sequence()
.AppendInterval(attckSpeed[nowConboCount - 1] / (1 + (DatabaseManager.attackSpeedBuff / 100))) // ������ ������ ���� �ֱ⸸ŭ ���.
.AppendCallback(() => DatabaseManager.weaponStopMove = false)
.AppendCallback(() => DatabaseManager.checkAttackLadder = false)
.AppendCallback(() => isAttackWait = true);
        }


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
