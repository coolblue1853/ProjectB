using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    // 방어도
    // 체력증가
    public int armor;
    public int hp;
    public int basicDmg; // 최소 공격력 증가
    public int critical;
    public int criticalDmg;
    public int dropRate;
    public int attSpeed; // 기본 공격 속도
    public int moveSpeed;

    public bool isBleeding;
    [ConditionalHide("isBleeding")]
    public int bleedingPerCent;
    [ConditionalHide("isBleeding")]
    public int bleedingDamage;
    [ConditionalHide("isBleeding")]
    public float bleedingDamageInterval;
    [ConditionalHide("isBleeding")]
    public int bleedingDamageCount;
    
    

    [System.Serializable]
    public struct SkillCoolTime
    {
        public string skillName;
        public int coolDownCount;
    
    }

    [SerializeField]
    public SkillCoolTime[] coolDownSkill;

}
