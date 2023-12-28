using UnityEngine;
using DG.Tweening;
public class PlayerController : MonoBehaviour
{
    float chInRommSize = 1f;
    public float runSpeed = 10f;  // 이동 속도
    public int runStemina; // 달리기시 사용하는 스테미나
    public float intervalRunStemina; // 달리기시 사용하는 스테미나
    public float moveSpeed = 10f;  // 이동 속도
    public float jumpForce = 13f; // 점프 힘
    public float dashSpeed = 20f; // 대쉬 속도
    public float dashDuration = 0.2f; // 대쉬 지속 시간
    private float dashWait = 2f; // 대쉬 타이머
    private float dashTimer; // 대쉬 타이머
    public int dashStemina; // 대쉬시 사용하는 스테미나
    public int maxJumps = 2;      // 최대 점프 횟수
    public int jumpsRemaining;    // 남은 점프 횟수
    private bool isChangeDirection = false;    // 남은 점프 횟수
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isRun = false;
    public bool isWall = false;
    private bool isWallReset = false;
    Vector2 moveVelocity = Vector2.zero;
    float horizontalInput;

    Sequence waitSequence;
    public string states = "";
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        if (rb.velocity != Vector2.zero  && DatabaseManager.weaponStopMove == true && isGrounded == true)
        {
            rb.velocity = Vector2.zero;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && isRun == false && DatabaseManager.weaponStopMove == false && Mathf.Abs(horizontalInput)>0)
        {
            isRun = true;
            runSteminaDown();
        }
        else if ((Input.GetKeyDown(KeyCode.LeftShift) && isRun == true) && DatabaseManager.weaponStopMove == false)
        {
            isRun = false;
        }
        // 이동
        if(DatabaseManager.weaponStopMove == false)
        {
            Move();
        }


        // 대쉬
        if (Input.GetKeyDown(KeyCode.Z) && dashTimer <= 0f && DatabaseManager.weaponStopMove == false && PlayerHealthManager.Instance.nowStemina > dashStemina)
        {
            Dash();
        }



        // 점프
        if (Input.GetKeyDown(KeyCode.C) && DatabaseManager.weaponStopMove == false)
        {
            if(isWall == true)
            {
                WallJump();
            }
            else if((isGrounded || jumpsRemaining > 0) && states != "dash")
            {
                Jump();
            }

        }

        // 대쉬 타이머 업데이트
        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
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
    void Move()
    {
        moveVelocity = Vector2.zero;

        if (states != "dash" && states != "wallJump")
        {
             horizontalInput = Input.GetAxisRaw("Horizontal");

            if (horizontalInput < 0)
            {
                states = "moveLeft";
                moveVelocity = Vector2.left * (isRun ? runSpeed : moveSpeed);
                transform.localScale = new Vector3(-chInRommSize, chInRommSize, 1);
            }
            else if (horizontalInput > 0)
            {
                states = "moveRight";
                moveVelocity = Vector2.right * (isRun ? runSpeed : moveSpeed);
                transform.localScale = new Vector3(chInRommSize, chInRommSize, 1);
            }
            else if (horizontalInput == 0 && states != "move")
            {
                states = "move";
                Sequence sequence = DOTween.Sequence()
                    .AppendInterval(0.2f)
                    .AppendCallback(() => RunChecker());
            }
            // Applying velocity to the Rigidbody2D
            rb.velocity = new Vector2(moveVelocity.x, rb.velocity.y);
        }


    }

    void RunChecker()
    {
        if(Input.GetAxisRaw("Horizontal")== 0)
        {
            isRun = false;
        }
    }

    void Dash()
    {
        if(states != "wallJump")
        {
            PlayerHealthManager.Instance.SteminaDown(dashStemina);
            states = "dash";
            rb.gravityScale = 0f;
            // 대쉬 속도로만 이동하도록 설정
            rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0f);
            // 대쉬 타이머 설정
            dashTimer = dashWait;


                         Sequence sequence = DOTween.Sequence()
            .AppendInterval(dashDuration) // 2초 대기
            .AppendCallback(() => rb.gravityScale = 3f)
            .AppendCallback(() => rb.velocity = new Vector2(0f, 0f))
            .AppendCallback(() => states = "s");
        }
    }
    void WallJump()
    {
        states = "wallJump";
        if (transform.localScale == new Vector3(-chInRommSize, chInRommSize, 1))
        {
            jumpsRemaining--;
            rb.velocity = new Vector2(1, 1.5f) * 10f;
        }
        else
        {
            jumpsRemaining--;
            rb.velocity = new Vector2(-1, 1.5f) * 10f;
        }
        Sequence sequence = DOTween.Sequence()
        .AppendInterval(0.4f) // 2초 대기
        .AppendCallback(() => isWallReset =false)
        .AppendCallback(() => states = "s");

    }
    void Jump()
    {
        isWallReset = false;
        states = "jump";
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpsRemaining--;

        // 점프 후에도 isGrounded를 false로 유지하여 FixedUpdate에서 다시 체크할 수 있도록 합니다.
        isGrounded = false;
    }

    void FixedUpdate()
    {
        // 바닥 감지 레이캐스트
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.8f, LayerMask.GetMask("Ground"));

        // Ground 태그를 가진 오브젝트에 닿았고, 각도가 일정 범위 내에 있으면 점프 횟수 초기화
        if (hit.collider != null && hit.collider.CompareTag("Ground") && IsGrounded(hit.normal))
        {
            isWallReset = false;
            isGrounded = true;
            jumpsRemaining = maxJumps;
        }
        else
        {
            isGrounded = false;
        }
        if(states != "dash")
        {
            RaycastHit2D hitWall = Physics2D.Raycast(transform.position, Vector2.right, 0.6f, LayerMask.GetMask("Ground"));
            RaycastHit2D hitWall2 = Physics2D.Raycast(transform.position, Vector2.left, 0.6f, LayerMask.GetMask("Ground"));
            if (((hitWall.collider != null && hitWall.collider.CompareTag("Ground") && transform.localScale == new Vector3(chInRommSize, chInRommSize, 1)) ||( hitWall2.collider != null && hitWall2.collider.CompareTag("Ground") && transform.localScale == new Vector3(-chInRommSize, chInRommSize, 1)) ) && isGrounded ==false)
            {
                float horizontalInput = Input.GetAxisRaw("Horizontal");
                    if (isWallReset == false && horizontalInput != 0)
                {
                    Debug.Log("1회동작");
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
}