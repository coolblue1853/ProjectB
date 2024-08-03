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
    public int incDmg; // ������ ����
    public int dropRate;
    public int attSpeed; // �⺻ ���� �ӵ�
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
    public int bleedingDmgPer; // ������ ������ ����.

    public int poisonDmg;// �ߵ� ������ ��ġ ����

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
