using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
public class Skill : MonoBehaviour
{
    KeyAction action;
    InputAction attackAction;
    InputAction skillAAction;
    InputAction skillSAction;
    InputAction skillDAction;
    InputAction skillFAction;
    private void OnEnable()
    {
        skillAAction.Enable();
        skillAAction.canceled += OnSkillAReleased;
        skillSAction.Enable();
        skillSAction.canceled += OnSkillSReleased;
        skillDAction.Enable();
        skillDAction.canceled += OnSkillDReleased;
        skillFAction.Enable();
        skillFAction.canceled += OnSkillFReleased;
    }
    private void OnDisable()
    {
        skillAAction.Disable();
        skillAAction.canceled -= OnSkillAReleased;
        skillSAction.Disable();
        skillSAction.canceled -= OnSkillSReleased;
        skillDAction.Disable();
        skillDAction.canceled -= OnSkillDReleased;
        skillFAction.Disable();
        skillFAction.canceled -= OnSkillFReleased;
    }

    public GameObject[] skillPivot;
    public float[] interval;
    public bool isLeft;
    public bool isRight;
    public bool isButtonDownSkill;
    public float SkillCoolTime;
    public Sprite skillImage;
    public string skillAnim;
    public bool isWeaponStopMove;
    public int useStemina;
    public float attckSpeed;
    Weapon weapon;
    int[] damgeArray = new int[10];
    public Rigidbody2D rb;

    public bool isHoldSkill = false;
    public float holdingTime = 0;
    public bool isCancleAttack = false;

    bool isActiveHoldA = false;
    bool isActiveHoldB = false;
    bool isActiveHoldC = false;
    bool isActiveHoldD = false;
    GameObject damageObject;
    DamageObject dmOb;

    Sequence sequenceA;
    Sequence sequenceB;
    Sequence sequenceC;
    Sequence sequenceD;


    private void Awake()
    {
        skillCooldown = GameObject.FindWithTag("Cooldown").GetComponent<SkillCooldown>();
        weapon = transform.parent.gameObject.GetComponent<Weapon>();
        damgeArray = weapon.damgeArray;

        action = new KeyAction();
        skillAAction = action.Player.SkillA;
        skillSAction = action.Player.SkillS;
        skillDAction = action.Player.SkillD;
        skillFAction = action.Player.SkillF;
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
        DatabaseManager.checkAttackLadder = true;
        weapon.isAttackWait = false;
        if (isHoldSkill == false)
        {
            Sequence sequence = DOTween.Sequence()
.AppendInterval(attckSpeed) // 사전에 지정한 공격 주기만큼 대기.
.AppendCallback(() => DatabaseManager.weaponStopMove = false)
.AppendCallback(() => DatabaseManager.checkAttackLadder = false)
.AppendCallback(() => weapon.isAttackWait = true);
        }
        else
        {
            DatabaseManager.weaponStopMove = false;
            DatabaseManager.checkAttackLadder = false;
            weapon.isAttackWait = true;
        }

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


    private IEnumerator SpawnSkills()
    {
        // interval[0]의 시간만큼 대기
        yield return new WaitForSeconds(interval[0]);

        // 첫 번째 요소는 건너뛰고 두 번째 요소부터 생성
        for (int i = 1; i < skillprefab.Length && i < skillPivot.Length; i++)
        {
            // 스킬 프리팹을 피벗 위치에 인스턴스화
            damageObject = Instantiate(skillprefab[i], skillPivot[i].transform.position, skillPivot[i].transform.rotation, this.transform);
            dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.SetDamge(damgeArray);
            // 다음 스킬을 생성하기 전에 interval[i]의 시간만큼 대기
            if (i < interval.Length)
            {
                yield return new WaitForSeconds(interval[i]);
            }
        }
    }
    public void ActiveLeft()
    {

        if (isButtonDownSkill && skillCooldown.isCooldownA == false && PlayerHealthManager.Instance.nowStemina > useStemina &&(weapon.isAttackWait|| isCancleAttack))
        {
            PlayerController.instance.ActiveAttackAnim(skillAnim, attckSpeed);
            PlayerHealthManager.Instance.SteminaDown(useStemina);


             damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform);

             dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.SetDamge(damgeArray);
            if (skillprefab.Length > 1)
            {
                StartCoroutine(SpawnSkills());
            }

            if (isHoldSkill == false)
            {
                skillCooldown.UseSkill();
                CheckAttackWait();
            }
            else
            {
                if (isWeaponStopMove == true)
                {
                    DatabaseManager.weaponStopMove = true;
                    rb.velocity = Vector2.zero;
                }
                isActiveHoldA = true;
                sequenceA = DOTween.Sequence()
               .AppendInterval(holdingTime) // 사전에 지정한 공격 주기만큼 대기.
               .AppendCallback(() => isActiveHoldA = false)
               .AppendCallback(() => skillCooldown.UseSkill())
               .AppendCallback(() => CheckAttackWait())
               .AppendCallback(() => dmOb.DestroyObject());

            }


        }

    }
    public void ActiveRight()
    {

        if (isButtonDownSkill && skillCooldown.isCooldownB == false && PlayerHealthManager.Instance.nowStemina > useStemina && (weapon.isAttackWait || isCancleAttack))
        {

            PlayerController.instance.ActiveAttackAnim(skillAnim, attckSpeed);
            PlayerHealthManager.Instance.SteminaDown(useStemina);

             damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform);

             dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.SetDamge(damgeArray);
            if (skillprefab.Length > 1)
            {
                StartCoroutine(SpawnSkills());
            }
            if (isHoldSkill == false)
            {
                skillCooldown.UseSkillB();
                CheckAttackWait();
            }
            else
            {
                if (isWeaponStopMove == true)
                {
                    DatabaseManager.weaponStopMove = true;
                    rb.velocity = Vector2.zero;
                }
                isActiveHoldB = true;
                sequenceB = DOTween.Sequence()
               .AppendInterval(holdingTime) // 사전에 지정한 공격 주기만큼 대기.
               .AppendCallback(() => isActiveHoldB = false)
               .AppendCallback(() => skillCooldown.UseSkillB())
               .AppendCallback(() => CheckAttackWait())
               .AppendCallback(() => dmOb.DestroyObject());

            }
        }
    }
    public void ActiveSideLeft()
    {

        if (isButtonDownSkill && skillCooldown.isCooldownC == false && PlayerHealthManager.Instance.nowStemina > useStemina && (weapon.isAttackWait || isCancleAttack))
        {
            PlayerController.instance.ActiveAttackAnim(skillAnim, attckSpeed);
            PlayerHealthManager.Instance.SteminaDown(useStemina);
             damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform);
             dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.SetDamge(damgeArray);
            if (skillprefab.Length > 1)
            {
                StartCoroutine(SpawnSkills());
            }
            if (isHoldSkill == false)
            {
                skillCooldown.UseSkillC();
                CheckAttackWait();
            }
            else
            {
                if (isWeaponStopMove == true)
                {
                    DatabaseManager.weaponStopMove = true;
                    rb.velocity = Vector2.zero;
                }
                isActiveHoldC = true;
                 sequenceC = DOTween.Sequence()
                .AppendInterval(holdingTime) // 사전에 지정한 공격 주기만큼 대기.
                .AppendCallback(() => isActiveHoldC = false)
                .AppendCallback(() => skillCooldown.UseSkillC())
                .AppendCallback(() => CheckAttackWait())
                .AppendCallback(() => dmOb.DestroyObject());

            }
        }
    }
    public void ActiveSideRight()
    {

        if (isButtonDownSkill && skillCooldown.isCooldownD == false && PlayerHealthManager.Instance.nowStemina > useStemina && (weapon.isAttackWait || isCancleAttack))
        {
            PlayerController.instance.ActiveAttackAnim(skillAnim, attckSpeed);
            PlayerHealthManager.Instance.SteminaDown(useStemina);
             damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform);
             dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.SetDamge(damgeArray);
            if (skillprefab.Length > 1)
            {
                StartCoroutine(SpawnSkills());
            }
            if (isHoldSkill == false)
            {
                skillCooldown.UseSkillD();
                CheckAttackWait();
            }
            else
            {
                if (isWeaponStopMove == true)
                {
                    DatabaseManager.weaponStopMove = true;
                    rb.velocity = Vector2.zero;
                }
                isActiveHoldD = true;
                sequenceD = DOTween.Sequence()
               .AppendInterval(holdingTime) // 사전에 지정한 공격 주기만큼 대기.
               .AppendCallback(() => isActiveHoldD = false)
               .AppendCallback(() => skillCooldown.UseSkillD())
               .AppendCallback(() => CheckAttackWait())
               .AppendCallback(() => dmOb.DestroyObject());

            }
        }
    }


    void OnSkillAReleased(InputAction.CallbackContext context)
    {
        if (isHoldSkill == true)
        {
            if (isActiveHoldA == true)
            {
                Destroy(damageObject.gameObject); // 생성된 오브젝트 삭제.
                // -> 이거 말고 데미지 오브젝트에서 함수 발동하는 방식으로 변경!.
                isActiveHoldA = false;
                sequenceA.Kill();
                skillCooldown.UseSkill();
                CheckAttackWait();
                PlayerController.instance.StopAttackAnim();
                dmOb.DestroyObject();
            }
        }
    }
    void OnSkillSReleased(InputAction.CallbackContext context)
    {
        if (isHoldSkill == true)
        {
            if (isActiveHoldB == true)
            {
                Destroy(damageObject.gameObject); // 생성된 오브젝트 삭제.
                // -> 이거 말고 데미지 오브젝트에서 함수 발동하는 방식으로 변경!.
                isActiveHoldB = false;
                sequenceB.Kill();
                skillCooldown.UseSkillB();
                CheckAttackWait();
                PlayerController.instance.StopAttackAnim();
                dmOb.DestroyObject();
            }
        }
    }
    void OnSkillDReleased(InputAction.CallbackContext context)
    {
        if (isHoldSkill == true)
        {
            if (isActiveHoldC == true)
            {
                Destroy(damageObject.gameObject); // 생성된 오브젝트 삭제.
                // -> 이거 말고 데미지 오브젝트에서 함수 발동하는 방식으로 변경!.
                isActiveHoldC = false;
                sequenceC.Kill();
                skillCooldown.UseSkillC();
                CheckAttackWait();
                PlayerController.instance.StopAttackAnim();
                dmOb.DestroyObject();
            }
        }
    }
    void OnSkillFReleased(InputAction.CallbackContext context)
    {
        if (isHoldSkill == true)
        {
            if (isActiveHoldD == true)
            {
                Destroy(damageObject.gameObject); // 생성된 오브젝트 삭제.
                // -> 이거 말고 데미지 오브젝트에서 함수 발동하는 방식으로 변경!.
                isActiveHoldD = false;
                sequenceD.Kill();
                skillCooldown.UseSkillD();
                CheckAttackWait();
                PlayerController.instance.StopAttackAnim();
                dmOb.DestroyObject();
            }
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}
