using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using AnyPortrait;
using DarkTonic.MasterAudio;
using Com.LuisPedroFonseca.ProCamera2D;
public class PlayerController : MonoBehaviour
{
    public ProCamera2D proCamera;
    int LadderLayer = 31;
    float chInRommSize = 1.5f;
    public float runSpeed = 10f;  // 이동 속도
    public int runStemina; // 달리기시 사용하는 스테미나
    public float intervalRunStemina; // 달리기시 사용하는 스테미나
    public float moveSpeed = 10f;  // 이동 속도
    public float jumpForce = 13f; // 점프 힘
    public float dashSpeed = 20f; // 대쉬 속도
    public float dashDuration = 0.2f; // 대쉬 지속 시간
    public float dashWait = 2f; // 대쉬 타이머
    private float dashTimer; // 대쉬 타이머
    public int dashStemina; // 대쉬시 사용하는 스테미나
    public int maxJumps = 2;      // 최대 점프 횟수
    private MovementUnlocks _movementUnlocks;
    private float _airLaunchDirection;
    private int AvailableJumps => _movementUnlocks.JumpLimit(maxJumps);
    private bool MovementGrounded => rb != null && rb.linearVelocity.y <= 0.01f &&
        ((isOnGround && isPlafromCheck && ((hit1.collider != null && IsGrounded(hit1.normal)) || (hit2.collider != null && IsGrounded(hit2.normal))))
         || TouchingFloor());

    // 계단 모서리처럼 발 감지 레이 두 개가 다 빗나가도 몸 충돌 박스가 바닥(위를 향한 면)에 얹혀 있으면 땅으로 본다.
    // 레이만 보면 공중 판정 → 공중 방향 잠금(반대 입력 0)·점프 소진으로 아무 조작도 안 됐다(11-01 사용자 "벽에 닿으면 반대로도 못 움직임")
    private static ContactFilter2D s_floorFilter;
    private static bool s_floorFilterReady;
    private bool TouchingFloor()
    {
        if (!s_floorFilterReady)
        {
            s_floorFilter = new ContactFilter2D();
            s_floorFilter.SetLayerMask(LayerMask.GetMask("Ground"));
            s_floorFilter.SetNormalAngle(60f, 120f);
            s_floorFilterReady = true;
        }
        return rb.IsTouching(s_floorFilter);
    }
    /// <summary>치트 Unstuck 용: 조작이 막혔을 때 원인을 남길 상태 한 줄</summary>
    public string DebugStateLine()
    {
        return $"states={states} attacked={isAttacked} attackedUp={isAttackedUp} attackAnim={isAttackAnim} jumpAnim={isJumpAnim} " +
               $"ladder={isLadder}/{isUpLadder} onGround={isOnGround} plat={isPlafromCheck} rays={hit1.collider != null}/{hit2.collider != null} " +
               $"floor={(rb != null && TouchingFloor())} grounded={MovementGrounded} coyote={coyote.InCoyote(Time.time)} jumps={jumpsRemaining} " +
               $"hIn={horizontalInput} airDir={_airLaunchDirection} vel={(rb != null ? rb.linearVelocity : Vector2.zero)} trigger={(bc != null && bc.isTrigger)} " +
               $"wall={isWall} pos={transform.position}";
    }

    /// <summary>치트 Unstuck 용: 조작을 막는 플래그를 전부 내리고 조금 띄운다</summary>
    public void DebugUnstuck()
    {
        isAttacked = false;
        isAttackedUp = false;
        isAttackAnim = false;
        isJumpAnim = false;
        isWall = false;
        states = "";
        _airLaunchDirection = 0f;
        jumpsRemaining = AvailableJumps;
        if (rb != null)
        {
            rb.gravityScale = 3f;
            rb.linearVelocity = Vector2.zero;
            rb.position += Vector2.up * 0.3f;
        }
    }
    private bool OnMovementGround => isUpLadder || MovementGrounded || coyote.InCoyote(Time.time);
    public bool CanMovementJump => (jumpsRemaining > 0 && (AvailableJumps > 1 || OnMovementGround)) || coyote.InCoyote(Time.time);
    public bool CanMovementDash => _movementUnlocks.CanDash(MovementGrounded);
    public int MovementJumpLimit => AvailableJumps;
    public int jumpsRemaining;    // 남은 점프 횟수
    private bool isChangeDirection = false;    // 남은 점프 횟수
    private Rigidbody2D rb;
    public bool isGround;
    private bool isRun = false;
    public bool isWall = false;
    private bool isWallReset = false;
    Vector2 moveVelocity = Vector2.zero;
    float horizontalInput;

    public GameObject rightWallCheckPoint;
    public GameObject WallCheckPoint;

    KeyAction action;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction dashAction;
    InputAction runAction;
    Sequence waitSequence;

    InputAction verticalAction;
    InputAction  upAction;
    InputAction downAction;
    public string states = "";

    // 애니메이션
    public Transform characterGroup;
    public apPortrait mainCharacter;

    CharacterRig _rig;
    /// <summary>활성 캐릭터 리그 — 픽셀 리그가 붙으면 그것, 아니면 AnyPortrait 래퍼 (PixelRigDesign.md 3절)</summary>
    public CharacterRig Rig
    {
        get
        {
            if (_rig == null)
            {
                _rig = PortraitRig.For(mainCharacter);
            }
            return _rig;
        }
    }

    public void UseRig(CharacterRig rig)
    {
        if (rig != null)
        {
            _rig = rig;
        }
    }
    public Texture2D CharBaseSuit2;


    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        dashAction.Enable();
        runAction.Enable();
        verticalAction.Enable();
        upAction.Enable();
        downAction.Enable();
    }
    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        dashAction.Disable();
        runAction.Disable();
        verticalAction.Disable();
        upAction.Disable();
        downAction.Disable();
    }
    private void Awake()
    {
        _movementUnlocks = GetComponent<MovementUnlocks>();
        if (_movementUnlocks == null)
        {
            _movementUnlocks = gameObject.AddComponent<MovementUnlocks>();
        }
        action = new KeyAction();
        moveAction = action.Player.Move;
        jumpAction = action.Player.Jump;
        dashAction = action.Player.Dash;
        runAction = action.Player.Run;
        verticalAction = action.UI.verticalCheck;
        upAction = action.UI.UPInventory;
        downAction = action.UI.DownInventory;
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
    }

    static public PlayerController instance;
    public bool boxColliderTrue = false;
  public  bool platformTrue = false; 
 
    BoxCollider2D bc;
    public bool isAttacked;
    /// <summary>점프 키를 누르고 있는가 (활공 GlideMotor 가 읽는다)</summary>
    public bool JumpHeld { get { return jumpAction != null && jumpAction.IsPressed(); } }
    public bool isAttackedUp;
    void Start()
    {
        this.gameObject.transform.position = SaveManager.instance.datas.playerPos;
        proCamera.CenterOnTargets();
        bc = this.GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        _attackManager = GetComponent<AttackManager>();
        jumpsRemaining = AvailableJumps;
    }
    bool once = false;
    public GameObject touchLight;
    public GameObject nowTouch;
    [HideInInspector] public bool uiDashWait = false; // 미사용(직렬화 호환용) — 제작창 닫기 직후 대시 방지는 DashLock 토큰(PlayerStateDesign Phase 4)
    /// <summary>UI 를 닫은 직후 같은 키 입력으로 대시가 나가지 않게 0.1초 DashLock (Tick 에서 만료)</summary>
    public void DashWait2F()
    {
        PlayerStatus.AcquireFor(StatusFlag.DashLock, 0.1f, "UiClose");
    }

    public void TouchLightOn(float attP, float lightMaxT, float lightT)
    {
        if(nowTouch != null)
        {
            Destroy(nowTouch.gameObject);
        }

        GameObject touch = Instantiate(touchLight, this.transform.position, Quaternion.identity, this.transform);
        nowTouch = touch;
        TouchLightManager touchLightManager = touch.GetComponent<TouchLightManager>();
        touchLightManager.StartLighting(attP, lightMaxT, lightT);
    }

    /// <summary>
    /// 매 프레임 호출되는 대기 애니메이션용: 이미 재생 중이면 다시 Play하지 않는다
    /// (AnyPortrait Play 재호출이 매 프레임 GC 할당·애니 리셋을 일으키던 문제, C10).
    /// </summary>
    void PlayLoopAnim(string clip, float fade = 0.12f)
    {
        if (Rig.IsPlaying(clip))
        {
            return;
        }
        // 대기·걷기·달리기 사이는 짧게 크로스페이드해서 툭툭 끊기지 않게 한다
        Rig.CrossFade(clip, fade);
    }

    // ── 공격자 히트스톱: 적중 순간 플레이어 애니를 잠깐 멈춘다 (HitFeedback에서 호출) ──
    float hitStopTimer;
    float lastHitStopAt = -1f;

    public void HitStop(float seconds)
    {
        if (seconds <= 0f || Rig == null || Time.time - lastHitStopAt < 0.08f)
        {
            return; // 다단히트가 손을 계속 멈추지 않도록 0.08초에 1번
        }
        lastHitStopAt = Time.time;
        if (hitStopTimer <= 0f)
        {
            Rig.SetAnimationSpeed(0f);
        }
        hitStopTimer = Mathf.Max(hitStopTimer, seconds);
    }

    void TickHitStop()
    {
        if (hitStopTimer <= 0f)
        {
            return;
        }
        hitStopTimer -= Time.deltaTime;
        if (hitStopTimer <= 0f && Rig != null)
        {
            RestoreAnimSpeed();
        }
    }

    [Tooltip("방향 잠금을 끈 공격(무기·스킬 옵션)만 쓰는 값: 공격 시작 후 방향 전환을 막는 시간(초) (W-13)")]
    [SerializeField] private float _turnLockTime = 0.2f;
    float attackStartedAt = -10f;
    // 던파식 방향 잠금(10-30): 기본은 공격 모션 내내 시작 방향을 본다. 좌우 입력은 이동만 하고 모션이 끝나면 입력 방향으로 돌아온다
    bool attackFacingLocked = true;
    // 평타 중 이동 가능한 무기(녹슨 검 등)는 후딜 동안 무기별 배율로 느리게 걷는다
    private float AttackMoveMul => Weapon.Equipped != null ? Weapon.Equipped.AttackMoveSpeedMul : 1f;
    // 후딜 전진(Attack Drift) 중에는 달리기여도 걷기 속도 기준으로 배율을 곱한다
    bool RunNow => isRun && !(Weapon.Equipped != null && Weapon.Equipped.OwnsMoveLockInRecovery);

    /// <summary>이동정지 무기의 평타 후딜 중 조작 편의(CombatFeelPlan CF-1·2·3·5). 점프·이동 캔슬이면 후딜을 끊고, 반대키는 판정 뒤 몸을 돌리고,
    /// 지상 앞키는 true(정지 대신 전진 허용)를 돌려준다. 스킬·UI 잠금이 겹치면 아무것도 안 한다</summary>
    bool UpdateAttackRecoveryMove(Weapon w)
    {
        if (w == null || !w.OwnsMoveLockInRecovery || moveAction == null || states == "dash" || states == "wallJump" || isUpLadder)
        {
            return false;
        }
        float h = moveAction.ReadValue<float>();
        float facing = transform.localScale.x < 0f ? -1f : 1f;
        bool back = h * facing < 0f;
        bool wantJump = jumpAction.triggered || jumpBuffer.Peek(Time.time);
        if (wantJump && verticalAction.ReadValue<float>() >= -0.5f && !isWall && CanMovementJump && w.CanJumpCancelNow)
        {
            EndRecoveryForMove(w);
            return false;
        }
        if ((back || (runAction.triggered && h != 0f)) && w.CanMoveCancelNow)
        {
            EndRecoveryForMove(w);
            return false;
        }
        if (back && w.CanTurnNow)
        {
            transform.localScale = new Vector3(h < 0f ? -chInRommSize : chInRommSize, chInRommSize, 1); // 모션은 유지, 이동만 새 방향
            facing = -facing;
        }
        return OnMovementGround && w.CanDriftNow(h, facing);
    }

    void EndRecoveryForMove(Weapon w)
    {
        StopAttackAnim();
        w.CancelMeleeRecovery();
        attackFacingLocked = false;
    }

    bool TurnLocked => isAttackAnim && (attackFacingLocked || Time.time - attackStartedAt < _turnLockTime);

    /// <summary>공격·스킬 시작 직전에 부른다: 지금 좌우 입력이 있으면 그 방향으로 돌아선다(잠금 중이라도 새 공격은 입력 방향으로 시작, 10-30).
    /// 입력이 없으면 현재 방향 그대로. 대시·벽점프 중에는 돌지 않는다</summary>
    public void FaceInputForAttack()
    {
        if (moveAction == null || states == "dash" || states == "wallJump")
        {
            return;
        }
        float h = moveAction.ReadValue<float>();
        if (h < 0f)
        {
            transform.localScale = new Vector3(-chInRommSize, chInRommSize, 1);
        }
        else if (h > 0f)
        {
            transform.localScale = new Vector3(chInRommSize, chInRommSize, 1);
        }
    }

    string attackAnimPlaying; // 공속 배율이 걸린 공격 애니 (히트스톱 해제 후 배율 복원용, W-11)

    /// <summary>히트스톱 해제: 전체 1배속으로 돌린 뒤 진행 중인 공격 애니에는 공속 배율을 다시 건다</summary>
    void RestoreAnimSpeed()
    {
        Rig.SetAnimationSpeed(1f);
        if (isAttackAnim && !string.IsNullOrEmpty(attackAnimPlaying))
        {
            Rig.SetAnimationSpeed(attackAnimPlaying, StatMath.AttackSpeedMul);
        }
    }

    /// <summary>공격이 공중에서 끝나면 Jump 애니로 돌아가 착지 판정이 Idle 로 이어지게 한다 (W-12)</summary>
    void ReturnToAirAnim()
    {
        attackAnimPlaying = null;
        // isGround 는 벽 판정용이라 지면에서도 false 일 수 있다 → 착지 판정(세로 속도 0)과 같은 기준을 쓴다
        if (Mathf.Abs(rb.linearVelocity.y) > 0.01f && !isUpLadder && states != "dash")
        {
            isJumpAnim = true;
            Rig.Play("Jump");
        }
    }

    /// <summary>착지 스쿼시: 메시(자식)만 살짝 눌렀다 편다 — 루트 스케일은 방향·충돌에 쓰이므로 건드리지 않음</summary>
    void LandSquash()
    {
        if (Rig == null || Rig.transform == transform)
        {
            return;
        }
        Transform mesh = Rig.transform;
        mesh.DOKill(true);
        mesh.DOPunchScale(new Vector3(0.08f, -0.12f, 0f), 0.18f, 6, 0.6f);
    }

    void Update()
    {
        PlayerStatus.Tick(); // 시간 만료 토큰(대시 무적 등) 해제
        TickHitStop();
       if (verticalInput == 0 && states != "move" && isUpLadder ==true && isAttackAnim == false)
        {
            PlayLoopAnim("RopeStay");

        }
        if (horizontalInput == 0&& isUpLadder == false)
        {
            if (isJumpAnim == false && isUpLadder == false && states != "dash" && isAttackAnim == false)
            {
                PlayLoopAnim("Idle");
            }
            if (isJumpAnim == false && isUpLadder == true && states != "dash" && isAttackAnim == false)
            {
                PlayLoopAnim("RopeStay");
            }

        }
        if (isJumpAnim == false && isUpLadder == false && states != "dash")
        {
            rb.gravityScale = 3f;

        }
        if (rb.linearVelocity.y ==0 && isJumpAnim == true && states != "dash")
        {
            isJumpAnim = false;
        }

            if (DatabaseManager.isOpenUI == false && isAttacked == false && isAttackedUp == true)
        {
            isAttackedUp = false;
            if (rb.linearVelocity != Vector2.zero )
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

            if (boxColliderTrue == false && platformTrue == false)
        {
            bc.isTrigger = false;
        }
        else
        {
            bc.isTrigger = true;
        }

        if (DatabaseManager.isOpenUI == true && rb.linearVelocity != Vector2.zero && once == false)
        {
            once = true;
            rb.linearVelocity = Vector2.zero;
        }
        if(DatabaseManager.isOpenUI == false && isAttacked == false)
        {
            once = false;
            bool driftBypass = UpdateAttackRecoveryMove(Weapon.Equipped); // CF-1·2·3·5: 후딜 중 전진·캔슬·방향 전환 (캔슬되면 잠금이 풀린다)
            bool moveBlocked = DatabaseManager.weaponStopMove == true && !driftBypass;
            // 공격 이동정지 중에도 대시는 움직인다(대시가 MoveLock을 끄지 않고 무시, B-114 C5)
            // 가로 속도만 멈춘다. y 까지 0 으로 만들면 공중 평타 연타로 떨어지지 않는다(W-08)
            if (moveBlocked && states != "dash" && !isUpLadder && !MovementGrounded)
            {
                // CF-6: 공중 평타는 가로 관성 유지(입력 없으면 0.15초 내 감속, 입력 시 이동속도 x0.7)
                float airVx = CombatRules.AirAttackVx(rb.linearVelocity.x, moveAction.ReadValue<float>(), moveSpeed * StatMath.MoveSpeedMul, Time.deltaTime);
                if (airVx != rb.linearVelocity.x)
                {
                    rb.linearVelocity = new Vector2(airVx, rb.linearVelocity.y);
                }
            }
            else if (rb.linearVelocity.x != 0f && moveBlocked && states != "dash")
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            UpdateJumpCut();

            if (runAction.triggered && isRun == false && DatabaseManager.weaponStopMove == false && Mathf.Abs(horizontalInput) > 0 )
            {
                isRun = true;
                runSteminaDown();
            }
            else if ((runAction.triggered && isRun == true) && DatabaseManager.weaponStopMove == false)
            {

                isRun = false;
            }
            // 이동
            if (!moveBlocked && isUpLadder == false&& states != "dash")
            {

                Move();
            }


            // 대쉬 (CF-10: 입력을 0.1초 버퍼링)
            if (dashAction.triggered)
            {
                dashBuffer.Press(Time.time);
            }
            if (DatabaseManager.isOpenUI == false && !PlayerStatus.Has(StatusFlag.DashLock) && dashBuffer.Peek(Time.time) && CanMovementDash && dashTimer <= 0f && PlayerHealthManager.Instance.nowStemina > dashStemina)//&& DatabaseManager.weaponStopMove == false
            {
                dashBuffer.Consume(Time.time);
                Dash();
            }



            // 점프
            // 점프 입력은 0.1초 버퍼링: 착지 직전·후딜 끝나기 직전에 눌러도 가능해지는 순간 점프한다
            if (jumpAction.triggered)
            {
                jumpBuffer.Press(Time.time);
            }
            if (jumpAction.triggered && DatabaseManager.weaponStopMove == false && verticalInput < -0.5f && currentOneWayPlatform != null)
            {
                jumpBuffer.Clear();
                DisableCollision(); // 하향 점프는 누른 순간에만
            }
            else if (jumpBuffer.Peek(Time.time) && DatabaseManager.weaponStopMove == false && isWall == false && states != "dash")
            {
                // 코요테: 발판에서 걸어 내려간 직후에는 지상 점프로 인정
                if (CanMovementJump)
                {
                    jumpBuffer.Consume(Time.time);
                    if (jumpsRemaining <= 0)
                    {
                        jumpsRemaining = 1;
                    }
                    Jump();
                }
            }

            // 대쉬 타이머 업데이트
            if (dashTimer > 0f)
            {
                dashTimer -= Time.deltaTime;
            }
        }

        if (states != "dash" && states != "wallJump" && isLadder == true && DatabaseManager.checkAttackLadder == false && DatabaseManager.isOpenUI == false)
        {
            if(upAction.triggered == true|| downAction.triggered)
            {

                jumpsRemaining = AvailableJumps;
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0;
                isUpLadder = true;

                // 로프 위에 고정시키기
                Vector2 newPosition = new Vector2(nowLadder.transform.position.x, transform.position.y);
                
                // 로프는 몸 뒤에 둔다(LadderLayer 그대로) — 오르기 셀이 뒷모습이라 가슴 쪽에 로프가 있어야 한다. 예전 측면 오르기 때처럼
                // 로프를 플레이어 앞(+1)으로 올리면 등·머리 위로 로프가 지나가 로프 뒤에 매달린 것처럼 보였다(10-30 인게임 캡처)

                // 현재 위치와 로프 위치 사이의 거리 계산
                Vector2 distance = newPosition - (Vector2)transform.position;
                BoxCollider2D bc = this.GetComponent<BoxCollider2D>();
                boxColliderTrue = true;
                // Rigidbody 이동으로 순간 이동 방지
                rb.MovePosition(rb.position + distance);
            }


        }

        if(isUpLadder == true)
        {
            LadderMove();

            if (currentOneWayPlatform != null)
            {
              //  DisableCollision();
            }
        }


        }
    bool ladderJumpCheck = false;
    GameObject nowLadder;

    void LadderMove()
    {
        moveVelocity = Vector2.zero;

        if (states != "dash" && states != "wallJump"&& DatabaseManager.isOpenUI == false)
        {
            horizontalInput = moveAction.ReadValue<float>();
            verticalInput = verticalAction.ReadValue<float>();
            if (verticalInput < -0.2f && isAttackAnim == false)
            {
                states = "moveDown";
                PlayRopeSound();
                Rig.Play("RopeDown");
                moveVelocity = Vector2.down * (isRun ? runSpeed * StatMath.MoveSpeedMul : moveSpeed * StatMath.MoveSpeedMul);
            }
            else if (verticalInput > 0.2f && isAttackAnim == false)
            {
                PlayRopeSound();
                states = "moveUp";
                Rig.Play("RopeUp");
                moveVelocity = Vector2.up * (isRun ? runSpeed * StatMath.MoveSpeedMul : moveSpeed * StatMath.MoveSpeedMul);
            }
            else if (verticalInput == 0 && states != "move" && check == false && isAttackAnim == false)
            {
                Rig.Play("RopeStay");
                check = true;
                Sequence sequence = DOTween.Sequence()
                    .AppendInterval(0.3f)
                    .AppendCallback(() => VerticalRunChecker());

            }
            // Applying velocity to the Rigidbody2D
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, moveVelocity.y);


            if (horizontalInput < 0 && !TurnLocked) // 로프 공격 중에도 방향 고정 (10-30)
            {
                transform.localScale = new Vector3(-chInRommSize, chInRommSize, 1);
            }
            else if (horizontalInput > 0 && !TurnLocked)
            {
                transform.localScale = new Vector3(chInRommSize, chInRommSize, 1);
            }
        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {


        if (collision.gameObject.CompareTag("Ground"))
        {
            BaseGround baseGround = collision.gameObject.GetComponent<BaseGround>();

            if(baseGround == null)
            {
                currentOneWayPlatform = collision.gameObject;
            }


        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
           // currentOneWayPlatform = null;
        }
    }

    BoxCollider2D lastPlatform;
    BoxCollider2D platformCollider;
    private void DisableCollision()
    {
         platformCollider = currentOneWayPlatform.GetComponent<BoxCollider2D>();
        isJumpAnim = true;
        if ( isAttackAnim == false)
        {
        //    Rig.Play("Jump");
        }


        Physics2D.IgnoreCollision(playerCollider, platformCollider);
        isDownJump = true;
    }

    bool isDownJump;
    public void AbleCollision()
    {
        if(platformCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
            isDownJump = false;
        }

    }
    private void runSteminaDown()
    {
        if (PlayerHealthManager.Instance.nowStemina > runStemina)
        {
            PlayerHealthManager.Instance.SteminaDown(runStemina);
        }
        else
        {
            isRun = false;
        }

        if (isRun == true)
        {
            waitSequence = DOTween.Sequence()
                .AppendInterval(intervalRunStemina)
                .OnComplete(() => runSteminaDown());
        }
    }
    float verticalInput;
    /// <summary>↓ 를 누른 채 공격 중인가 — 다운된 적에게 다운 공격(피해 x1.3, CombatSpec 9절). 새 키 없이 기존 ↓ 입력을 쓴다</summary>
    public bool IsDownAttackHeld => isAttackAnim && verticalAction != null && verticalAction.ReadValue<float>() < -0.5f;
    Sequence moveSequence;
    void Move()
    {
        moveVelocity = Vector2.zero;


        if (states != "dash" && states != "wallJump")
        {
            horizontalInput = moveAction.ReadValue<float>();
            if (OnMovementGround)
            {
                _airLaunchDirection = horizontalInput;
            }
            else
            {
                if (_airLaunchDirection == 0f) { _airLaunchDirection = horizontalInput; }
                horizontalInput = MovementUnlockRules.AirInput(horizontalInput, _airLaunchDirection, true, _movementUnlocks.AirTurn);
            }
            verticalInput = verticalAction.ReadValue<float>();
            if (horizontalInput < 0)
            {
              if (isJumpAnim == false&& isAttackAnim == false)
                    {
                        if (isRun)
                    {
                        PlayRunSound();
                        PlayLoopAnim("Run");

                        }
                        else
                    {

                        PlayWalkSound();
                        PlayLoopAnim("Walk");
                        }
                    }


                states = "moveLeft";
                moveVelocity = Vector2.left * (RunNow ? runSpeed : moveSpeed) * StatMath.MoveSpeedMul * AttackMoveMul;
                if (!TurnLocked) // 공격 모션 중엔 방향 고정 (W-13, 10-30 던파식)
                {
                    transform.localScale = new Vector3(-chInRommSize, chInRommSize, 1);
                }
            }
            else if (horizontalInput > 0)
            {
                if (isJumpAnim == false && isAttackAnim == false)
                {
                    if (isRun)
                    {
                        PlayLoopAnim("Run");
                        PlayRunSound();
                    }
                    else
                    {
                        PlayWalkSound();
  
               
                        PlayLoopAnim("Walk");
                    }
                }

                states = "moveRight";
                moveVelocity = Vector2.right * (RunNow ? runSpeed : moveSpeed) * StatMath.MoveSpeedMul * AttackMoveMul;
                if (!TurnLocked) // 공격 모션 중엔 방향 고정 (W-13, 10-30 던파식)
                {
                    transform.localScale = new Vector3(chInRommSize, chInRommSize, 1);
                }
            }
            else if (horizontalInput == 0 && states != "move" && check ==false)
            {
                if (isJumpAnim == false && isUpLadder == false&& states != "dash" && isAttackAnim == false)
                    Rig.Play("Idle");
                check = true;
                moveSequence = DOTween.Sequence()
                    .AppendInterval(0.3f)
                    .AppendCallback(() => RunChecker());
            }
            // Applying velocity to the Rigidbody2D
            // CF-9: 지상은 0.06초 가속·0.05초 감속, 공중은 즉시
            float vx = OnMovementGround ? CombatRules.GroundVx(rb.linearVelocity.x, moveVelocity.x, Time.deltaTime) : moveVelocity.x;
            rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
        }

    }

    bool check = false;
    void RunChecker()
    {
        check = false;
        if (horizontalInput < 0.1f)
        {
            if (states != "dash")
            {
                states = "move";
                isRun = false;

            }
            states = "move";
            isRun = false;
        }
    }
    void VerticalRunChecker()
    {
        check = false;
        if (verticalInput == 0)
        {

            states = "move";
            isRun = false;

            if(isUpLadder == true && isAttackAnim == false)
            {
                Rig.Play("RopeStay");
            }
        }
    }
    Sequence dashSequence;
    public float dashMovePoint;
    private AttackManager _attackManager;

    /// <summary>홀드 스킬을 끊는다 — 대시 시작·피격 경직 진입 (B-113, 설계 2.4-6)</summary>
    public void CancelSkillHold()
    {
        if (_attackManager != null)
        {
            _attackManager.CancelHold();
        }
    }

    void Dash()
    {
        if (!CanMovementDash) { return; }
        rb.gravityScale = 3;
        isUpLadder = false;
        ChangeLadderLayerOrder();
        CancelSkillHold(); // 홀드 중 대시하면 홀드를 끝낸다(쿨 적용)
        // 공격 중 대시는 잠긴 방향이 아니라 입력 방향으로 나가고, 남은 모션 동안 방향 잠금도 푼다 (10-30). states 가 dash 로 바뀌기 전에 불러야 돈다
        FaceInputForAttack();
        attackFacingLocked = false;
        MasterAudio.PlaySound("Dash");
        if (isAttackAnim == false)
        {
            Rig.Play("Dash");

        }
        // 대시 무적은 자기 토큰으로: 끝나도 다른 출처(스킬 대시 등)의 무적을 끄지 않는다 (B-114 C1)
        PlayerStatus.AcquireFor(StatusFlag.Invincible, dashDuration, "Dash");
        DashAfterimage.Begin(Rig, dashDuration);
        boxColliderTrue = false;
        PlayerHealthManager.Instance.SteminaDown(dashStemina);
        states = "dash";


        rb.gravityScale = 0f;
        // 대쉬 속도로만 이동하도록 설정
        rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0f);
        if (hit1.collider != null || hit2.collider != null)
        {
            DustPuffs.Dash(Rig is PixelRig ? Rig.transform.position : groundCheck.transform.position, transform.localScale.x); // 땅 위 대시만 먼지(픽셀 리그는 발바닥 원점)
        }
        // 대쉬 타이머 설정
        dashTimer = dashWait;

        moveSequence.Kill();
        dashSequence.Kill();
        dashSequence = DOTween.Sequence()
        .AppendInterval(dashDuration) // 2초 대기
        .AppendCallback(() => rb.gravityScale = 3f)
        .AppendCallback(() => rb.linearVelocity = new Vector2(0f, 0f))
        .AppendCallback(() => states = "move");
    }

    public void SkillDash(float dashDu, float dashSpd, float dashYSpeed, bool isDashInvins, bool isBackDash, bool isGravity = false)
    {
        states = "dash";
        DashAfterimage.Begin(Rig, dashDu);
        // 스킬 대시 무적·슈퍼아머도 시간 만료 토큰. 연속 스킬 대시로 시퀀스가 Kill돼도 반드시 풀린다 (B-114 C2, C3)
        if (isDashInvins == true)
        {
            PlayerStatus.AcquireFor(StatusFlag.Invincible, dashDu, "SkillDash");
        }
        PlayerStatus.AcquireFor(StatusFlag.SuperArmor, dashDu, "SkillDash");
        boxColliderTrue = false;
        if(isGravity == false)
        {
            rb.gravityScale = 0f;
        }

        // 대쉬 속도로만 이동하도록 설정
        if(isBackDash == true)
        {
            rb.linearVelocity = new Vector2(transform.localScale.x * -dashSpd, transform.localScale.y * dashYSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector2(transform.localScale.x * dashSpd, transform.localScale.y * dashYSpeed);
        }

        moveSequence.Kill();
        dashSequence.Kill();
        dashSequence = DOTween.Sequence()
        .AppendInterval(dashDu) //
        .AppendCallback(() => rb.gravityScale = 3f)
        .AppendCallback(() => rb.linearVelocity = new Vector2(0f, 0f))
        .AppendCallback(() => states = "move");
    }


    public float wallJumpForce;
    void WallJump()
    {
        states = "wallJump";
        if (transform.localScale == new Vector3(-chInRommSize, chInRommSize, 1))
        {
            jumpsRemaining--;
            rb.linearVelocity = new Vector2(1, 1.5f) * wallJumpForce;
        }
        else
        {
            jumpsRemaining--;
            rb.linearVelocity = new Vector2(-1, 1.5f) * wallJumpForce;
        }
        Invoke("ReturnIsGround", 0.2f);
        Sequence sequence = DOTween.Sequence()
        .AppendInterval(0.4f) // 2초 대기
        .AppendCallback(() => isWallReset =false)
        .AppendCallback(() => states = "s");

    }
    bool isJumpAnim = false;
    void Jump()
    {
        if (!CanMovementJump) { return; }
        rb.gravityScale = 3;
        isUpLadder = false;
        ChangeLadderLayerOrder();
       
        MasterAudio.PlaySound("Jump");
        BoxCollider2D bc = this.GetComponent<BoxCollider2D>();
        /*
        if(bc.isTrigger == true)
        {
            bc.isTrigger = false;
        }

        */


        if(isAttackAnim == false)
        {
            isJumpAnim = true;
            Rig.Play("Jump");
        }


        boxColliderTrue = false;
        isWallReset = false;
        states = "jump";
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpsRemaining--;
        _jumpCutArmed = true;
        _jumpStartTime = Time.time;
        isGround = false;
        isOnGround = false;
        coyote.NotifyJumped();
        // 점프 후에도 isGrounded를 false로 유지하여 FixedUpdate에서 다시 체크할 수 있도록 합니다.
        Invoke("ReturnIsGround", 0.2f);
    }

    bool _jumpCutArmed;
    float _jumpStartTime;

    /// <summary>CF-7 가변 점프: 상승 중 키를 떼면 vy×0.5 를 1회 적용(최소 홀드 0.08초 전에 뗐으면 그 시점까지 기다린다)</summary>
    void UpdateJumpCut()
    {
        if (!_jumpCutArmed)
        {
            return;
        }
        if (rb.linearVelocity.y <= 0f || states == "dash" || states == "wallJump" || isAttacked || isUpLadder)
        {
            _jumpCutArmed = false;
            return;
        }
        if (CombatRules.ShouldCutJump(jumpAction.IsPressed(), Time.time - _jumpStartTime, rb.linearVelocity.y))
        {
            _jumpCutArmed = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * CombatRules.JumpCutMul);
        }
    }

    void ReturnIsGround()
    {
        isOnGround = true;
    }
    public GameObject groundCheck;
    public GameObject groundCheck2;
    public bool isPlafromCheck = true;
    public bool isOnGround = true;
    // 씬에 옛 값(0.1/0.08)이 직렬화돼 있어 필드 대신 CombatRules 상수를 쓴다(CF-4)
    const float _jumpBufferTime = CombatRules.JumpBufferTime;
    const float _coyoteTime = CombatRules.CoyoteTime;
    [SerializeField] private bool _fallConsumesGroundJump = true;
    InputBuffer _jumpBuffer, _dashBuffer;
    InputBuffer dashBuffer => _dashBuffer ?? (_dashBuffer = new InputBuffer(CombatRules.DashBufferTime));
    CoyoteTimer _coyote;
    InputBuffer jumpBuffer => _jumpBuffer ?? (_jumpBuffer = new InputBuffer(_jumpBufferTime));
    CoyoteTimer coyote => _coyote ?? (_coyote = new CoyoteTimer(_coyoteTime));
    int groundMask;
    RaycastHit2D hit1;
    RaycastHit2D hit2;
    void FixedUpdate()
    {
        if (groundMask == 0)
        {
            groundMask = LayerMask.GetMask("Ground"); // 매 프레임 문자열 조회 대신 1회 캐시
        }
        hit1 = Physics2D.Raycast(groundCheck.transform.position, Vector2.down, 0.3f, groundMask);
        hit2 = Physics2D.Raycast(groundCheck2.transform.position, Vector2.down, 0.3f, groundMask);
        Debug.DrawRay(groundCheck.transform.position, Vector2.down * 0.3f, Color.red);

        // 바닥 감지 레이캐스트

        bool grounded = MovementGrounded;
        if (grounded)
        {
            jumpsRemaining = AvailableJumps;
            isWallReset = false;
            if (isJumpAnim == true && isUpLadder == false && states != "dash" && isAttackAnim ==false)
            {
                isJumpAnim = false;
                Rig.Play("Idle");
                LandSquash();
                // 픽셀 리그는 원점이 발바닥 — 땅 감지점은 발보다 위라 먼지가 무릎 높이에서 피었다
                DustPuffs.Land(Rig is PixelRig ? Rig.transform.position : groundCheck.transform.position, Rig is PixelRig pr ? pr.LastLandStrength : 0.6f); // 높이 떨어질수록 큰 먼지
            }


        }

        // 착지 상태를 계속 유지해야 코요테 타이머가 서 있는 플레이어를 추락으로 보지 않는다.
        isGround = grounded;

        // 메이플식: 점프 없이 발판에서 떨어지면 코요테 시간 뒤 공중 점프 1회만 남긴다 (Q-105 Q-B)
        coyote.SetGrounded(isGround || isUpLadder, Time.time);
        if (_fallConsumesGroundJump && coyote.FellPastCoyote(Time.time) && jumpsRemaining >= AvailableJumps)
        {
            jumpsRemaining = AvailableJumps - 1;
        }

        /*
        if(states != "dash" &&  isUpLadder == false&& isDownJump ==false && isPlafromCheck == true)//&&isPlafromCheck == true 
        {
            RaycastHit2D hitWall = Physics2D.Raycast(transform.position, Vector2.right,0.3f, LayerMask.GetMask("Ground"));
            RaycastHit2D hitWall2 = Physics2D.Raycast(transform.position, Vector2.left, 0.3f, LayerMask.GetMask("Ground"));

            if (((hitWall.collider != null && hitWall.collider.CompareTag("Ground") && transform.localScale == new Vector3(chInRommSize, chInRommSize, 1)) ||( hitWall2.collider != null && hitWall2.collider.CompareTag("Ground") && transform.localScale == new Vector3(-chInRommSize, chInRommSize, 1)) ) && isGround ==false)
            {
                horizontalInput = moveAction.ReadValue<float>();
                if (isWallReset == false && horizontalInput != 0)
                {

                    isWallReset = true;
                    isWall = true;
                    rb.velocity = new Vector2(0f, 0f);
                    rb.gravityScale = 0.1f;
                }

            }
            else
            {
                isWall = false;
                rb.gravityScale = 3f;
            }
        }
        */

    }

    public GameObject currentOneWayPlatform;

    [SerializeField] private BoxCollider2D playerCollider;




    bool IsWall(Vector2 normal)
    {
        // Calculate the angle between the normal vector and the up vector
        float angle = Vector2.Angle(normal, Vector2.up);

        // Check if the angle is within a certain range (e.g., 45 degrees)
        return angle <= 45f;
    }
    bool IsGrounded(Vector2 normal)
    {
        // 내적을 사용하여 각도가 일정 범위 내에 있는지 확인
        float dot = Vector2.Dot(normal, Vector2.up);
        return dot >= 0.9f; // 내적 값이 0.9 이상이면 일정 범위 내에 있다고 판단
    }
    public bool isLadder;
    public bool isUpLadder;


    bool isAttackAnim = false;
    Sequence attackAnimSequence;
    public GameObject RightSoket;
    GameObject revOb;
    /// <param name="lockFacing">true(기본)면 모션 내내 방향 고정, false면 시작 후 _turnLockTime 만 고정</param>
    public void ActiveAttackAnim(bool isRevers, string anim, float time, bool isDelay = false, bool lockFacing = true)
    {
        if(anim == null || anim == "")
        {
            return;
        }
        if(isJumpAnim == true)
        {
            isJumpAnim = false;
        }
        Rig.Play(anim);
        // 공격 애니 속도 = 공속 배율 (세트·장비 공속이 눈에 보이게)
        attackAnimPlaying = anim;
        attackStartedAt = Time.time;
        attackFacingLocked = lockFacing;
        if (hitStopTimer <= 0f)
        {
            Rig.SetAnimationSpeed(anim, StatMath.AttackSpeedMul);
        }
        isAttackAnim = true;
        if(isUpLadder == true)
        {
            Debug.Log("공격시작");
            isRopeAttack = true;
        }
        if (isRevers)
        {
            // 장착 무기의 오른손 스프라이트를 직접 쓴다. 소켓 자식이 없으면(무기 교체 직후 등) 건너뛴다
            revOb = Weapon.Equipped != null && Weapon.Equipped.RightWeqponSprite != null ? Weapon.Equipped.RightWeqponSprite
                : RightSoket.transform.childCount > 0 ? RightSoket.transform.GetChild(0).gameObject : null;
        }
        if (isRevers && revOb != null)
        {
            revOb.transform.localScale = new Vector3(-Mathf.Abs(revOb.transform.localScale.x), revOb.transform.localScale.y, revOb.transform.localScale.z);
        }

        if(isDelay == false)
        {
            attackAnimSequence.Kill();
            attackAnimSequence = DOTween.Sequence()
           .AppendInterval(time)
           .AppendCallback(() => isAttackAnim = false)
           .AppendCallback(() => ReversSprite(isRevers))
           .AppendCallback(() => EndRopeAttackAnim())
           .AppendCallback(() => ReturnToAirAnim());

        }



    }

    void ReversSprite(bool isRev)
    {
        if (isRev && revOb != null)
        {
            revOb.transform.localScale = new Vector3(Mathf.Abs(revOb.transform.localScale.x), revOb.transform.localScale.y, revOb.transform.localScale.z);
        }
    }
    public void StopAttackAnim()
    {
        attackAnimSequence.Kill();
       isAttackAnim = false;
        EndRopeAttackAnim();
    }
    void EndRopeAttackAnim()
    {
        if(isUpLadder == true)
        {
            isRopeAttack = false;
            Rig.Play("RopeStay");
        }

    }

    void ChangeLadderLayerOrder()
    {
        if(nowLadder != null)
        {
            SpriteRenderer sR = nowLadder.GetComponent<SpriteRenderer>();
            SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
            sR.sortingOrder = LadderLayer;
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
    
            if (collision.tag == "Ladder"&& DatabaseManager.checkAttackLadder == false)
            {
                ladderJumpCheck = true;
                isLadder = true;
                nowLadder = collision.gameObject;
            }
            if (collision.tag == "InGroundPlayer")
            {
                //  Debug.Log(collision.name);
                if (isJumpAnim == true && isUpLadder == false && rb.linearVelocity.y == 0 && states != "dash" && isAttackAnim == false)
                {
                    isJumpAnim = false;
                    Rig.Play("Idle");

                }
                if (jumpsRemaining != maxJumps)
                {
                    jumpsRemaining = AvailableJumps;
                }
                if (isUpLadder == true)
                {
                    platformTrue = true;
                }

            }

            if (collision.tag == "InGroundPlayer")
            {

            }
        


    }

    bool isRopeAttack = false;
    private void OnTriggerExit2D(Collider2D collision)
    {

            if (collision.tag == "Ladder")
            {
                if (isRopeAttack == false)
                {
                    isLadder = false;
                    nowLadder = collision.gameObject;
                    BoxCollider2D bc = this.GetComponent<BoxCollider2D>();

                    if (isUpLadder == true)
                    {
                        //bc.isTrigger = false;
                        boxColliderTrue = false;
                        rb.gravityScale = 3;
                        isUpLadder = false;
                        ChangeLadderLayerOrder();
                        nowLadder = null;
                        AbleCollision();
                    }
                }
                else if (isRopeAttack == true)
                {
                    Debug.Log("공격끝");
                    isRopeAttack = false;
                    Rig.Play("RopeStay");
                }
            }
            if (collision.tag == "InGroundPlayer")
            {
                Debug.Log("나감");
                platformTrue = false;
            }
        

    }

    void PlayRunSound()
    {
        if (Rig is PixelRig)
        {
            return; // 픽셀 리그는 발 닿는 포즈에서 직접 낸다(Footsteps.Contact)
        }
        Footsteps.Step(true); // 지면별 발소리(풀·흙·돌) — 예전 MasterAudio SoilRun 대체
    }
    void PlayWalkSound()
    {
        if (Rig is PixelRig)
        {
            return;
        }
        Footsteps.Step(false);
    }
    void PlayRopeSound()
    {
        if (!MasterAudio.IsSoundGroupPlaying("Rope"))
        {
            MasterAudio.PlaySound3DAtTransform("Rope", this.transform);
        }
    }
}
