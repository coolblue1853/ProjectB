using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    // 방어도
    // 체력증가
    public int armor;
    public int hp;
    public int critical;
    public int dropRate;
    [System.Serializable]
    public struct SkillCoolTime
    {
        public string skillName;
        public int coolDownCount;
    
    }

    [SerializeField]
    public SkillCoolTime[] coolDownSkill;

}
