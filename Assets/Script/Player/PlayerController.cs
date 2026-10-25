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
    public bool isAttackedUp;
    void Start()
    {
        this.gameObject.transform.position = SaveManager.instance.datas.playerPos;
        proCamera.CenterOnTargets();
        bc = this.GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumps;
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

    [Tooltip("공격 시작 후 방향 전환을 막는 시간(초). 이동 가능 무기로 휘두르는 중 뒤돌면 판정까지 뒤집히던 문제 (W-13)")]
    [SerializeField] private float _turnLockTime = 0.2f;
    float attackStartedAt = -10f;
    bool TurnLocked => isAttackAnim && Time.time - attackStartedAt < _turnLockTime;

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
            // 공격 이동정지 중에도 대시는 움직인다(대시가 MoveLock을 끄지 않고 무시, B-114 C5)
            // 가로 속도만 멈춘다. y 까지 0 으로 만들면 공중 평타 연타로 떨어지지 않는다(W-08)
            if (rb.linearVelocity.x != 0f && DatabaseManager.weaponStopMove == true && states != "dash")
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

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
            if (DatabaseManager.weaponStopMove == false && isUpLadder == false&& states != "dash")
            {

                Move();
            }


            // 대쉬
            if (DatabaseManager.isOpenUI == false && !PlayerStatus.Has(StatusFlag.DashLock) && dashAction.triggered && dashTimer <= 0f && PlayerHealthManager.Instance.nowStemina > dashStemina)//&& DatabaseManager.weaponStopMove == false 
            {
                rb.gravityScale =3;
                isUpLadder = false;
                ChangeLadderLayerOrder();
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
                if (jumpsRemaining > 0 || coyote.InCoyote(Time.time))
                {
                    jumpBuffer.Consume(Time.time);
                    if (jumpsRemaining <= 0)
                    {
                        jumpsRemaining = 1;
                    }
                    rb.gravityScale = 3;
                    isUpLadder = false;
                    ChangeLadderLayerOrder();
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

                jumpsRemaining = maxJumps;
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0;
                isUpLadder = true;

                // 로프 위에 고정시키기
                Vector2 newPosition = new Vector2(nowLadder.transform.position.x, transform.position.y);
                
                //로프 애니메이션 재생, 레이어 순서 변경
                SpriteRenderer sR = nowLadder.GetComponent<SpriteRenderer>();
                SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
                sR.sortingOrder = playerSR.sortingOrder + 1;

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


            if (horizontalInput < 0)
            {

                transform.localScale = new Vector3(-chInRommSize, chInRommSize, 1);
            }
            else if (horizontalInput > 0)
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
    Sequence moveSequence;
    void Move()
    {
        moveVelocity = Vector2.zero;


        if (states != "dash" && states != "wallJump")
        {
            horizontalInput = moveAction.ReadValue<float>();
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
                moveVelocity = Vector2.left * (isRun ? runSpeed * StatMath.MoveSpeedMul : moveSpeed * StatMath.MoveSpeedMul);
                if (!TurnLocked) // 공격 초반엔 방향 고정 (W-13)
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
                moveVelocity = Vector2.right * (isRun ? runSpeed * StatMath.MoveSpeedMul : moveSpeed * StatMath.MoveSpeedMul);
                if (!TurnLocked) // 공격 초반엔 방향 고정 (W-13)
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
            rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
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
    void Dash()
    {
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
            DustPuffs.Dash(groundCheck.transform.position, transform.localScale.x); // 땅 위 대시만 먼지
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
        isGround = false;
        coyote.NotifyJumped();
        // 점프 후에도 isGrounded를 false로 유지하여 FixedUpdate에서 다시 체크할 수 있도록 합니다.
        Invoke("ReturnIsGround", 0.2f);
    }

    void ReturnIsGround()
    {
        isOnGround = true;
    }
    public GameObject groundCheck;
    public GameObject groundCheck2;
    public bool isPlafromCheck = true;
    public bool isOnGround = true;
    [SerializeField] private float _jumpBufferTime = 0.1f;
    [SerializeField] private float _coyoteTime = 0.08f;
    [SerializeField] private bool _fallConsumesGroundJump = true;
    InputBuffer _jumpBuffer;
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

        if (((hit1.collider != null && hit1.collider.CompareTag("Ground") && IsGrounded(hit1.normal)) ||( hit2.collider != null && hit2.collider.CompareTag("Ground") && IsGrounded(hit2.normal))) && isOnGround == true && isPlafromCheck == true)
        {

            isOnGround = false;
            jumpsRemaining = maxJumps;
            if (isJumpAnim == true && isUpLadder == false && states != "dash" && isAttackAnim ==false)
            {
                isJumpAnim = false;
                Rig.Play("Idle");
                LandSquash();
                // 픽셀 리그는 원점이 발바닥 — 땅 감지점은 발보다 위라 먼지가 무릎 높이에서 피었다
                DustPuffs.Land(Rig is PixelRig ? Rig.transform.position : groundCheck.transform.position, Rig is PixelRig pr ? pr.LastLandStrength : 0.6f); // 높이 떨어질수록 큰 먼지
            }


        }

        // Ground 태그를 가진 오브젝트에 닿았고, 각도가 일정 범위 내에 있으면 점프 횟수 초기화
        if (hit1.collider != null && hit1.collider.CompareTag("Ground") && IsGrounded(hit1.normal) && isOnGround == true)//&& isPlafromCheck == true
        {

            isWallReset = false;
            isGround = true;
           // jumpsRemaining = maxJumps;



        }
        else
        {
            isGround = false;
        }

        // 메이플식: 점프 없이 발판에서 떨어지면 코요테 시간 뒤 공중 점프 1회만 남긴다 (Q-105 Q-B)
        coyote.SetGrounded(isGround || isUpLadder, Time.time);
        if (_fallConsumesGroundJump && coyote.FellPastCoyote(Time.time) && jumpsRemaining >= maxJumps)
        {
            jumpsRemaining = maxJumps - 1;
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
    public void ActiveAttackAnim(bool isRevers, string anim, float time, bool isDelay = false)
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
                    jumpsRemaining = maxJumps;
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
        Footsteps.Step(true); // 지면별 발소리(풀·흙·돌) — 예전 MasterAudio SoilRun 대체
    }
    void PlayWalkSound()
    {
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