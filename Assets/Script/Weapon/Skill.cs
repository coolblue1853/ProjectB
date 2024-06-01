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
    public int objectMaxCount; // �ִ� �߻����� �����ϴ� ������ ȿ���� ����. �ƹ��� ���Ƶ� ���⿡ ������ Ƚ����ŭ ����.
    bool isActiveHoldA = false;
    bool isActiveHoldB = false;
    bool isActiveHoldC = false;
    bool isActiveHoldD = false;
    GameObject damageObject;
    DamageObject dmOb;
    public GameObject player;
    Sequence sequenceA;
    Sequence sequenceB;
    Sequence sequenceC;
    Sequence sequenceD;
    public bool isNullParent = false;
    public bool isRayCheckSkill = false; // �������� ������ ���. �� �տ��� �����.
    [ConditionalHide("isRayCheckSkill")]
    public Vector2[] direction; // �̵� ����, ����Ʈ������. ���͹��� ���������� ��ü ��ų-1����ŭ ����
    [ConditionalHide("isRayCheckSkill")]
    public float[] distance;    // �̵� �Ÿ�
    [ConditionalHide("isRayCheckSkill")]
    public GameObject rayPositon;    // ray�� �����ϴ� ��ġ.
    [ConditionalHide("isRayCheckSkill")]
    public LayerMask collisionLayer; // �浹 üũ�� ���� ���̾�


    public bool isGorundCheckSkill = false; // ������ �����Ǵ� ���.
    [ConditionalHide("isGorundCheckSkill")]
    public float groundCheckLength;
    [ConditionalHide("isGorundCheckSkill")]
    public LayerMask groundCollisionLayer; // �浹 üũ�� ���� ���̾�
    [ConditionalHide("isGorundCheckSkill")]
    public float yPivot; // �浹 üũ�� ���� ���̾�


    public bool isRoundAttack; // �����ǰ���, Ư�� �������� ��ä�� �Ѹ��� ��������
    [ConditionalHide("isRoundAttack")]
     public float radius = 5f;       // ���� ������
    [ConditionalHide("isRoundAttack")]
    public float startAngle = 0f;   // ���� ����
    [ConditionalHide("isRoundAttack")]
    public float endAngle = 360f;   // �� ����
    [ConditionalHide("isRoundAttack")]
    public int bulletCount = 30;    // ������ ź�� ����


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
        player = GameObject.FindWithTag("Player");
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
        weapon.isSkillAttackWait = false;
        if (isHoldSkill == false)
        {
            Sequence sequence = DOTween.Sequence()
.AppendInterval(attckSpeed / (1 + (DatabaseManager.attackSpeedBuff / 100))) // ������ ������ ���� �ֱ⸸ŭ ���.
.AppendCallback(() => DatabaseManager.weaponStopMove = false)
.AppendCallback(() => DatabaseManager.checkAttackLadder = false)
.AppendCallback(() => weapon.isSkillAttackWait = true);
        }
        else
        {
            DatabaseManager.weaponStopMove = false;
            DatabaseManager.checkAttackLadder = false;
            weapon.isSkillAttackWait = true;
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
        // interval[0]�� �ð���ŭ ���
        yield return new WaitForSeconds(interval[0]);
        // ù ��° ��Ҵ� �ǳʶٰ� �� ��° ��Һ��� ����
        for (int i = 1; i < skillprefab.Length && i < skillPivot.Length && i < objectMaxCount; i++)
        {
            if(isRayCheckSkill == false)
            {
                if(isGorundCheckSkill == false)
                {
                    // ��ų �������� �ǹ� ��ġ�� �ν��Ͻ�ȭ
                    damageObject = Instantiate(skillprefab[i], skillPivot[i].transform.position, skillPivot[i].transform.rotation, this.transform);
                    if (isNullParent == true)
                    {
                        damageObject.transform.parent = null;
                    }
                }
                else
                {
                    Vector2 newDir = new Vector2(0, -1);
                    Vector2 currentPosition = new Vector2(rayPositon.transform.position.x, rayPositon.transform.position.y);
                    Vector2 destination = currentPosition + newDir.normalized * groundCheckLength;
                    RaycastHit2D hit = Physics2D.Raycast(currentPosition, newDir, groundCheckLength, collisionLayer);

                    if (hit.collider == null) // �浹�� ������ ��ȯ x �ƿ� ����� �ȵ�
                    {
                    }
                    else
                    {
                        // �浹�� �ִ� ���, �浹 ���� �տ� ������Ʈ�� �̵�
                        Vector2 safePosition = hit.point - newDir.normalized; // �浹 �������� �ణ ������ ��ġ
                        safePosition = new Vector2(safePosition.x, safePosition.y + yPivot);
                        damageObject = Instantiate(skillprefab[i], new Vector2(skillPivot[i].transform.position.x, safePosition.y), skillPivot[i].transform.rotation, this.transform);
                        if (isNullParent == true)
                        {
                            damageObject.transform.parent = null;
                        }

                    }
                }

            }
            else // ray�� ����ϴ� ���. �������� ���ϴ� ����μ� pivot�� �ϳ��� �־ �ȴ�.
            {
                Vector2 newDir = new Vector2(Mathf.Abs(direction[i - 1].x), direction[i - 1].y);
                if (player.transform.localScale.x < 0)
                {
                    newDir = new Vector2(-Mathf.Abs(direction[i - 1].x), direction[i - 1].y);
                }
                Vector2 currentPosition = new Vector2(rayPositon.transform.position.x, rayPositon.transform.position.y);
                Vector2 destination = currentPosition + newDir.normalized * distance[i - 1];
                RaycastHit2D hit = Physics2D.Raycast(currentPosition, newDir, distance[i - 1], collisionLayer);


                if (hit.collider == null)
                {

                    if (isGorundCheckSkill == false)
                    {
                        damageObject = Instantiate(skillprefab[i], new Vector2(destination.x, skillPivot[i].transform.position.y), skillPivot[i].transform.rotation, this.transform);
                        if (isNullParent == true)
                        {
                            damageObject.transform.parent = null;
                        }
                    }
                    else
                    {
                        Vector2 newDir2 = new Vector2(0, -1);
                        Vector2 currentPosition2 = new Vector2(destination.x, rayPositon.transform.position.y);
                        Vector2 destination2 = currentPosition2 + newDir2.normalized * groundCheckLength;
                        RaycastHit2D hit2 = Physics2D.Raycast(currentPosition2, newDir2, groundCheckLength, collisionLayer);
                        if (hit2.collider != null) // �浹�� ������ ��ȯ x �ƿ� ����� �ȵ�
                        {                            // �浹�� �ִ� ���, �浹 ���� �տ� ������Ʈ�� �̵�
                            Vector2 safePosition2 = hit2.point - newDir2.normalized; // �浹 �������� �ణ ������ ��ġ
                            safePosition2 = new Vector2(safePosition2.x, safePosition2.y + yPivot);
                            damageObject = Instantiate(skillprefab[i], new Vector2(destination.x, safePosition2.y), skillPivot[i].transform.rotation, this.transform);
                            if (isNullParent == true)
                            {
                                damageObject.transform.parent = null;
                            }
                        }
                    }

                }
                else
                {
                    if (isGorundCheckSkill == false)
                    {
                        // �浹�� �ִ� ���, �浹 ���� �տ� ������Ʈ�� �̵�
                        Vector2 safePosition = hit.point - newDir.normalized * 0.2f; // �浹 �������� �ణ ������ ��ġ
                        safePosition = new Vector2(safePosition.x, safePosition.y);
                        damageObject = Instantiate(skillprefab[i], new Vector2(safePosition.x, skillPivot[i].transform.position.y), skillPivot[i].transform.rotation, this.transform);
                        if (isNullParent == true)
                        {
                            damageObject.transform.parent = null;
                        }
                    }
                    else
                    {                        // �浹�� �ִ� ���, �浹 ���� �տ� ������Ʈ�� �̵�
                        Vector2 safePosition = hit.point - newDir.normalized * 0.2f; // �浹 �������� �ణ ������ ��ġ
                        safePosition = new Vector2(safePosition.x, safePosition.y);
                        Vector2 newDir2 = new Vector2(0, -1);
                        Vector2 currentPosition2 = new Vector2(safePosition.x, rayPositon.transform.position.y);
                        RaycastHit2D hit2 = Physics2D.Raycast(currentPosition2, newDir2, groundCheckLength, collisionLayer);
                        if (hit2.collider != null) // �浹�� ������ ��ȯ x �ƿ� ����� �ȵ�
                        {                            // �浹�� �ִ� ���, �浹 ���� �տ� ������Ʈ�� �̵�
                            Vector2 safePosition2 = hit2.point - newDir2.normalized; // �浹 �������� �ణ ������ ��ġ
                            safePosition2 = new Vector2(safePosition2.x, safePosition2.y + yPivot);
                            damageObject = Instantiate(skillprefab[i], new Vector2(safePosition.x, safePosition2.y), skillPivot[i].transform.rotation, this.transform);
                            if (isNullParent == true)
                            {
                                damageObject.transform.parent = null;
                            }
                        }
                    }
                }
          
            }

           
            
            dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.SetDamge(damgeArray);
            // ���� ��ų�� �����ϱ� ���� interval[i]�� �ð���ŭ ���
            if (i < interval.Length)
            {
                yield return new WaitForSeconds(interval[i]);
            }
        }
    }

    public void RoundAttack(GameObject damageOb, GameObject attackPivot)
    {
        float angleStep = (endAngle - startAngle) / (bulletCount - 1); // ź�� ���� ���

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + i * angleStep; // ���� ź���� ���� ���

            // �� ������Ʈ�� ���⿡ ���� ź���� ������ �����մϴ�.
            if (player.transform.localScale.x < 0)
            {
                angle = 180f - angle;
            }

            // ������ �������� ��ȯ�մϴ�.
            float angleInRadians = angle * Mathf.Deg2Rad;

            // ź���� ��ġ ���
            float x = radius * Mathf.Cos(angleInRadians);
            float y = radius * Mathf.Sin(angleInRadians);

            // ź�� ����
            GameObject bullet = GameObject.Instantiate(damageOb, attackPivot.transform.position + new Vector3(x, y, 0f), Quaternion.identity, this.transform);
            if (bullet != null)
            {
                if (isNullParent == true)
                {
                    bullet.transform.parent = null;
                }
                dmOb = bullet.GetComponent<DamageObject>();
                dmOb.SetDamge(damgeArray);

                // ź�� ���� ����
                bullet.transform.up = new Vector2(x, y).normalized;
            }
        }
    }

    public void ActiveLeft()
    {
        if (isButtonDownSkill && skillCooldown.isCooldownA == false && PlayerHealthManager.Instance.nowStemina > useStemina && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack))
        {
            bool isActive = true;

            if(isRoundAttack == true)
            {
                RoundAttack(skillprefab[0], skillPivot[0]);
            }
            else if (isGorundCheckSkill == false)
            {
                damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform);
            }
            else
            {
                Vector2 newDir = new Vector2(0, -1);
                Vector2 currentPosition = new Vector2(rayPositon.transform.position.x, rayPositon.transform.position.y);
                Vector2 destination = currentPosition + newDir.normalized * groundCheckLength;
                RaycastHit2D hit = Physics2D.Raycast(currentPosition, newDir, groundCheckLength, collisionLayer);

                if (hit.collider == null) // �浹�� ������ ��ȯ x �ƿ� ����� �ȵ�
                {
                    isActive = false;
                }
                else
                {
                    // �浹�� �ִ� ���, �浹 ���� �տ� ������Ʈ�� �̵�
                    Vector2 safePosition = hit.point - newDir.normalized; // �浹 �������� �ణ ������ ��ġ
                    safePosition = new Vector2(safePosition.x, safePosition.y + yPivot);
                    damageObject = Instantiate(skillprefab[0], new Vector2(skillPivot[0].transform.position.x, safePosition.y), skillPivot[0].transform.rotation, this.transform);

                }
            }


            if(isActive == true)
            {
                PlayerController.instance.ActiveAttackAnim(skillAnim, attckSpeed / (1 + (DatabaseManager.attackSpeedBuff / 100)));
                PlayerHealthManager.Instance.SteminaDown(useStemina);

                if(damageObject != null)
                {
                    if (isNullParent == true)
                    {
                        damageObject.transform.parent = null;
                    }
                    dmOb = damageObject.GetComponent<DamageObject>();
                    dmOb.SetDamge(damgeArray);
                }

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
                   .AppendInterval(holdingTime) // ������ ������ ���� �ֱ⸸ŭ ���.
                   .AppendCallback(() => isActiveHoldA = false)
                   .AppendCallback(() => skillCooldown.UseSkill())
                   .AppendCallback(() => CheckAttackWait())
                   .AppendCallback(() => dmOb.DestroyObject());

                }
            }
         


        }

    }
    public void ActiveRight()
    {

        if (isButtonDownSkill && skillCooldown.isCooldownB == false && PlayerHealthManager.Instance.nowStemina > useStemina && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack))
        {
            bool isActive = true;

            if (isGorundCheckSkill == false)
            {
                damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform);
            }
            else
            {
                Vector2 newDir = new Vector2(0, -1);
                Vector2 currentPosition = new Vector2(rayPositon.transform.position.x, rayPositon.transform.position.y);
                Vector2 destination = currentPosition + newDir.normalized * groundCheckLength;
                RaycastHit2D hit = Physics2D.Raycast(currentPosition, newDir, groundCheckLength, collisionLayer);

                if (hit.collider == null) // �浹�� ������ ��ȯ x �ƿ� ����� �ȵ�
                {
                    isActive = false;
                }
                else
                {
                    // �浹�� �ִ� ���, �浹 ���� �տ� ������Ʈ�� �̵�
                    Vector2 safePosition = hit.point - newDir.normalized; // �浹 �������� �ణ ������ ��ġ
                    safePosition = new Vector2(safePosition.x, safePosition.y + yPivot);
                    damageObject = Instantiate(skillprefab[0], new Vector2(skillPivot[0].transform.position.x, safePosition.y), skillPivot[0].transform.rotation, this.transform);

                }
            }

            if (isActive == true)
            {
                PlayerController.instance.ActiveAttackAnim(skillAnim, attckSpeed / (1 + (DatabaseManager.attackSpeedBuff / 100)));
                PlayerHealthManager.Instance.SteminaDown(useStemina);
                if (isNullParent == true)
                {
                    damageObject.transform.parent = null;
                }
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
                   .AppendInterval(holdingTime) // ������ ������ ���� �ֱ⸸ŭ ���.
                   .AppendCallback(() => isActiveHoldB = false)
                   .AppendCallback(() => skillCooldown.UseSkillB())
                   .AppendCallback(() => CheckAttackWait())
                   .AppendCallback(() => dmOb.DestroyObject());

                }
            }
                
        }
    }
    public void ActiveSideLeft()
    {

        if (isButtonDownSkill && skillCooldown.isCooldownC == false && PlayerHealthManager.Instance.nowStemina > useStemina && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack))
        {
            bool isActive = true;

            if(isGorundCheckSkill == false)
            {
                damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform);
            }
            else
            {
                Vector2 newDir = new Vector2(0, -1);
                Vector2 currentPosition = new Vector2(rayPositon.transform.position.x, rayPositon.transform.position.y);
                Vector2 destination = currentPosition + newDir.normalized * groundCheckLength;
                RaycastHit2D hit = Physics2D.Raycast(currentPosition, newDir, groundCheckLength, collisionLayer);

                if (hit.collider == null) // �浹�� ������ ��ȯ x �ƿ� ����� �ȵ�
                {
                    isActive = false;
                }
                else
                {
                    // �浹�� �ִ� ���, �浹 ���� �տ� ������Ʈ�� �̵�
                    Vector2 safePosition = hit.point - newDir.normalized ; // �浹 �������� �ణ ������ ��ġ
                    safePosition = new Vector2(safePosition.x, safePosition.y + yPivot);
                    damageObject = Instantiate(skillprefab[0], new Vector2(skillPivot[0].transform.position.x, safePosition.y), skillPivot[0].transform.rotation, this.transform);

                }
            }


            if(isActive == true)
            {
                PlayerController.instance.ActiveAttackAnim(skillAnim, attckSpeed / (1 + (DatabaseManager.attackSpeedBuff / 100)));
                PlayerHealthManager.Instance.SteminaDown(useStemina);
                if (isNullParent == true)
                {
                    damageObject.transform.parent = null;
                }
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
                   .AppendInterval(holdingTime) // ������ ������ ���� �ֱ⸸ŭ ���.
                   .AppendCallback(() => isActiveHoldC = false)
                   .AppendCallback(() => skillCooldown.UseSkillC())
                   .AppendCallback(() => CheckAttackWait())
                   .AppendCallback(() => dmOb.DestroyObject());

                }
            }

           
        }
    }
    public void ActiveSideRight()
    {

        if (isButtonDownSkill && skillCooldown.isCooldownD == false && PlayerHealthManager.Instance.nowStemina > useStemina && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack))
        {
            bool isActive = true;

            if (isGorundCheckSkill == false)
            {
                damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform);
            }
            else
            {
                Vector2 newDir = new Vector2(0, -1);
                Vector2 currentPosition = new Vector2(rayPositon.transform.position.x, rayPositon.transform.position.y);
                Vector2 destination = currentPosition + newDir.normalized * groundCheckLength;
                RaycastHit2D hit = Physics2D.Raycast(currentPosition, newDir, groundCheckLength, collisionLayer);

                if (hit.collider == null) // �浹�� ������ ��ȯ x �ƿ� ����� �ȵ�
                {
                    isActive = false;
                }
                else
                {
                    // �浹�� �ִ� ���, �浹 ���� �տ� ������Ʈ�� �̵�
                    Vector2 safePosition = hit.point - newDir.normalized; // �浹 �������� �ణ ������ ��ġ
                    safePosition = new Vector2(safePosition.x, safePosition.y + yPivot);
                    damageObject = Instantiate(skillprefab[0], new Vector2(skillPivot[0].transform.position.x, safePosition.y), skillPivot[0].transform.rotation, this.transform);

                }
            }
            if (isActive == true)
            {
                PlayerController.instance.ActiveAttackAnim(skillAnim, attckSpeed / (1 + (DatabaseManager.attackSpeedBuff / 100)));
                PlayerHealthManager.Instance.SteminaDown(useStemina);
                if (isNullParent == true)
                {
                    damageObject.transform.parent = null;
                }

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
                   .AppendInterval(holdingTime) // ������ ������ ���� �ֱ⸸ŭ ���.
                   .AppendCallback(() => isActiveHoldD = false)
                   .AppendCallback(() => skillCooldown.UseSkillD())
                   .AppendCallback(() => CheckAttackWait())
                   .AppendCallback(() => dmOb.DestroyObject());

                }
            }
              
        }
    }


    void OnSkillAReleased(InputAction.CallbackContext context)
    {
        if (isHoldSkill == true)
        {
            if (isActiveHoldA == true)
            {
                Destroy(damageObject.gameObject); // ������ ������Ʈ ����.
                // -> �̰� ���� ������ ������Ʈ���� �Լ� �ߵ��ϴ� ������� ����!.
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
                Destroy(damageObject.gameObject); // ������ ������Ʈ ����.
                // -> �̰� ���� ������ ������Ʈ���� �Լ� �ߵ��ϴ� ������� ����!.
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
                Destroy(damageObject.gameObject); // ������ ������Ʈ ����.
                // -> �̰� ���� ������ ������Ʈ���� �Լ� �ߵ��ϴ� ������� ����!.
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
                Destroy(damageObject.gameObject); // ������ ������Ʈ ����.
                // -> �̰� ���� ������ ������Ʈ���� �Լ� �ߵ��ϴ� ������� ����!.
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
