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
    InputAction attackAction;
    InputAction[] skillAction = new InputAction[4]; // 키보드로  A S D F 순
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
    public bool holdDisEffectByTime = false; // 이게 켜져 있으면 공속 버프에 영향을 안받게 됨
    public bool isCancleAttack = false;
    public bool isEffectMaxAdd; // 최대 발생량 증가가 영향을 끼치는 아이템인지 확인
    [ConditionalHide("isEffectMaxAdd")]
    public int objectMaxCount; // 최대 발생량이 증가하는 아이템 효과를 위함. 아무리 많아도 여기에 지정된 횟수만큼 나감.
    bool isActiveHoldA = false;
    bool isActiveHoldB = false;
    bool isActiveHoldC = false;
    bool isActiveHoldD = false;
    public float skillAWaitTime = 0; // 딜레이시간, 이 시간이 지나고 프리팹 생성
    public float skillBWaitTime = 0; // 딜레이시간, 이 시간이 지나고 프리팹 생성
    public float skillCWaitTime = 0; // 딜레이시간, 이 시간이 지나고 프리팹 생성
    public float skillDWaitTime = 0; // 딜레이시간, 이 시간이 지나고 프리팹 생성

    GameObject damageObject;
    DamageObject dmOb;
    public GameObject player;
    Sequence sequenceA;
    Sequence sequenceB;
    Sequence sequenceC;
    Sequence sequenceD;
    public bool isNullParent = false;
    public bool isRayCheckSkill = false; // 전방으로 나가는 기술. 벽 앞에서 멈춘다.

    [ConditionalHide("isRayCheckSkill")]
    public Vector2[] direction; // 이동 방향, 리스트형태임. 인터벌과 마찬가지로 전체 스킬-1개만큼 존재
    [ConditionalHide("isRayCheckSkill")]
    public float[] distance;    // 이동 거리
    [ConditionalHide("isRayCheckSkill")]
    public GameObject rayPositon;    // ray가 시작하는 위치.
    [ConditionalHide("isRayCheckSkill")]
    public LayerMask collisionLayer; // 충돌 체크를 위한 레이어
    Vector2 originPoint;
    Vector2 newDir; // rayCheck에 필요한 변수들
    Vector2 destination;
    RaycastHit2D hit;
    Vector2 safePosition;

    public bool isGorundCheckSkill = false; // 땅에서 생성되는 기술.
    [ConditionalHide("isGorundCheckSkill")]
    public float groundCheckLength;
    [ConditionalHide("isGorundCheckSkill")]
    public LayerMask groundCollisionLayer; // 충돌 체크를 위한 레이어
    [ConditionalHide("isGorundCheckSkill")]
    public float yPivot; // 충돌 체크를 위한 레이어
    Vector2 groundCheckDir;
    Vector2 groundCheckPosition;
    RaycastHit2D groundCheckHit;

    public bool isRoundAttack; // 원형의공격, 특정 각도내에 전채로 뿌리는 공격형식
    [ConditionalHide("isRoundAttack")]
    public float radius = 5f;       // 원의 반지름
    [ConditionalHide("isRoundAttack")]
    public float startAngle = 0f;   // 시작 각도
    [ConditionalHide("isRoundAttack")]
    public float endAngle = 360f;   // 끝 각도
    [ConditionalHide("isRoundAttack")]
    public int bulletCount = 30;    // 생성할 탄막 개수


    public string skillName;
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
            skillAction[i].canceled += OnSkillAReleased;
        }
    }
    private void OnDisable()
    {
        for (int i = 0; i < skillAction.Length; i++)
        {
            skillAction[i].Disable();
            skillAction[i].canceled -= OnSkillAReleased;
        }
    }
    private void Awake()
    {
        skillCooldown = GameObject.FindWithTag("Cooldown").GetComponent<SkillCooldown>();
        for(int i =0; i < skillAction.Length; i++)
        {
            skillBackGround[i] = skillCooldown.transform.GetChild(i+1).gameObject;
        }

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

    void SkillRayCheck(bool isRayCheck, bool isChangeDir, int num)
    {
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
        safePosition = hit.point - newDir.normalized * 0.2f; // 충돌 지점에서 약간 떨어진 위치
    }
    void SkillGroundCheck(bool isHit, int num)
    {
         groundCheckDir = new Vector2(0, -1);
         groundCheckPosition = new Vector2(destination.x, originPoint.y);
         groundCheckHit = Physics2D.Raycast(groundCheckPosition, groundCheckDir, groundCheckLength, collisionLayer);
        if (groundCheckHit.collider != null) // 충돌이 없으면 소환 x 아예 사용이 안됨
        {
            Vector2 groundSafePositon = groundCheckHit.point - groundCheckDir.normalized; // 충돌 지점에서 약간 떨어진 위치
            groundSafePositon = new Vector2(groundSafePositon.x, groundSafePositon.y + yPivot);
            if(isHit == false)
                damageObject = Instantiate(skillprefab[num], new Vector2(destination.x, groundSafePositon.y), skillPivot[num].transform.rotation, this.transform);
            else
                damageObject = Instantiate(skillprefab[num], new Vector2(safePosition.x, groundSafePositon.y), skillPivot[num].transform.rotation, this.transform);
        }
    }
    private IEnumerator SpawnSkills() // 다수의 DamageObject를 생성해야 하는 경우
    {
        bool isChangeDir = false;
        if (player.transform.localScale.x < 0) isChangeDir = true;
        if (DatabaseManager.skillHitCount.ContainsKey(skillName)) allObjectMaxCount = objectMaxCount + DatabaseManager.skillHitCount[skillName];
        else allObjectMaxCount = objectMaxCount;
        yield return new WaitForSeconds(interval[0] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
        for (int i = 1; i < skillprefab.Length && i < skillPivot.Length && (isEffectMaxAdd == false || i < allObjectMaxCount); i++)
        {
            if (isRayCheckSkill == false) //RayCast를 확인하는 기술인가
            {
                if (isGorundCheckSkill == false) //일정 거리이상 점프시에 사용을 못하는 기술인가
                    damageObject = Instantiate(skillprefab[i], skillPivot[i].transform.position, skillPivot[i].transform.rotation, this.transform);
                else
                {
                    SkillRayCheck(false, isChangeDir,i);
                    Vector2 safePosition = hit.point - newDir.normalized; 
                    safePosition = new Vector2(safePosition.x, safePosition.y + yPivot);
                    damageObject = Instantiate(skillprefab[i], new Vector2(skillPivot[i].transform.position.x, safePosition.y), skillPivot[i].transform.rotation, this.transform);
                }
            }
            else // ray를 사용하는 기술. 전방으로 향하는 기술로서 pivot이 하나만 있어도 된다.
            {
                SkillRayCheck(true, isChangeDir, i);
                if (hit.collider == null)
                {
                    if (isGorundCheckSkill == false)
                        damageObject = Instantiate(skillprefab[i], new Vector2(destination.x, skillPivot[i].transform.position.y), skillPivot[i].transform.rotation, this.transform);
                    else //groundCheck를 해줘야하는경우
                        SkillGroundCheck(false, i);
                }
                else
                {
                    if (isGorundCheckSkill == false)
                        damageObject = Instantiate(skillprefab[i], new Vector2(safePosition.x, skillPivot[i].transform.position.y), skillPivot[i].transform.rotation, this.transform);
                    else
                        SkillGroundCheck(true, i);
                }
            }
            MasterAudio.PlaySound(skillSound[i]);
            if (isNullParent == true) damageObject.transform.parent = null;
            dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.skillName = skillName;
            dmOb.SetDamge(damgeArray);
            // 다음 스킬을 생성하기 전에 interval[i]의 시간만큼 대기
            if (i < interval.Length)
            {
                yield return new WaitForSeconds(interval[i] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
            }
        }
    }

    public void RoundAttack(GameObject damageOb, GameObject attackPivot)
    {
        if (DatabaseManager.skillBulletCount.ContainsKey(skillName))
        {
            allBullet = bulletCount + DatabaseManager.skillBulletCount[skillName];
        }
        else
        {
            allBullet = bulletCount;
        }

        float angleStep = (endAngle - startAngle) / (allBullet - 1); // 탄막 간격 계산

        for (int i = 0; i < allBullet; i++)
        {
            float angle = startAngle + i * angleStep; // 현재 탄막의 각도 계산

            // 적 오브젝트의 방향에 따라 탄막의 각도를 조정합니다.
            if (player.transform.localScale.x < 0)
            {
                angle = 180f - angle;
            }

            // 각도를 라디안으로 변환합니다.
            float angleInRadians = angle * Mathf.Deg2Rad;

            // 탄막의 위치 계산
            float x = radius * Mathf.Cos(angleInRadians);
            float y = radius * Mathf.Sin(angleInRadians);

            // 탄막 생성
            GameObject bullet = GameObject.Instantiate(damageOb, attackPivot.transform.position + new Vector3(x, y, 0f), Quaternion.identity, this.transform);
            if (bullet != null)
            {
                if (isNullParent == true)
                {
                    bullet.transform.parent = null;
                }
                dmOb = bullet.GetComponent<DamageObject>();
                dmOb.skillName = skillName;
                dmOb.SetDamge(damgeArray);

                // 탄막 방향 설정
                bullet.transform.up = new Vector2(x, y).normalized;
            }
        }
    }

    public void ActiveLeft()
    {
        if (isButtonDownSkill && skillCooldown.isCooldown[0] == false && PlayerHealthManager.Instance.nowStemina > useStemina && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack))
        {
            isActive = true;

            if (isRoundAttack == true)
            {
                RountAttack(skillAWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)));
            }
            else if (isGorundCheckSkill == false)
            {
                NomalAttack(skillAWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)));
            }
            else
            {
                if (skillAWaitTime == 0)
                {
                    GroundAttack();
                }
                else
                {
                    Sequence sequence = DOTween.Sequence()
                    .AppendInterval(skillAWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)))
                    .AppendCallback(() => GroundAttack());
                }
            }

            SkillAnimCheck("A");
        }
    }
    public void ActiveRight()
    {

        if (isButtonDownSkill && skillCooldown.isCooldown[1] == false && PlayerHealthManager.Instance.nowStemina > useStemina && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack))
        {
            isActive = true;
            if (isRoundAttack == true)
            {
                RountAttack(skillBWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)));

            }
            else if (isGorundCheckSkill == false)
            {
                NomalAttack(skillBWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)));
            }
            else
            {
                if (skillBWaitTime == 0)
                {
                    GroundAttack();
                }
                else
                {
                    Sequence sequence = DOTween.Sequence()
                    .AppendInterval(skillBWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)))
                    .AppendCallback(() => GroundAttack());
                }
            }
            SkillAnimCheck("B");
        }
    }
    public void ActiveSideLeft()
    {

        if (isButtonDownSkill && skillCooldown.isCooldown[2] == false && PlayerHealthManager.Instance.nowStemina > useStemina && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack))
        {
            isActive = true;
            if (isRoundAttack == true)
            {
                RountAttack(skillCWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)));

            }
            else if (isGorundCheckSkill == false)
            {
                NomalAttack(skillCWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)));
            }
            else
            {
                if (skillCWaitTime == 0)
                {
                    GroundAttack();
                }
                else
                {
                    Sequence sequence = DOTween.Sequence()
                    .AppendInterval(skillCWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)))
                    .AppendCallback(() => GroundAttack());
                }
            }
            SkillAnimCheck("C");
        }
    }
    public void ActiveSideRight()
    {

        if (isButtonDownSkill && skillCooldown.isCooldown[3] == false && PlayerHealthManager.Instance.nowStemina > useStemina && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack))
        {

            isActive = true;
            if (isRoundAttack == true)
            {
                RountAttack(skillDWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)));
            }
            else if (isGorundCheckSkill == false)
            {
                NomalAttack(skillDWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)));
            }
            else
            {
                if (skillDWaitTime == 0)
                {
                    GroundAttack();
                }
                else
                {
                    Sequence sequence = DOTween.Sequence()
                    .AppendInterval(skillDWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100)))
                    .AppendCallback(() => GroundAttack());
                }
            }
            SkillAnimCheck("D");

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
                // skillCooldown.UseSkillA(skillName);
                skillCooldown.UseSkill(0, skillName);
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
                //    skillCooldown.UseSkillB(skillName);
                skillCooldown.UseSkill(1, skillName);
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
                //     skillCooldown.UseSkillC(skillName);
                skillCooldown.UseSkill(2, skillName);
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
                //   skillCooldown.UseSkillD(skillName);
                skillCooldown.UseSkill(3, skillName);
                CheckAttackWait();
                PlayerController.instance.StopAttackAnim();
                dmOb.DestroyObject();
            }
        }
    }

    void GroundAttack() // 땅에서 생성되는 기술
    {
        Vector2 newDir = new Vector2(0, -1);
        Vector2 currentPosition = new Vector2(rayPositon.transform.position.x, rayPositon.transform.position.y);
        Vector2 destination = currentPosition + newDir.normalized * groundCheckLength;
        RaycastHit2D hit = Physics2D.Raycast(currentPosition, newDir, groundCheckLength, collisionLayer);

        if (hit.collider == null) // 충돌이 없으면 소환 x 아예 사용이 안됨
        {
            isActive = false;
        }
        else
        {
            // 충돌이 있는 경우, 충돌 지점 앞에 오브젝트를 이동
            Vector2 safePosition = hit.point - newDir.normalized; // 충돌 지점에서 약간 떨어진 위치
            safePosition = new Vector2(safePosition.x, safePosition.y + yPivot);
            damageObject = Instantiate(skillprefab[0], new Vector2(skillPivot[0].transform.position.x, safePosition.y), skillPivot[0].transform.rotation, this.transform);
            MasterAudio.PlaySound(skillSound[0]);

        }
    }

    void SkillEffect(string skillNum) // 기술 에니메이션 및 데미지 오브젝트에 공격설정
    {
        PlayerController.instance.ActiveAttackAnim(false,skillAnim, attckSpeed / (1 + (DatabaseManager.attackSpeedBuff / 100)));
        PlayerHealthManager.Instance.SteminaDown(useStemina);


        float waitTime = 0;

        switch (skillNum)
        {
            case "A":
                waitTime = skillAWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100));
                break;
            case "B":
                waitTime = skillBWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100));
                break;
            case "C":
                waitTime = skillCWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100));
                break;
            case "D":
                waitTime = skillDWaitTime / (1 + (DatabaseManager.attackSpeedBuff / 100));
                break;
        }

        if (delayEffectPrefab != null)
        {
            GameObject effect = Instantiate(delayEffectPrefab, new Vector2(skillPivot[0].transform.position.x, skillPivot[0].transform.position.y), skillPivot[0].transform.rotation);
            MasterAudio.PlaySound(skillSound[0]);
            DestoryByTime dBT = effect.GetComponent<DestoryByTime>();
            dBT.DestroyEffect(waitTime);
        }
        if (waitTime == 0)
        {
            SetSkillDmg();

            //GroundAttack();
        }
        else
        {
            sequenceA = DOTween.Sequence()
           .AppendInterval(waitTime) // 사전에 지정한 공격 주기만큼 대기.
           .AppendCallback(() => SetSkillDmg());
        }



    }
    void SetSkillDmg()
    {
        if(isRoundAttack == false)
        {
            if (isNullParent == true)
            {
                damageObject.transform.parent = null;
            }
            dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.skillName = skillName;
            dmOb.SetDamge(damgeArray);
            if (skillprefab.Length > 1)
            {
                StartCoroutine(SpawnSkills());
            }
        }

    }

    void NomalAttack(float waitTime) // 일반적인 공격의 생성
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
    void RountAttack(float waitTime)
    {
        if (waitTime == 0)
        {
            RoundAttack(skillprefab[0], skillPivot[0]);
            MasterAudio.PlaySound(skillSound[0]);
        }
        else
        {
            Sequence sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .AppendCallback(() => RoundAttack(skillprefab[0], skillPivot[0]));
            MasterAudio.PlaySound(skillSound[0]);
        }
    }
    void SkillAnimCheck(string skillNum) // 기술의 애니메이션 재생과 홀드 스킬의 구분
    {


        if (isActive == true)
        {
            SkillEffect(skillNum);

            if (isHoldSkill == false)
            {
                switch (skillNum)
                {
                    case "A":
                        //  skillCooldown.UseSkillA(skillName);
                        skillCooldown.UseSkill(0, skillName);
                        break;
                    case "B":
                        // skillCooldown.UseSkillB(skillName);
                        skillCooldown.UseSkill(1, skillName);
                        break;
                    case "C":
                        //skillCooldown.UseSkillC(skillName);
                        skillCooldown.UseSkill(2, skillName);
                        break;
                    case "D":
                        //  skillCooldown.UseSkillD(skillName);
                        skillCooldown.UseSkill(3, skillName);
                        break;
                }

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
                if (holdDisEffectByTime)
                {
                    waitTime = holdingTime;
                }
                else
                {
                    waitTime =holdingTime / (1 + (DatabaseManager.attackSpeedBuff / 100));
                }
                switch (skillNum)
                {
                    case "A":
                        isActiveHoldA = true;
                        sequenceA = DOTween.Sequence()
                       .AppendInterval(waitTime) // 사전에 지정한 공격 주기만큼 대기.
                       .AppendCallback(() => isActiveHoldA = false)
                                            // .AppendCallback(() => skillCooldown.UseSkillA(skillName))
                                            .AppendCallback(() => skillCooldown.UseSkill(0, skillName))
                       .AppendCallback(() => CheckAttackWait())
                       .AppendCallback(() => dmOb.DestroyObject());
                        break;
                    case "B":
                        isActiveHoldB = true;
                        sequenceB = DOTween.Sequence()
                       .AppendInterval(waitTime) // 사전에 지정한 공격 주기만큼 대기.
                       .AppendCallback(() => isActiveHoldB = false)
                                            // .AppendCallback(() => skillCooldown.UseSkillB(skillName))
                                            .AppendCallback(() => skillCooldown.UseSkill(1, skillName))
                       .AppendCallback(() => CheckAttackWait())
                       .AppendCallback(() => dmOb.DestroyObject());
                        break;
                    case "C":
                        isActiveHoldC = true;
                        sequenceC = DOTween.Sequence()
                       .AppendInterval(waitTime) // 사전에 지정한 공격 주기만큼 대기.
                       .AppendCallback(() => isActiveHoldC = false)
                                           //  .AppendCallback(() => skillCooldown.UseSkillC(skillName))
                                           .AppendCallback(() => skillCooldown.UseSkill(2, skillName))
                       .AppendCallback(() => CheckAttackWait())
                       .AppendCallback(() => dmOb.DestroyObject());
                        break;
                    case "D":
                        isActiveHoldD = true;
                        sequenceD = DOTween.Sequence()
                       .AppendInterval(waitTime) // 사전에 지정한 공격 주기만큼 대기.
                       .AppendCallback(() => isActiveHoldD = false)
                                          //   .AppendCallback(() => skillCooldown.UseSkillD(skillName))
                                          .AppendCallback(() => skillCooldown.UseSkill(3, skillName))
                       .AppendCallback(() => CheckAttackWait())
                       .AppendCallback(() => dmOb.DestroyObject());
                        break;
                }



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
            .AppendInterval(attckSpeed / (1 + (DatabaseManager.attackSpeedBuff / 100))) // 사전에 지정한 공격 주기만큼 대기.
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
