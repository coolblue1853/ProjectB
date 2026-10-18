using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    // 방어도
    // 체력증가
    public int armor;

    /// <summary>장착 아이템의 강화 단계와, 적용 당시 실제로 더한 값(해제 시 같은 값을 뺀다)</summary>
    [System.NonSerialized] public int enhanceLevel;
    [System.NonSerialized] public int appliedHp, appliedBasicDmg, appliedArmor;

    /// <summary>방어구 방어력 → 플레이어 방어(playerDef) 반영 비율. d²/(d+Def) 공식에서 전부 넣으면 Leaf 세트만으로 피해 75% 감소라 절반만 (B-212, 튜닝 대상)</summary>
    public const float ArmorToDef = 0.5f;

    public int Enhanced(int value)
    {
        return Mathf.RoundToInt(value * EnhanceRules.GearMultiplier(enhanceLevel));
    }
    public int hp;
    public int basicDmg; // 최소 공격력 증가
    public int critical;
    public int criticalDmg;
    public int incDmg; // 데미지 증가
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
    public int bleedingDmgPer; // 출혈시 데미지 증가.

    public int poisonDmg;// 중독 데미지 수치 증가

    public int addIncomingDmg;

    [System.Serializable]
    public struct SkillCoolTime
    {
        public string skillName;
        public int coolDownCount;
    }

    [SerializeField]
    public SkillCoolTime[] coolDownSkill;

}
