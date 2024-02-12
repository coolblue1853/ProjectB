using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Weapon : MonoBehaviour
{
    public int maxComboCount = 0 ;
    public int nowConboCount = 0;
    public float maxComboTime = 0; // 콤보 최대 유지시간
    public float attckSpeed = 0;   // 공격 주기, 짧을수록 더 빠르게 공격 가능.

    public bool isAttackWait = true;
    public bool isWeaponStopMove = false;
    public float time;
    public GameObject attackPivot;

    public GameObject[] attackPrefab;
    public Skill skillLeft;
    public Skill skillRight;
    private void Start()
    {
        isAttackWait = true;
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
        if (nowConboCount == 0 && (isAttackWait == true || isSkillCancel == true))
        {
            nowConboCount++;
            //프리팹 소환
            GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot.transform.position, attackPivot.transform.rotation,this.transform);
            CheckAttackWait();
        }
        else
        {
            if (nowConboCount < maxComboCount && time <= maxComboTime && isAttackWait == true)
            {
                nowConboCount++;
                GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot.transform.position, attackPivot.transform.rotation, this.transform);
                CheckAttackWait();
            }
            else if (nowConboCount >= maxComboCount && time <= maxComboTime && isAttackWait == true) // 콤보수 초기화 및 다시 카운트 1로 내려옴.
            {
                nowConboCount = 1;
                GameObject damageObject = Instantiate(attackPrefab[nowConboCount - 1], attackPivot.transform.position, attackPivot.transform.rotation, this.transform);
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
        .AppendInterval(attckSpeed) // 사전에 지정한 공격 주기만큼 대기.
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
