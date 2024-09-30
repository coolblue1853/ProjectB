using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using DarkTonic.MasterAudio;
public class Skill : MonoBehaviour
{
    KeyAction action;
    InputAction[] skillAction = new InputAction[4]; // Ű�����  A S D F ��
    public string[] skillSound;
    public GameObject[] skillBackGround = new GameObject[4];
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
    public bool holdDisEffectByTime = false; // �̰� ���� ������ ���� ������ ������ �ȹް� ��
    public bool isCancleAttack = false;
    public bool isEffectMaxAdd; // �ִ� �߻��� ������ ������ ��ġ�� ���������� Ȯ��
    [ConditionalHide("isEffectMaxAdd")]
    public int objectMaxCount; // �ִ� �߻����� �����ϴ� ������ ȿ���� ����. �ƹ��� ���Ƶ� ���⿡ ������ Ƚ����ŭ ����.
     bool isActiveHold = false;

    public float skillWaitTime = 0; // �����̽ð�, �� �ð��� ������ ������ ����

    GameObject damageObject;
    DamageObject dmOb;
    public GameObject player;
    Sequence skillSequence;
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
    Vector2 originPoint;
    Vector2 newDir; // rayCheck�� �ʿ��� ������
    Vector2 destination;
    RaycastHit2D hit;
    Vector2 safePosition;

    public bool isGorundCheckSkill = false; // ������ �����Ǵ� ���.
    [ConditionalHide("isGorundCheckSkill")]
    public float groundCheckLength;
    [ConditionalHide("isGorundCheckSkill")]
    public LayerMask groundCollisionLayer; // �浹 üũ�� ���� ���̾�
    [ConditionalHide("isGorundCheckSkill")]
    public float yPivot; // �浹 üũ�� ���� ���̾�
    Vector2 groundCheckDir;
    Vector2 groundCheckPosition;
    RaycastHit2D groundCheckHit;

    public bool isRoundAttack; // �����ǰ���, Ư�� �������� ��ä�� �Ѹ��� ��������
    [ConditionalHide("isRoundAttack")]
    public float radius = 5f;       // ���� ������
    [ConditionalHide("isRoundAttack")]
    public float startAngle = 0f;   // ���� ����
    [ConditionalHide("isRoundAttack")]
    public float endAngle = 360f;   // �� ����
    [ConditionalHide("isRoundAttack")]
    public int bulletCount = 30;    // ������ ź�� ����

    public string skillName;
    public int skillNum;
    public string skillDetail;
    public GameObject delayEffectPrefab;
    public GameObject[] skillprefab;
    public SkillCooldown skillCooldown;
    int allObjectMaxCount;
    int allBullet = 0;
    bool isActive = true;

    private void OnEnable()
    {
        for(int i =0; i < skillAction.Length; i++)
        {
            skillAction[i].Enable();
            skillAction[i].canceled += OnSkillReleased;
        }
    }
    private void OnDisable()
    {
        for (int i = 0; i < skillAction.Length; i++)
        {
            skillAction[i].Disable();
            skillAction[i].canceled -= OnSkillReleased;
        }
    }
    private void Awake()
    {
        originPoint = Vector2.zero;
        skillCooldown = GameObject.FindWithTag("Cooldown").GetComponent<SkillCooldown>();
        for(int i =0; i < skillAction.Length; i++)
            skillBackGround[i] = skillCooldown.transform.GetChild(i + 1).gameObject;

        weapon = transform.parent.gameObject.GetComponent<Weapon>();
        damgeArray = weapon.damgeArray;

        action = new KeyAction();
        skillAction[0] = action.Player.SkillA;
        skillAction[1] = action.Player.SkillS;
        skillAction[2] = action.Player.SkillD;
        skillAction[3] = action.Player.SkillF;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb = transform.parent.parent.GetComponent<Rigidbody2D>();
    }
    public void DisactiveMainSkill(int skillNum)
    {
        skillBackGround[skillNum].SetActive(false);
    }
    public void ActiveMainSkill(int num)
    {
        skillBackGround[num].SetActive(true);
        skillCooldown.cooldownTime[num] = SkillCoolTime;
        skillCooldown.cooldownImage[num].sprite = skillImage;
        skillCooldown.backImage[num].sprite = skillImage;
        skillCooldown.UseSkill(num, skillName);
    }

    void WallCheck( bool isRayCheck, bool isChangeDir, int num)
    {
        if(originPoint == Vector2.zero)
             originPoint = rayPositon.transform.position;
        Vector2 currentPosition = new Vector2(originPoint.x, originPoint.y);
        if (!isRayCheck)
        {
             newDir = new Vector2(0, -1);
            destination = currentPosition + newDir.normalized * groundCheckLength;
            hit = Physics2D.Raycast(currentPosition, newDir, groundCheckLength, collisionLayer);
        }
        else
        {
             newDir = new Vector2(Mathf.Abs(direction[num - 1].x), direction[num - 1].y);
            if (isChangeDir) newDir = new Vector2(-Mathf.Abs(direction[num - 1].x), direction[num - 1].y);
             destination = currentPosition + newDir.normalized * distance[num - 1];
             hit = Physics2D.Raycast(currentPosition, newDir, distance[num - 1], collisionLayer);
        }
        safePosition = hit.point - newDir.normalized * 0.2f; // �浹 �������� �ణ ������ ��ġ
    }
    void GroundCheck(bool isHit, int num)
    {
         groundCheckDir = new Vector2(0, -1);
         groundCheckPosition = new Vector2(destination.x, originPoint.y);
         groundCheckHit = Physics2D.Raycast(groundCheckPosition, groundCheckDir, groundCheckLength, collisionLayer);
        if (groundCheckHit.collider != null) // �浹�� ������ ��ȯ x �ƿ� ����� �ȵ�
        {
            Vector2 groundSafePositon = groundCheckHit.point - groundCheckDir.normalized; // �浹 �������� �ణ ������ ��ġ
            groundSafePositon = new Vector2(groundSafePositon.x, groundSafePositon.y + yPivot);
            if(isHit == false)
                damageObject = Instantiate(skillprefab[num], new Vector2(destination.x, groundSafePositon.y), skillPivot[num].transform.rotation, this.transform);
            else
                damageObject = Instantiate(skillprefab[num], new Vector2(safePosition.x, groundSafePositon.y), skillPivot[num].transform.rotation, this.transform);
        }
    }
    private IEnumerator SpawnSkills() // 
    {
        bool isChangeDir = false;
        if (player.transform.localScale.x < 0) isChangeDir = true;
        if (DatabaseManager.skillHitCount.ContainsKey(skillName)) allObjectMaxCount = objectMaxCount + DatabaseManager.skillHitCount[skillName];
        else allObjectMaxCount = objectMaxCount;
        yield return new WaitForSeconds(interval[0] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
        for (int i = 1; i < skillprefab.Length && i < skillPivot.Length && (isEffectMaxAdd == false || i < allObjectMaxCount); i++)
        {
            if (isRayCheckSkill == false) //RayCast�� Ȯ���ϴ� ����ΰ�
            {
                if (isGorundCheckSkill == false) //���� �Ÿ��̻� �����ÿ� ����� ���ϴ� ����ΰ�
                    damageObject = Instantiate(skillprefab[i], skillPivot[i].transform.position, skillPivot[i].transform.rotation, this.transform);
                else
                {
                    WallCheck(false, isChangeDir,i);
                    Vector2 safePosition = hit.point - newDir.normalized; 
                    safePosition = new Vector2(safePosition.x, safePosition.y + yPivot);
                    damageObject = Instantiate(skillprefab[i], new Vector2(skillPivot[i].transform.position.x, safePosition.y), skillPivot[i].transform.rotation, this.transform);
                }
            }
            else // ray�� ����ϴ� ���. �������� ���ϴ� ����μ� pivot�� �ϳ��� �־ �ȴ�.
            {
                WallCheck(true, isChangeDir, i);
                if (hit.collider == null)
                {
                    if (isGorundCheckSkill == false)
                        damageObject = Instantiate(skillprefab[i], new Vector2(destination.x, skillPivot[i].transform.position.y), skillPivot[i].transform.rotation, this.transform);
                    else //groundCheck�� ������ϴ°��
                        GroundCheck(false, i);
                }
                else
                {
                    if (isGorundCheckSkill == false)
                        damageObject = Instantiate(skillprefab[i], new Vector2(safePosition.x, skillPivot[i].transform.position.y), skillPivot[i].transform.rotation, this.transform);
                    else
                        GroundCheck(true, i);
                }
            }
            MasterAudio.PlaySound(skillSound[i]);
            if (isNullParent == true && damageObject!= null) damageObject.transform.parent = null;
            dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.skillName = skillName;
            dmOb.SetDamge(damgeArray);
            // ���� ��ų�� �����ϱ� ���� interval[i]�� �ð���ŭ ���
            if (i < interval.Length)
                yield return new WaitForSeconds(interval[i] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
        }
        originPoint = Vector2.zero;
    }

    public void RoundSpawn(GameObject damageOb, GameObject attackPivot)
    {
        if (DatabaseManager.skillBulletCount.ContainsKey(skillName))
            allBullet = bulletCount + DatabaseManager.skillBulletCount[skillName]; // ������ �� �ش� ����� źȯ �߰��� �ִ°�� ���ڸ� ������ŵ�ϴ�.
        else
            allBullet = bulletCount;
        float angleStep = (endAngle - startAngle) / (allBullet - 1); // ź�� ���� ���
        for (int i = 0; i < allBullet; i++)
        {
            float angle = startAngle + i * angleStep; // ���� ź���� ���� ���
            // �� ������Ʈ�� ���⿡ ���� ź���� ������ �����մϴ�.
            if (player.transform.localScale.x < 0) angle = 180f - angle;

            // ������ �������� ��ȯ�մϴ�.
            float angleInRadians = angle * Mathf.Deg2Rad;

            // ź���� ��ġ ���
            float x = radius * Mathf.Cos(angleInRadians);
            float y = radius * Mathf.Sin(angleInRadians);

            // ź�� ����
            GameObject bullet = GameObject.Instantiate(damageOb, attackPivot.transform.position + new Vector3(x, y, 0f), Quaternion.identity, this.transform);
            if (bullet != null)
            {
                if (isNullParent == true) bullet.transform.parent = null;
                dmOb = bullet.GetComponent<DamageObject>();
                dmOb.skillName = skillName;
                dmOb.SetDamge(damgeArray);

                // ź�� ���� ����
                bullet.transform.up = new Vector2(x, y).normalized;
            }
        }
    }

    public void ActiveSkill(int num)
    {
        if (isButtonDownSkill && skillCooldown.isCooldown[num] == false && PlayerHealthManager.Instance.nowStemina > useStemina 
            && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack))
        {
            isActive = true;
            if (isRoundAttack == true) RoundAttack(skillWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)));
            else if (isGorundCheckSkill == false) NomalAttack(skillWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)));
            else
            {
                if (skillWaitTime == 0) GroundAttack();
                else
                {
                    Sequence sequence = DOTween.Sequence()
                    .AppendInterval(skillWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)))
                    .AppendCallback(() => GroundAttack());
                }
            }
            SkillAnimCheck(num);
        }
    }
   
    void OnSkillReleased(InputAction.CallbackContext context)
    {
        if (isHoldSkill == true)
        {
            if (isActiveHold == true)
            {
                Destroy(damageObject.gameObject); // ������ ������Ʈ ����.
                isActiveHold = false;
                skillSequence.Kill();
                skillCooldown.UseSkill(skillNum, skillName);
                CheckAttackWait();
                PlayerController.instance.StopAttackAnim();
                dmOb.DestroyObject();
            }
        }
    }
  
    void GroundAttack() // ������ �����Ǵ� ���, ���� ��ü
    {
        Vector2 newDir = new Vector2(0, -1);
        Vector2 currentPosition = new Vector2(rayPositon.transform.position.x, rayPositon.transform.position.y);
        Vector2 destination = currentPosition + newDir.normalized * groundCheckLength;
        RaycastHit2D hit = Physics2D.Raycast(currentPosition, newDir, groundCheckLength, collisionLayer);
        if (hit.collider == null) // �浹�� ������ ��ȯ x �ƿ� ����� �ȵ�
            isActive = false;
        else
        {
            Vector2 safePosition = hit.point - newDir.normalized; // �浹 �������� �ణ ������ ��ġ
            safePosition = new Vector2(safePosition.x, safePosition.y + yPivot);
            damageObject = Instantiate(skillprefab[0], new Vector2(skillPivot[0].transform.position.x, safePosition.y), skillPivot[0].transform.rotation, this.transform);
            MasterAudio.PlaySound(skillSound[0]);
        }
    }

    void SkillEffect() // ��� ���ϸ��̼� �� ������ ������Ʈ�� ���ݼ���
    {
        PlayerController.instance.ActiveAttackAnim(false,skillAnim, attckSpeed / (1 + (DatabaseManager.attackSpeedBuff / 100)));
        PlayerHealthManager.Instance.SteminaDown(useStemina);

        float waitTime = 0;
        waitTime = skillWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100));
        if (delayEffectPrefab != null)
        {
            GameObject effect = Instantiate(delayEffectPrefab, new Vector2(skillPivot[0].transform.position.x, skillPivot[0].transform.position.y), skillPivot[0].transform.rotation);
            MasterAudio.PlaySound(skillSound[0]);
            DestoryByTime dBT = effect.GetComponent<DestoryByTime>();
            dBT.DestroyEffect(waitTime);
        }
        if (waitTime == 0) SetSkillDmg();
        else
        {
            skillSequence = DOTween.Sequence()
           .AppendInterval(waitTime) // ������ ������ ���� �ֱ⸸ŭ ���.
           .AppendCallback(() => SetSkillDmg());
        }

    }
    void SetSkillDmg()
    {
        if(isRoundAttack == false)
        {
            if (isNullParent == true) damageObject.transform.parent = null;
            dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.skillName = skillName;
            dmOb.SetDamge(damgeArray);
            if (skillprefab.Length > 1) StartCoroutine(SpawnSkills());// �ټ��� ������Ʈ�� �����ϴ� ����� ���
        }

    }

    void NomalAttack(float waitTime) // �Ϲ����� ������ ����
    {
        if (waitTime == 0)
        {
            damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform);
            MasterAudio.PlaySound(skillSound[0]);
        }
        else
        {
            Sequence sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .AppendCallback(() => damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform));
            MasterAudio.PlaySound(skillSound[0]);
        }
    }
    void RoundAttack(float waitTime)
    {
        if (waitTime == 0)
        {
            RoundSpawn(skillprefab[0], skillPivot[0]);
            MasterAudio.PlaySound(skillSound[0]);
        }
        else
        {
            Sequence sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .AppendCallback(() => RoundSpawn(skillprefab[0], skillPivot[0]));
            MasterAudio.PlaySound(skillSound[0]);
        }
    }
    void SkillAnimCheck(int skillNum) // ����� �ִϸ��̼� ����� Ȧ�� ��ų�� ����
    {
        if (isActive == true)
        {
            SkillEffect();
            if (isHoldSkill == false)
            {
                skillCooldown.UseSkill(skillNum, skillName);
                CheckAttackWait();
            }
            else
            {
                if (isWeaponStopMove == true)
                {
                    DatabaseManager.weaponStopMove = true;
                    rb.velocity = Vector2.zero;
                }

                float waitTime =0;
                if (holdDisEffectByTime) waitTime = holdingTime;
                else waitTime = holdingTime / (1 + (DatabaseManager.attackSpeedBuff / 100));
                isActiveHold = true;
                skillSequence = DOTween.Sequence()
               .AppendInterval(waitTime) // ������ ������ ���� �ֱ⸸ŭ ���.
               .AppendCallback(() => isActiveHold = false)      
               .AppendCallback(() => skillCooldown.UseSkill(skillNum, skillName))
               .AppendCallback(() => CheckAttackWait())
               .AppendCallback(() => dmOb.DestroyObject());
            }
        }
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
}
