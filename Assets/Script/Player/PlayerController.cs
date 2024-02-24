using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
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
    public bool isGrounded;
    private bool isRun = false;
    public bool isWall = false;
    private bool isWallReset = false;
    Vector2 moveVelocity = Vector2.zero;
    float horizontalInput;
  
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


    public  bool isAttacked;
    public bool isAttackedUp;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumps;
    }
    bool once = false;
    void Update()
    {
       //Debug.Log(moveAction.ReadValue<float>());

        if(DatabaseManager.isOpenUI == true && rb.velocity != Vector2.zero && once == false)
        {
            once = true;
            rb.velocity = Vector2.zero;
        }
        if(DatabaseManager.isOpenUI == false && isAttacked == false)
        {
            once = false;
            if (rb.velocity != Vector2.zero && DatabaseManager.weaponStopMove == true && isGrounded == true)
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
            // �̵�
            if (DatabaseManager.weaponStopMove == false && isUpLadder == false)
            {
                Move();
            }


            // �뽬
            if (dashAction.triggered && dashTimer <= 0f && DatabaseManager.weaponStopMove == false && PlayerHealthManager.Instance.nowStemina > dashStemina)
            {
                rb.gravityScale =3;
                isUpLadder = false;
                Dash();
            }



            // ����
            if (jumpAction.triggered && DatabaseManager.weaponStopMove == false)
            {
                if (verticalInput < 0 && currentOneWayPlatform != null)
                {
                  DisableCollision();
                }
                else if (isWall == true)
                {
                    WallJump();
                }
                else if ((isGrounded || jumpsRemaining > 0) && states != "dash")
                {
                    rb.gravityScale = 3;
                    isUpLadder = false;
                    Jump();
                }

            }

            // �뽬 Ÿ�̸� ������Ʈ
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




                this.gameObject.transform.position = new Vector2(nowLadder.transform.position.x, this.transform.position.y);
            }


        }

        if(isUpLadder == true)
        {
            LadderMove();

            if (currentOneWayPlatform != null)
            {
                DisableCollision();
            }
        }
    }
    GameObject nowLadder;
    void LadderMove()
    {
        moveVelocity = Vector2.zero;

        if (states != "dash" && states != "wallJump")
        {
            verticalInput = verticalAction.ReadValue<float>();
            if (verticalInput < 0)
            {
                states = "moveDown";
                moveVelocity = Vector2.down * (isRun ? runSpeed : moveSpeed);
            }
            else if (verticalInput > 0)
            {
                states = "moveUp";
                moveVelocity = Vector2.up * (isRun ? runSpeed : moveSpeed);
            }
            else if (verticalInput == 0 && states != "move" && check == false)
            {
                Debug.Log("����");
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
            currentOneWayPlatform = null;
        }
    }
    BoxCollider2D platformCollider;
    private void DisableCollision()
    {
         platformCollider = currentOneWayPlatform.GetComponent<BoxCollider2D>();

        Physics2D.IgnoreCollision(playerCollider, platformCollider);
    }
    public void AbleCollision()
    {
        if(platformCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
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
    void Move()
    {
        moveVelocity = Vector2.zero;

        if (states != "dash" && states != "wallJump")
        {
            horizontalInput = moveAction.ReadValue<float>();
            verticalInput = verticalAction.ReadValue<float>();
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
            else if (horizontalInput == 0 && states != "move" && check ==false)
            {
                check = true;
                Sequence sequence = DOTween.Sequence()
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
    public GameObject groundCheck;
    public bool isPlafromCheck = true;
    void FixedUpdate()
    {
        // �ٴ� ���� ����ĳ��Ʈ
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));

        // Ground �±׸� ���� ������Ʈ�� ��Ұ�, ������ ���� ���� ���� ������ ���� Ƚ�� �ʱ�ȭ
        if (hit.collider != null && hit.collider.CompareTag("Ground") && IsGrounded(hit.normal) && isPlafromCheck == true)
        {

            isWallReset = false;
            isGrounded = true;
            jumpsRemaining = maxJumps;

            if(isAttacked == true && rb.velocity.y == 0)
            {
                isAttackedUp = false;
                rb.velocity = new Vector2(0f, 0f);
            }

        }
        else
        {
            isGrounded = false;
        }
        if(states != "dash" && isPlafromCheck == true && isUpLadder == false)
        {
            RaycastHit2D hitWall = Physics2D.Raycast(transform.position, Vector2.right, 1f, LayerMask.GetMask("Ground"));
            RaycastHit2D hitWall2 = Physics2D.Raycast(transform.position, Vector2.left,1f, LayerMask.GetMask("Ground"));
            if (((hitWall.collider != null && hitWall.collider.CompareTag("Ground") && transform.localScale == new Vector3(chInRommSize, chInRommSize, 1)) ||( hitWall2.collider != null && hitWall2.collider.CompareTag("Ground") && transform.localScale == new Vector3(-chInRommSize, chInRommSize, 1)) ) && isGrounded ==false)
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
        // ������ ����Ͽ� ������ ���� ���� ���� �ִ��� Ȯ��
        float dot = Vector2.Dot(normal, Vector2.up);
        return dot >= 0.9f; // ���� ���� 0.9 �̻��̸� ���� ���� ���� �ִٰ� �Ǵ�
    }
    public bool isLadder;
    bool isUpLadder;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Ladder")
        {
            isLadder = true;
            nowLadder = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Ladder")
        {
            isLadder = false;
            nowLadder = collision.gameObject;

            if(isUpLadder == true)
            {
                isUpLadder = false;
                nowLadder = null;
            }
        }
    }
}