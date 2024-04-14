using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
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
    public int addDmg = 0; // ������%  
    PlayerHealthManager phm;



    public int[] damgeArray = new int[10];
    public float attckSpeed = 0;   // ���� �ֱ�, ª������ �� ������ ���� ����.
    public bool isAttackWait = true;
    public bool isWeaponStopMove = false;
    public float time;
    public GameObject attackPivot;

    public GameObject[] attackPrefab;
    public string[] attackAnimName;
    public Skill skillLeft;
    public Skill skillRight;
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
        SetDmgArray();
    }
    public void CheckSkill()
    {
        skillLeft.ActiveMainSkill();
        skillRight.ActiveMainSkill();
    }
    public void ChecSidekSkill()
    {
        skillLeft.ActiveSideSkill();
        skillRight.ActiveSideSkill();
    }

    public void ActiveLeftSkill()
    {
        if(skillLeft != null)
        {
            skillLeft.ActiveLeft();
        }

    }
    public void ActiveRightSkill()
    {
        if (skillRight != null)
        {
            skillRight.ActiveRight();
        }
    }
    public void ActiveSideLeftSkill()
    {
        if (skillLeft != null)
        {
            skillLeft.ActiveSideLeft();
        }

    }
    public bool isSkillCancel = false;
    public void ActiveSideRightSkill()
    {
        if (skillRight != null)
        {
            skillRight.ActiveSideRight();
        }
    }

    public void MeleeAttack()
    {
        time = 0;
        if (nowConboCount == 0 && (isAttackWait == true))//|| isSkillCancel == true)
        {
            nowConboCount++;
            //������ ��ȯ
            GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot.transform.position, attackPivot.transform.rotation,this.transform);
            pC.ActiveAttackAnim(attackAnimName[nowConboCount - 1], attckSpeed);
            DamageObject dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.SetDamge(damgeArray);
            CheckAttackWait();
        }
        else
        {
            if (nowConboCount < maxComboCount && time <= maxComboTime && (isAttackWait == true ))//|| isSkillCancel == true
            {
                nowConboCount++;
                GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot.transform.position, attackPivot.transform.rotation, this.transform);
                pC.ActiveAttackAnim(attackAnimName[nowConboCount - 1], attckSpeed);
                DamageObject dmOb = damageObject.GetComponent<DamageObject>();
                dmOb.SetDamge(damgeArray);
                CheckAttackWait();
            }
            else if (nowConboCount >= maxComboCount && time <= maxComboTime && (isAttackWait == true)) // �޺��� �ʱ�ȭ �� �ٽ� ī��Ʈ 1�� ������. || isSkillCancel == true
            {
                nowConboCount = 1;
                GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot.transform.position, attackPivot.transform.rotation, this.transform);
                pC.ActiveAttackAnim(attackAnimName[nowConboCount - 1], attckSpeed);
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
        isAttackWait = false;
        Sequence sequence = DOTween.Sequence()
        .AppendInterval(attckSpeed) // ������ ������ ���� �ֱ⸸ŭ ���.
        .AppendCallback(() => DatabaseManager.weaponStopMove = false)
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
