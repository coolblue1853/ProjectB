using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using AnyPortrait;
public class PlayerController : MonoBehaviour
{
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
    }

    public bool boxColliderTrue = false;
  public  bool platformTrue = false; 
 
    BoxCollider2D bc;
    public bool isAttacked;
    public bool isAttackedUp;
    void Start()
    {
        bc = this.GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumps;
    }
    bool once = false;
    void Update()
    {
       if (verticalInput == 0 && states != "move" && isUpLadder ==true)
        {
            mainCharacter.Play("RopeStay");

        }
        if (horizontalInput == 0&& isUpLadder == false)
        {
            if (isJumpAnim == false && isUpLadder == false && states != "dash")
            {

                mainCharacter.Play("Idle");
            }

        }
        if (isJumpAnim == false && isUpLadder == false && states != "dash")
        {
            rb.gravityScale = 3f;

        }
        if (rb.velocity.y ==0 && isJumpAnim == true)
        {
            isJumpAnim = false;
        }





        if (Input.GetKeyDown(KeyCode.W))
        {
         
            mainCharacter.SetMeshImage("Hat", CharBaseSuit2);
            mainCharacter.SetMeshImage("RArm", CharBaseSuit2);
            mainCharacter.SetMeshImage("Rsholder", CharBaseSuit2);
            mainCharacter.SetMeshImage("RHand", CharBaseSuit2);
            mainCharacter.SetMeshImage("LArm", CharBaseSuit2);
            mainCharacter.SetMeshImage("LSholder", CharBaseSuit2);
            mainCharacter.SetMeshImage("LHand", CharBaseSuit2);
            mainCharacter.SetMeshImage("RDownLeg", CharBaseSuit2);
            mainCharacter.SetMeshImage("RUpperLeg", CharBaseSuit2);
            mainCharacter.SetMeshImage("RFoot", CharBaseSuit2);
            mainCharacter.SetMeshImage("LDownLeg", CharBaseSuit2);
            mainCharacter.SetMeshImage("LUpperLeg", CharBaseSuit2);
            mainCharacter.SetMeshImage("LFoot", CharBaseSuit2);
            mainCharacter.SetMeshImage("UppderBody", CharBaseSuit2);
            mainCharacter.SetMeshImage("DwonBody", CharBaseSuit2);
        }
            if (DatabaseManager.isOpenUI == false && isAttacked == false && isAttackedUp == true)
        {
            isAttackedUp = false;
            if (rb.velocity != Vector2.zero )
            {
                rb.velocity = Vector2.zero;
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

        if (DatabaseManager.isOpenUI == true && rb.velocity != Vector2.zero && once == false)
        {
            once = true;
            rb.velocity = Vector2.zero;
        }
        if(DatabaseManager.isOpenUI == false && isAttacked == false)
        {
            once = false;
            if (rb.velocity != Vector2.zero && DatabaseManager.weaponStopMove == true && isGround == true)
            {
                rb.velocity = Vector2.zero;
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
            if (dashAction.triggered && dashTimer <= 0f && DatabaseManager.weaponStopMove == false && PlayerHealthManager.Instance.nowStemina > dashStemina)
            {
                Debug.Log("대쉬작동");
                rb.gravityScale =3;
                isUpLadder = false;
                ChangeLadderLayerOrder();

                Dash();
            }



            // 점프
            if (jumpAction.triggered && DatabaseManager.weaponStopMove == false)
            {
                if (verticalInput < -0.5f && currentOneWayPlatform != null)
                {
                  DisableCollision();
                }
                else if (isWall == true)
                {
                    WallJump();
                }
                else if ((isGround || jumpsRemaining > 0) && states != "dash")
                {
                    if(jumpsRemaining > 0)
                    {
                        rb.gravityScale = 3;
                        isUpLadder = false;
                        ChangeLadderLayerOrder();
                        Jump();
                    }

                }

            }

            // 대쉬 타이머 업데이트
            if (dashTimer > 0f)
            {
                dashTimer -= Time.deltaTime;
            }
        }

        if (states != "dash" && states != "wallJump" && isLadder == true)
        {
            if(upAction.triggered == true|| downAction.triggered)
            {

                jumpsRemaining = maxJumps;
                rb.velocity = Vector2.zero;
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
        /*
        else if (isLadder == false && ladderJumpCheck ==true)
        {
            if (currentOneWayPlatform != null)
            {
                ladderJumpCheck = false;
                bool isCollisionIgnored = Physics2D.GetIgnoreCollision(playerCollider, platformCollider);
                if (isCollisionIgnored)
                {
                   // AbleCollision();
                }
            }
        }
        */

        }
    bool ladderJumpCheck = false;
    GameObject nowLadder;
    void LadderMove()
    {
        moveVelocity = Vector2.zero;

        if (states != "dash" && states != "wallJump")
        {
            verticalInput = verticalAction.ReadValue<float>();
            if (verticalInput < -0.2f)
            {
                states = "moveDown";
                mainCharacter.Play("RopeDown");
                moveVelocity = Vector2.down * (isRun ? runSpeed : moveSpeed);
            }
            else if (verticalInput > 0.2f)
            {

                Debug.Log("올라가는중");
                states = "moveUp";
                mainCharacter.Play("RopeUp");
                moveVelocity = Vector2.up * (isRun ? runSpeed : moveSpeed);
            }
            else if (verticalInput == 0 && states != "move" && check == false)
            {
                mainCharacter.Play("RopeStay");
                check = true;
                Sequence sequence = DOTween.Sequence()
                    .AppendInterval(0.3f)
                    .AppendCallback(() => VerticalRunChecker());
            }
            // Applying velocity to the Rigidbody2D
            rb.velocity = new Vector2(rb.velocity.x, moveVelocity.y);
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
        mainCharacter.Play("Jump");
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
              if (isJumpAnim == false)
                    {
                        if (isRun)
                        {
                            mainCharacter.Play("Run");

                        }
                        else
                        {
                            mainCharacter.Play("Walk");
                        }
                    }


                states = "moveLeft";
                moveVelocity = Vector2.left * (isRun ? runSpeed : moveSpeed);
                transform.localScale = new Vector3(-chInRommSize, chInRommSize, 1);
            }
            else if (horizontalInput > 0)
            {
                if (isJumpAnim == false)
                {
                    if (isRun)
                    {
                        mainCharacter.Play("Run");

                    }
                    else
                    {
                        mainCharacter.Play("Walk");
                    }
                }

                states = "moveRight";
                moveVelocity = Vector2.right * (isRun ? runSpeed : moveSpeed);
                transform.localScale = new Vector3(chInRommSize, chInRommSize, 1);
            }
            else if (horizontalInput == 0 && states != "move" && check ==false)
            {
                if (isJumpAnim == false && isUpLadder == false&& states != "dash")
                    mainCharacter.Play("Idle");
                check = true;
                moveSequence = DOTween.Sequence()
                    .AppendInterval(0.3f)
                    .AppendCallback(() => RunChecker());
            }
            // Applying velocity to the Rigidbody2D
            rb.velocity = new Vector2(moveVelocity.x, rb.velocity.y);
        }

    }

    bool check = false;
    void RunChecker()
    {
        check = false;
        if (horizontalInput == 0)
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

            if(isUpLadder == true)
            {
                mainCharacter.Play("RopeStay");
            }
        }
    }
    Sequence dashSequence;
    public float dashMovePoint;
    void Dash()
    {
        if(states != "wallJump")
        {
            BoxCollider2D bc = this.GetComponent<BoxCollider2D>();
            /*
            if (bc.isTrigger == true)
            {
                bc.isTrigger = false;
            }
            */
            mainCharacter.Play("Dash");
            boxColliderTrue = false;
            PlayerHealthManager.Instance.SteminaDown(dashStemina);
            states = "dash";
            

            rb.gravityScale = 0f;
            // 대쉬 속도로만 이동하도록 설정
            rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0f);
            // 대쉬 타이머 설정
            dashTimer = dashWait;

            moveSequence.Kill();
            dashSequence.Kill();
           dashSequence = DOTween.Sequence()
           .AppendInterval(dashDuration) // 2초 대기
            .OnComplete(() => rb.gravityScale = 3f)
            .OnComplete(() => rb.velocity = new Vector2(0f, 0f))
            .OnComplete(() => states = "move");
        }
    }


    public float wallJumpForce;
    void WallJump()
    {
        states = "wallJump";
        if (transform.localScale == new Vector3(-chInRommSize, chInRommSize, 1))
        {
            jumpsRemaining--;
            rb.velocity = new Vector2(1, 1.5f) * wallJumpForce;
        }
        else
        {
            jumpsRemaining--;
            rb.velocity = new Vector2(-1, 1.5f) * wallJumpForce;
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
        BoxCollider2D bc = this.GetComponent<BoxCollider2D>();
        /*
        if(bc.isTrigger == true)
        {
            bc.isTrigger = false;
        }

        */

        isJumpAnim = true;
        mainCharacter.Play("Jump");

        boxColliderTrue = false;
        isWallReset = false;
        states = "jump";
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpsRemaining--;
        isGround = false;
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
    RaycastHit2D hit1;
    RaycastHit2D hit2;
    void FixedUpdate()
    {
        hit1 = Physics2D.Raycast(groundCheck.transform.position, Vector2.down, 0.3f, LayerMask.GetMask("Ground"));
        hit2 = Physics2D.Raycast(groundCheck2.transform.position, Vector2.down, 0.3f, LayerMask.GetMask("Ground"));
        Debug.DrawRay(groundCheck.transform.position, Vector2.down * 0.3f, Color.red);

        // 바닥 감지 레이캐스트

        if (((hit1.collider != null && hit1.collider.CompareTag("Ground") && IsGrounded(hit1.normal)) ||( hit2.collider != null && hit2.collider.CompareTag("Ground") && IsGrounded(hit2.normal))) && isOnGround == true && isPlafromCheck == true)
        {

            isOnGround = false;
            jumpsRemaining = maxJumps;
            if (isJumpAnim == true && isUpLadder == false && states != "dash")
            {
                isJumpAnim = false;
                mainCharacter.Play("Idle");

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
    bool isUpLadder;

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
        if(collision.tag == "Ladder")
        {
            ladderJumpCheck = true;
            isLadder = true;
            nowLadder = collision.gameObject;
        }
        if (collision.tag == "InGroundPlayer")
        {
            Debug.Log(collision.name);
            if (isJumpAnim == true&& isUpLadder == false && rb.velocity.y == 0 && states != "dash")
            {
                isJumpAnim = false;
                mainCharacter.Play("Idle");

            }
            if (jumpsRemaining != maxJumps)
            {
                jumpsRemaining = maxJumps;
            }
            if(isUpLadder == true)
            {
                platformTrue = true;
            }

        }

        if(collision.tag == "InGroundPlayer")
        {

        }
    }
    

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Ladder")
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
        if (collision.tag == "InGroundPlayer")
        {
            Debug.Log("나감");
            platformTrue = false;
        }
    }
}