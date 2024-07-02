using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    // ��
    // ü������
    public int armor;
    public int hp;
    public int basicDmg; // �ּ� ���ݷ� ����
    public int critical;
    public int criticalDmg;
    public int dropRate;
    public int attSpeed; // �⺻ ���� �ӵ�
    public int moveSpeed;
    [System.Serializable]
    public struct SkillCoolTime
    {
        public string skillName;
        public int coolDownCount;
    
    }

    [SerializeField]
    public SkillCoolTime[] coolDownSkill;

}
