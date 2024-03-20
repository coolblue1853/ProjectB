using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class Skill : MonoBehaviour
{
    public GameObject skillPivot;
    public bool isLeft;
    public bool isRight;
    public bool isButtonDownSkill;
    public float SkillCoolTime;
    public Sprite skillImage;

    public bool isWeaponStopMove;
    public int useStemina;
    public float attckSpeed;
    Weapon weapon;
    int[] damgeArray = new int[10];
    public Rigidbody2D rb;

    private void Awake()
    {
        skillCooldown = GameObject.FindWithTag("Cooldown").GetComponent<SkillCooldown>();
        weapon = transform.parent.gameObject.GetComponent<Weapon>();
        damgeArray = weapon.damgeArray;
    }
    public GameObject[] skillprefab;
    public SkillCooldown skillCooldown;
    // Start is called before the first frame update

    void Start()
    {
        rb = transform.parent.parent.GetComponent<Rigidbody2D>();

    }
    private void CheckAttackWait()
    {
        if (isWeaponStopMove == true)
        {
            DatabaseManager.weaponStopMove = true;
            rb.velocity = Vector2.zero;
        }
        weapon.isAttackWait = false;
        Sequence sequence = DOTween.Sequence()
        .AppendInterval(attckSpeed) // 사전에 지정한 공격 주기만큼 대기.
        .AppendCallback(() => DatabaseManager.weaponStopMove = false)
        .AppendCallback(() => weapon.isAttackWait = true);
    }
    public void ActiveMainSkill()
    {
        if (isLeft)
        {

            skillCooldown.cooldownTimeA = SkillCoolTime;
            skillCooldown.cooldownImageA.sprite = skillImage;
            skillCooldown.UseSkill();
        }
        else if (isRight)
        {

            skillCooldown.cooldownTimeB = SkillCoolTime;
            skillCooldown.cooldownImageB.sprite = skillImage;
            skillCooldown.UseSkillB();
        }
    }
    public void ActiveSideSkill()
    {
        if (isLeft)
        {
            skillCooldown.cooldownTimeC = SkillCoolTime;
            skillCooldown.cooldownImageC.sprite = skillImage;
           skillCooldown.UseSkillC();
        }
        else if (isRight)
        {
            skillCooldown.cooldownTimeD = SkillCoolTime;
            skillCooldown.cooldownImageD.sprite = skillImage;
            skillCooldown.UseSkillD();
        }
    }


    public void ActiveLeft()
    {

        if (isButtonDownSkill && skillCooldown.isCooldownA == false && PlayerHealthManager.Instance.nowStemina > useStemina &&weapon.isAttackWait)
        {
            PlayerHealthManager.Instance.SteminaDown(useStemina);
            GameObject damageObject = Instantiate(skillprefab[0], skillPivot.transform.position, skillPivot.transform.rotation, this.transform);
            DamageObject dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.SetDamge(damgeArray);
            skillCooldown.UseSkill();
            CheckAttackWait();
        }

    }
    public void ActiveRight()
    {

        if (isButtonDownSkill && skillCooldown.isCooldownB == false && PlayerHealthManager.Instance.nowStemina > useStemina && weapon.isAttackWait)
        {
            PlayerHealthManager.Instance.SteminaDown(useStemina);
            GameObject damageObject = Instantiate(skillprefab[0], skillPivot.transform.position, skillPivot.transform.rotation, this.transform);
            DamageObject dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.SetDamge(damgeArray);
            skillCooldown.UseSkillB();
            CheckAttackWait();
        }
    }
    public void ActiveSideLeft()
    {

        if (isButtonDownSkill && skillCooldown.isCooldownC == false && PlayerHealthManager.Instance.nowStemina > useStemina && weapon.isAttackWait)
        {
            PlayerHealthManager.Instance.SteminaDown(useStemina);
            GameObject damageObject = Instantiate(skillprefab[0], skillPivot.transform.position, skillPivot.transform.rotation, this.transform);
            DamageObject dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.SetDamge(damgeArray);
            skillCooldown.UseSkillC();
            CheckAttackWait();
        }

    }
    public void ActiveSideRight()
    {

        if (isButtonDownSkill && skillCooldown.isCooldownD == false && PlayerHealthManager.Instance.nowStemina > useStemina && weapon.isAttackWait)
        {
            PlayerHealthManager.Instance.SteminaDown(useStemina);
            GameObject damageObject = Instantiate(skillprefab[0], skillPivot.transform.position, skillPivot.transform.rotation, this.transform);
            DamageObject dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.SetDamge(damgeArray);
            skillCooldown.UseSkillD();
            CheckAttackWait();
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
