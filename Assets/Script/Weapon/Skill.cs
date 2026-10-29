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
     bool isActiveHold = false;
    private int _activeSlot = -1; // 실제로 발동된 슬롯(ActiveSkill 인자). Inspector skillNum 과 다를 수 있다 (B-113)
    private Sequence _spawnDelaySequence;  // 선딜 뒤 데미지 오브젝트 생성
    private Sequence _dmgDelaySequence;    // 선딜 뒤 데미지 설정
    private Coroutine _spawnRoutine;       // 프리팹 여러 개 순차 생성(SpawnSkills)
    /// <summary>홀드 스킬이 유지 중인지 — 유지 중에는 다른 스킬을 발동하지 않는다 (B-113)</summary>
    public bool IsHolding => isActiveHold;

    public float skillWaitTime = 0; // 딜레이시간, 이 시간이 지나고 프리팹 생성

    GameObject damageObject;
    DamageObject dmOb;
    public GameObject player;
    Sequence skillSequence;
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
    public int skillNum;
    public string skillDetail;
    public GameObject delayEffectPrefab;
    [Tooltip("스킬 시전 순간 첫 피벗에 띄울 베기 이펙트 (세트·무기 테마색)")]
    public VfxId castVfx = VfxId.None;
    public float castVfxScale = 1f;
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
        // 무기 해제·파괴 시 잠금 토큰이 남지 않도록 (C9)
        attackWaitSequence.Kill();
        ReleaseLocks();
        // 홀드·선딜 정리: SetLink 기본값은 파괴 때만 Kill 하므로 비활성화로는 안 멈춘다. 다시 장착했을 때 IsHolding 이 남지 않게 (B-113)
        skillSequence.Kill();
        _spawnDelaySequence.Kill();
        _dmgDelaySequence.Kill();
        StopSpawnRoutine(); // 컴포넌트만 꺼지는 경우에도 순차 생성을 멈춘다
        if (isActiveHold && weapon != null)
        {
            weapon.isSkillAttackWait = true; // 홀드가 내린 스킬 대기를 되돌린다(만료 콜백이 Kill 되었으므로)
        }
        isActiveHold = false;
        _activeSlot = -1;
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
        safePosition = hit.point - newDir.normalized * 0.2f; // 충돌 지점에서 약간 떨어진 위치
    }
    void GroundCheck(bool isHit, int num)
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
    private IEnumerator SpawnSkills() // 
    {
        bool isChangeDir = false;
        if (player.transform.localScale.x < 0) isChangeDir = true;
        if (DatabaseManager.skillHitCount.ContainsKey(skillName)) allObjectMaxCount = objectMaxCount + DatabaseManager.skillHitCount[skillName];
        else allObjectMaxCount = objectMaxCount;
        yield return new WaitForSeconds(interval[0] / StatMath.AttackSpeedMul);
        for (int i = 1; i < skillprefab.Length && i < skillPivot.Length && (isEffectMaxAdd == false || i < allObjectMaxCount); i++)
        {
            if (isRayCheckSkill == false) //RayCast를 확인하는 기술인가
            {
                if (isGorundCheckSkill == false) //일정 거리이상 점프시에 사용을 못하는 기술인가
                    damageObject = Instantiate(skillprefab[i], skillPivot[i].transform.position, skillPivot[i].transform.rotation, this.transform);
                else
                {
                    WallCheck(false, isChangeDir,i);
                    Vector2 safePosition = hit.point - newDir.normalized; 
                    safePosition = new Vector2(safePosition.x, safePosition.y + yPivot);
                    damageObject = Instantiate(skillprefab[i], new Vector2(skillPivot[i].transform.position.x, safePosition.y), skillPivot[i].transform.rotation, this.transform);
                }
            }
            else // ray를 사용하는 기술. 전방으로 향하는 기술로서 pivot이 하나만 있어도 된다.
            {
                WallCheck(true, isChangeDir, i);
                if (hit.collider == null)
                {
                    if (isGorundCheckSkill == false)
                        damageObject = Instantiate(skillprefab[i], new Vector2(destination.x, skillPivot[i].transform.position.y), skillPivot[i].transform.rotation, this.transform);
                    else //groundCheck를 해줘야하는경우
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
            PlaySkillSound(i);
            if( damageObject != null)
            {
                if (isNullParent == true)
                {
                    damageObject.transform.parent = null; // 바닥을 못 찾아 생성이 안 됐으면 건너뛴다(NRE 방지)
                }
                dmOb = damageObject.GetComponent<DamageObject>();
                dmOb.skillName = skillName;
                dmOb.SetDamge(damgeArray);
                dmOb.isSkill = true; // 스킬 여부는 코드가 정한다(공유 프리팹 플래그 무시, W-04)

            }
         
            // 다음 스킬을 생성하기 전에 interval[i]의 시간만큼 대기
            if (i < interval.Length)
                yield return new WaitForSeconds(interval[i] / StatMath.AttackSpeedMul);
        }
        originPoint = Vector2.zero;
        _spawnRoutine = null;
    }

    public void RoundSpawn(GameObject damageOb, GameObject attackPivot)
    {
        if (DatabaseManager.skillBulletCount.ContainsKey(skillName))
            allBullet = bulletCount + DatabaseManager.skillBulletCount[skillName]; // 아이템 중 해당 기술의 탄환 중가가 있는경우 숫자를 증가시킵니다.
        else
            allBullet = bulletCount;
        float angleStep = allBullet > 1 ? (endAngle - startAngle) / (allBullet - 1) : 0f; // 탄막 간격 계산 (1발이면 0 — W-24 NaN 방지)
        for (int i = 0; i < allBullet; i++)
        {
            float angle = startAngle + i * angleStep; // 현재 탄막의 각도 계산
            // 적 오브젝트의 방향에 따라 탄막의 각도를 조정합니다.
            if (player.transform.localScale.x < 0) angle = 180f - angle;

            // 각도를 라디안으로 변환합니다.
            float angleInRadians = angle * Mathf.Deg2Rad;

            // 탄막의 위치 계산
            float x = radius * Mathf.Cos(angleInRadians);
            float y = radius * Mathf.Sin(angleInRadians);

            // 탄막 생성
            GameObject bullet = GameObject.Instantiate(damageOb, attackPivot.transform.position + new Vector3(x, y, 0f), Quaternion.identity, this.transform);
            if (bullet != null)
            {
                if (isNullParent == true) bullet.transform.parent = null;
                dmOb = bullet.GetComponent<DamageObject>();
                dmOb.skillName = skillName;
                dmOb.SetDamge(damgeArray);
                dmOb.isSkill = true; // 스킬 여부는 코드가 정한다(공유 프리팹 플래그 무시, W-04)

                // 탄막 방향 설정
                bullet.transform.up = new Vector2(x, y).normalized;
            }
        }
    }

    /// <summary>지금 이 슬롯 스킬을 쓸 수 있는지 (쿨다운·스태미나·후딜)</summary>
    public bool CanActivate(int num)
    {
        return isButtonDownSkill && skillCooldown.isCooldown[num] == false && PlayerHealthManager.Instance.nowStemina > useStemina
            && ((weapon.isAttackWait && weapon.isSkillAttackWait) || isCancleAttack);
    }

    public void ActiveSkill(int num)
    {
        if (CanActivate(num))
        {
            isActive = true;
            _activeSlot = num;
            // 이전 시전의 오브젝트를 다시 잡지 않게 — 바닥을 못 찾아 생성이 안 되면 SetSkillDmg 가 건너뛴다
            damageObject = null;
            dmOb = null;
            if (isRoundAttack == true) RoundAttack(skillWaitTime / StatMath.AttackSpeedMul);
            else if (isGorundCheckSkill == false) NomalAttack(skillWaitTime / StatMath.AttackSpeedMul);
            else
            {
                if (skillWaitTime == 0) GroundAttack();
                else
                {
                    _spawnDelaySequence = DOTween.Sequence()
                    .AppendInterval(skillWaitTime / StatMath.AttackSpeedMul)
                    .AppendCallback(() => GroundAttack()).SetLink(gameObject);
                }
            }
            SkillAnimCheck(num);
        }
    }
   
    void OnSkillReleased(InputAction.CallbackContext context)
    {
        // 이 스킬이 실제로 발동된 슬롯의 키를 뗐을 때만 홀드 종료 (다른 스킬 키·Inspector skillNum 불일치, B-113)
        if (_activeSlot < 0 || _activeSlot >= skillAction.Length || context.action != skillAction[_activeSlot])
        {
            return;
        }
        if (isHoldSkill == true && isActiveHold == true)
        {
            EndHold();
        }
    }

    /// <summary>홀드 종료 단일 경로(키 해제·시간 만료). 선딜 중이면 아직 안 나온 생성·데미지 설정, 순차 생성 코루틴도 멈춘다 (B-113, 설계 2.4-6).
    /// 만료도 키 해제처럼 데미지 오브젝트를 즉시 파괴한다(예전 만료는 DestroyObject 의 지연 파괴). DestroyObject 는 홀드 버프 해제(BuffOff)를 위해 부른다.</summary>
    void EndHold()
    {
        isActiveHold = false;
        skillSequence.Kill();
        _spawnDelaySequence.Kill();
        _dmgDelaySequence.Kill();
        StopSpawnRoutine();
        skillCooldown.UseSkill(_activeSlot, skillName);
        CheckAttackWait();
        if (PlayerController.instance != null)
        {
            PlayerController.instance.StopAttackAnim();
        }
        if (dmOb != null)
        {
            dmOb.DestroyObject();
        }
        if (damageObject != null)
        {
            Destroy(damageObject.gameObject); // 생성된 오브젝트 삭제.
        }
        dmOb = null;
        damageObject = null;
    }

    void StopSpawnRoutine()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }
  
    void GroundAttack() // 땅에서 생성되는 기술, 단일 개체
    {
        Vector2 newDir = new Vector2(0, -1);
        Vector2 currentPosition = new Vector2(rayPositon.transform.position.x, rayPositon.transform.position.y);
        Vector2 destination = currentPosition + newDir.normalized * groundCheckLength;
        RaycastHit2D hit = Physics2D.Raycast(currentPosition, newDir, groundCheckLength, collisionLayer);
        if (hit.collider == null) // 충돌이 없으면 소환 x 아예 사용이 안됨
            isActive = false;
        else
        {
            Vector2 safePosition = hit.point - newDir.normalized; // 충돌 지점에서 약간 떨어진 위치
            safePosition = new Vector2(safePosition.x, safePosition.y + yPivot);
            damageObject = Instantiate(skillprefab[0], new Vector2(skillPivot[0].transform.position.x, safePosition.y), skillPivot[0].transform.rotation, this.transform);
            PlaySkillSound(0);
        }
    }

    void SkillEffect() // 기술 에니메이션 및 데미지 오브젝트에 공격설정
    {
        PlayerController.instance.ActiveAttackAnim(false,skillAnim, attckSpeed / StatMath.AttackSpeedMul);
        PlayerHealthManager.Instance.SteminaDown(useStemina);

        float waitTime = 0;
        waitTime = skillWaitTime / StatMath.AttackSpeedMul;
        if (skillPivot.Length > 0)
        {
            Vector3 fxPos = skillPivot[0].transform.position;
            if (VfxLibrary.IsGroundFx(castVfx) && PlayerController.instance.groundCheck != null)
            {
                fxPos.y = PlayerController.instance.groundCheck.transform.position.y + 0.15f; // 마법진·빛기둥은 발밑 높이
            }
            Vfx.Play(castVfx, fxPos, PlayerController.instance.transform.localScale.x < 0f, castVfxScale);
        }
        if (delayEffectPrefab != null)
        {
            GameObject effect = Instantiate(delayEffectPrefab, new Vector2(skillPivot[0].transform.position.x, skillPivot[0].transform.position.y), skillPivot[0].transform.rotation);
            PlaySkillSound(0);
            DestoryByTime dBT = effect.GetComponent<DestoryByTime>();
            dBT.DestroyEffect(waitTime);
        }
        if (waitTime == 0) SetSkillDmg();
        else
        {
            _dmgDelaySequence = DOTween.Sequence()
           .AppendInterval(waitTime) // 사전에 지정한 공격 주기만큼 대기.
           .AppendCallback(() => SetSkillDmg()).SetLink(gameObject);
        }

    }
    void SetSkillDmg()
    {
        if(isRoundAttack == false)
        {
            if (damageObject == null)
            {
                return; // 바닥을 못 찾아 생성되지 않음(GroundAttack) 또는 이미 해제됨
            }
            if (isNullParent == true) damageObject.transform.parent = null;
            dmOb = damageObject.GetComponent<DamageObject>();
            dmOb.skillName = skillName;
            dmOb.SetDamge(damgeArray);
                dmOb.isSkill = true; // 스킬 여부는 코드가 정한다(공유 프리팹 플래그 무시, W-04)
            if (skillprefab.Length > 1)
            {
                _spawnRoutine = StartCoroutine(SpawnSkills()); // 다수의 오브젝트를 생성하는 기술의 경우. 홀드 종료 때만 멈춘다(일반 스킬의 앞 시전은 그대로 둔다)
            }
        }

    }

    void NomalAttack(float waitTime) // 일반적인 공격의 생성
    {
        if (waitTime == 0)
        {
            damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform);
            PlaySkillSound(0);
        }
        else
        {
            _spawnDelaySequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .AppendCallback(() => damageObject = Instantiate(skillprefab[0], skillPivot[0].transform.position, skillPivot[0].transform.rotation, this.transform)).SetLink(gameObject);
            PlaySkillSound(0);
        }
    }
    void RoundAttack(float waitTime)
    {
        if (waitTime == 0)
        {
            RoundSpawn(skillprefab[0], skillPivot[0]);
            PlaySkillSound(0);
        }
        else
        {
            _spawnDelaySequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .AppendCallback(() => RoundSpawn(skillprefab[0], skillPivot[0])).SetLink(gameObject);
            PlaySkillSound(0);
        }
    }
    void SkillAnimCheck(int skillNum) // 기술의 애니메이션 재생과 홀드 스킬의 구분
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
                    AcquireLocks(false); // 홀드 동안 이동정지 (홀드 종료 CheckAttackWait에서 해제)
                    rb.linearVelocity = Vector2.zero;
                }

                float waitTime =0;
                if (holdDisEffectByTime) waitTime = holdingTime;
                else waitTime = holdingTime / StatMath.AttackSpeedMul;
                isActiveHold = true;
                weapon.isSkillAttackWait = false; // 홀드 중 평타 금지 — 종료 시 CheckAttackWait 가 되돌린다(W-06)
                skillSequence = DOTween.Sequence()
               .AppendInterval(waitTime) // 사전에 지정한 공격 주기만큼 대기.
               .AppendCallback(() => { if (isActiveHold) { EndHold(); } }).SetLink(gameObject); // 시간 만료도 키 해제와 같은 정리 (B-113, W-18)
            }
        }
    }

    System.IDisposable moveLockToken, ladderLockToken;
    Sequence attackWaitSequence;

    void AcquireLocks(bool ladder)
    {
        if (isWeaponStopMove && moveLockToken == null)
        {
            moveLockToken = PlayerStatus.Acquire(StatusFlag.MoveLock, "Skill:" + skillName);
        }
        if (ladder && ladderLockToken == null)
        {
            ladderLockToken = PlayerStatus.Acquire(StatusFlag.LadderLock, "Skill:" + skillName);
        }
    }

    void ReleaseLocks()
    {
        moveLockToken?.Dispose();
        moveLockToken = null;
        ladderLockToken?.Dispose();
        ladderLockToken = null;
    }

    /// <summary>스킬 후딜: 자기 토큰으로 이동정지·사다리 잠금. 홀드 스킬은 종료 시점에 호출되어 잠금을 푼다 (B-114 C4~C7)</summary>
    private void CheckAttackWait()
    {
        attackWaitSequence.Kill();
        ReleaseLocks();
        if (isHoldSkill == false)
        {
            AcquireLocks(true);
            if (isWeaponStopMove == true)
            {
                rb.linearVelocity = Vector2.zero;
            }
            weapon.isSkillAttackWait = false;
            attackWaitSequence = DOTween.Sequence()
            .AppendInterval(attckSpeed / StatMath.AttackSpeedMul) // 사전에 지정한 공격 주기만큼 대기.
            .AppendCallback(() => { ReleaseLocks(); weapon.isSkillAttackWait = true; }).SetLink(gameObject);
        }
        else
        {
            weapon.isSkillAttackWait = true;
        }
    }

    /// <summary>스킬 사운드 재생. 구버전 프리팹은 배열이 비어 있어 예외로 쿨다운·스태미나 처리가 건너뛰어졌다(W-01)</summary>
    void PlaySkillSound(int i)
    {
        if (skillSound != null && i >= 0 && i < skillSound.Length && !string.IsNullOrEmpty(skillSound[i]))
        {
            MasterAudio.PlaySound(skillSound[i]);
        }
    }
}
