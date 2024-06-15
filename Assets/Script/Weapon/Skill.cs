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


    public bool isGorundCheckSkill = false; // 땅에서 생성되는 기술.
    [ConditionalHide("isGorundCheckSkill")]
    public float groundCheckLength;
    [ConditionalHide("isGorundCheckSkill")]
    public LayerMask groundCollisionLayer; // 충돌 체크를 위한 레이어
    [ConditionalHide("isGorundCheckSkill")]
    public float yPivot; // 충돌 체크를 위한 레이어


    public bool isRoundAttack; // 원형의공격, 특정 각도내에 전채로 뿌리는 공격형식
    [ConditionalHide("isRoundAttack")]
    public float radius = 5f;       // 원의 반지름
    [ConditionalHide("isRoundAttack")]
    public float startAngle = 0f;   // 시작 각도
    [ConditionalHide("isRoundAttack")]
    public float endAngle = 360f;   // 끝 각도
    [ConditionalHide("isRoundAttack")]
    public int bulletCount = 30;    // 생성할 탄막 개수


    public GameObject delayEffectPrefab;
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

    public void ActiveMainSkill()
    {
        if (isLeft)
        {
            skillCooldown.cooldownTimeA = SkillCoolTime;
            skillCooldown.cooldownImageA.sprite = skillImage;
            skillCooldown.UseSkillA();
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
        yield return new WaitForSeconds(interval[0] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
        // 첫 번째 요소는 건너뛰고 두 번째 요소부터 생성
        for (int i = 1; i < skillprefab.Length && i < skillPivot.Length && (isEffectMaxAdd == false || i < objectMaxCount); i++)
        {
            if (isRayCheckSkill == false)
            {
                if (isGorundCheckSkill == false)
                {
                    // 스킬 프리팹을 피벗 위치에 인스턴스화
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

                    if (hit.collider == null) // 충돌이 없으면 소환 x 아예 사용이 안됨
                    {
                    }
                    else
                    {
                        // 충돌이 있는 경우, 충돌 지점 앞에 오브젝트를 이동
                        Vector2 safePosition = hit.point - newDir.normalized; // 충돌 지점에서 약간 떨어진 위치
                        safePosition = new Vector2(safePosition.x, safePosition.y + yPivot);
                        damageObject = Instantiate(skillprefab[i], new Vector2(skillPivot[i].transform.position.x, safePosition.y), skillPivot[i].transform.rotation, this.transform);
                        if (isNullParent == true)
                        {
                            damageObject.transform.parent = null;
                        }

                    }
                }

            }
            else // ray를 사용하는 기술. 전방으로 향하는 기술로서 pivot이 하나만 있어도 된다.
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
                        if (hit2.collider != null) // 충돌이 없으면 소환 x 아예 사용이 안됨
                        {                            // 충돌이 있는 경우, 충돌 지점 앞에 오브젝트를 이동
                            Vector2 safePosition2 = hit2.point - newDir2.normalized; // 충돌 지점에서 약간 떨어진 위치
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
                        // 충돌이 있는 경우, 충돌 지점 앞에 오브젝트를 이동
                        Vector2 safePosition = hit.point - newDir.normalized * 0.2f; // 충돌 지점에서 약간 떨어진 위치
                        safePosition = new Vector2(safePosition.x, safePosition.y);
                        damageObject = Instantiate(skillprefab[i], new Vector2(safePosition.x, skillPivot[i].transform.position.y), skillPivot[i].transform.rotation, this.transform);
                        if (isNullParent == true)
                        {
                            damageObject.transform.parent = null;
                        }
                    }
                    else
                    {                        // 충돌이 있는 경우, 충돌 지점 앞에 오브젝트를 이동
                        Vector2 safePosition = hit.point - newDir.normalized * 0.2f; // 충돌 지점에서 약간 떨어진 위치
                        safePosition = new Vector2(safePosition.x, safePosition.y);
                        Vector2 newDir2 = new Vector2(0, -1);
                        Vector2 currentPosition2 = new Vector2(safePosition.x, rayPositon.transform.position.y);
                        RaycastHit2D hit2 = Physics2D.Raycast(currentPosition2, newDir2, groundCheckLength, collisionLayer);
                        if (hit2.collider != null) // 충돌이 없으면 소환 x 아예 사용이 안됨
                        {                            // 충돌이 있는 경우, 충돌 지점 앞에 오브젝트를 이동
                            Vector2 safePosition2 = hit2.point - newDir2.normalized; // 충돌 지점에서 약간 떨어진 위치
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
            // 다음 스킬을 생성하기 전에 interval[i]의 시간만큼 대기
            if (i < interval.Length)
            {
                yield return new WaitForSeconds(interval[i] / (1 + (DatabaseManager.attackSpeedBuff / 100)));
            }
        }
    }

    public void RoundAttack(GameObject damageOb, GameObject attackPivot)
    {
        float angleStep = (endAngle - startAngle) / (bulletCount - 1); // 탄막 간격 계산

        for (int i = 0; i < bulletCount; i++)
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
                dmOb.SetDamge(damgeArray);

                // 탄막 방향 설정
                bullet.transform.up = new Vector2(x, y).normalized;
            }
        }
    }
    bool isActive = true;
    public void ActiveLeft()
    {
        if (isButtonDownSkill && skillCooldown.isCooldownA == false && PlayerHealthManager.Instance.nowStemina > useStemina && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack))
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

        if (isButtonDownSkill && skillCooldown.isCooldownB == false && PlayerHealthManager.Instance.nowStemina > useStemina && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack))
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

        if (isButtonDownSkill && skillCooldown.isCooldownC == false && PlayerHealthManager.Instance.nowStemina > useStemina && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack))
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

        if (isButtonDownSkill && skillCooldown.isCooldownD == false && PlayerHealthManager.Instance.nowStemina > useStemina && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack))
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
                skillCooldown.UseSkillA();
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
    }

    void NomalAttack(float waitTime) // 일반적인 공격의 생성
    {
        if (waitTime == 0)
        {
            damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform);
        }
        else
        {
            Sequence sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .AppendCallback(() => damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform));
        }
    }
    void RountAttack(float waitTime)
    {
        if (waitTime == 0)
        {
            RoundAttack(skillprefab[0], skillPivot[0]);
        }
        else
        {
            Sequence sequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .AppendCallback(() => RoundAttack(skillprefab[0], skillPivot[0]));
        }
    }
    void SkillAnimCheck(string skillNum) // 기술의 애니메이션 재생과 홀드 스킬의 구분
    {
        if (isActive == true)
        {
            if(isRoundAttack == false)
            {
                SkillEffect(skillNum);
            }

            if (isHoldSkill == false)
            {
                switch (skillNum)
                {
                    case "A":
                        skillCooldown.UseSkillA();
                        break;
                    case "B":
                        skillCooldown.UseSkillB();
                        break;
                    case "C":
                        skillCooldown.UseSkillC();
                        break;
                    case "D":
                        skillCooldown.UseSkillD();
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
                       .AppendCallback(() => skillCooldown.UseSkillA())
                       .AppendCallback(() => CheckAttackWait())
                       .AppendCallback(() => dmOb.DestroyObject());
                        break;
                    case "B":
                        isActiveHoldB = true;
                        sequenceB = DOTween.Sequence()
                       .AppendInterval(waitTime) // 사전에 지정한 공격 주기만큼 대기.
                       .AppendCallback(() => isActiveHoldB = false)
                       .AppendCallback(() => skillCooldown.UseSkillB())
                       .AppendCallback(() => CheckAttackWait())
                       .AppendCallback(() => dmOb.DestroyObject());
                        break;
                    case "C":
                        isActiveHoldC = true;
                        sequenceC = DOTween.Sequence()
                       .AppendInterval(waitTime) // 사전에 지정한 공격 주기만큼 대기.
                       .AppendCallback(() => isActiveHoldC = false)
                       .AppendCallback(() => skillCooldown.UseSkillC())
                       .AppendCallback(() => CheckAttackWait())
                       .AppendCallback(() => dmOb.DestroyObject());
                        break;
                    case "D":
                        isActiveHoldD = true;
                        sequenceD = DOTween.Sequence()
                       .AppendInterval(waitTime) // 사전에 지정한 공격 주기만큼 대기.
                       .AppendCallback(() => isActiveHoldD = false)
                       .AppendCallback(() => skillCooldown.UseSkillD())
                       .AppendCallback(() => CheckAttackWait())
                       .AppendCallback(() => dmOb.DestroyObject());
                        break;
                }



            }
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
