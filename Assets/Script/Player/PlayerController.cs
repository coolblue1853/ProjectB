using UnityEngine;
using DG.Tweening;
public class PlayerController : MonoBehaviour
{
    float chInRommSize = 1f;
    public float runSpeed = 10f;  // �̵� �ӵ�
    public int runStemina; // �޸���� ����ϴ� ���׹̳�
    public float intervalRunStemina; // �޸���� ����ϴ� ���׹̳�
    public float moveSpeed = 10f;  // �̵� �ӵ�
    public float jumpForce = 13f; // ���� ��
    public float dashSpeed = 20f; // �뽬 �ӵ�
    public float dashDuration = 0.2f; // �뽬 ���� �ð�
    private float dashWait = 2f; // �뽬 Ÿ�̸�
    private float dashTimer; // �뽬 Ÿ�̸�
    public int dashStemina; // �뽬�� ����ϴ� ���׹̳�
    public int maxJumps = 2;      // �ִ� ���� Ƚ��
    public int jumpsRemaining;    // ���� ���� Ƚ��
    private bool isChangeDirection = false;    // ���� ���� Ƚ��
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
        // �̵�
        if(DatabaseManager.weaponStopMove == false)
        {
            Move();
        }


        // �뽬
        if (Input.GetKeyDown(KeyCode.Z) && dashTimer <= 0f && DatabaseManager.weaponStopMove == false && PlayerHealthManager.Instance.nowStemina > dashStemina)
        {
            Dash();
        }



        // ����
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

        // �뽬 Ÿ�̸� ������Ʈ
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
            // �뽬 �ӵ��θ� �̵��ϵ��� ����
            rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0f);
            // �뽬 Ÿ�̸� ����
            dashTimer = dashWait;


                         Sequence sequence = DOTween.Sequence()
            .AppendInterval(dashDuration) // 2�� ���
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
        .AppendInterval(0.4f) // 2�� ���
        .AppendCallback(() => isWallReset =false)
        .AppendCallback(() => states = "s");

    }
    void Jump()
    {
        isWallReset = false;
        states = "jump";
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpsRemaining--;

        // ���� �Ŀ��� isGrounded�� false�� �����Ͽ� FixedUpdate���� �ٽ� üũ�� �� �ֵ��� �մϴ�.
        isGrounded = false;
    }

    void FixedUpdate()
    {
        // �ٴ� ���� ����ĳ��Ʈ
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.8f, LayerMask.GetMask("Ground"));

        // Ground �±׸� ���� ������Ʈ�� ��Ұ�, ������ ���� ���� ���� ������ ���� Ƚ�� �ʱ�ȭ
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
                    Debug.Log("1ȸ����");
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
        // ������ ����Ͽ� ������ ���� ���� ���� �ִ��� Ȯ��
        float dot = Vector2.Dot(normal, Vector2.up);
        return dot >= 0.9f; // ���� ���� 0.9 �̻��̸� ���� ���� ���� �ִٰ� �Ǵ�
    }
}