using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using AnyPortrait;
using DarkTonic.MasterAudio;
public class PlayerController : MonoBehaviour
{
    int LadderLayer = 31;
    float chInRommSize = 1.5f;
    public float runSpeed = 10f;  // �̵� �ӵ�
    public int runStemina; // �޸���� ����ϴ� ���׹̳�
    public float intervalRunStemina; // �޸���� ����ϴ� ���׹̳�
    public float moveSpeed = 10f;  // �̵� �ӵ�
    public float jumpForce = 13f; // ���� ��
    public float dashSpeed = 20f; // �뽬 �ӵ�
    public float dashDuration = 0.2f; // �뽬 ���� �ð�
    public float dashWait = 2f; // �뽬 Ÿ�̸�
    private float dashTimer; // �뽬 Ÿ�̸�
    public int dashStemina; // �뽬�� ����ϴ� ���׹̳�
    public int maxJumps = 2;      // �ִ� ���� Ƚ��
    public int jumpsRemaining;    // ���� ���� Ƚ��
    private bool isChangeDirection = false;    // ���� ���� Ƚ��
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

    // �ִϸ��̼�
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
        bc = this.GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumps;
    }
    bool once = false;
    void Update()
    {
       if (verticalInput == 0 && states != "move" && isUpLadder ==true && isAttackAnim == false)
        {
            mainCharacter.Play("RopeStay");

        }
        if (horizontalInput == 0&& isUpLadder == false)
        {
            if (isJumpAnim == false && isUpLadder == false && states != "dash" && isAttackAnim == false)
            {
                mainCharacter.Play("Idle");
            }
            if (isJumpAnim == false && isUpLadder == true && states != "dash" && isAttackAnim == false)
            {
                mainCharacter.Play("RopeStay");
            }

        }
        if (isJumpAnim == false && isUpLadder == false && states != "dash")
        {
            rb.gravityScale = 3f;

        }
        if (rb.velocity.y ==0 && isJumpAnim == true && states != "dash")
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
            if (rb.velocity != Vector2.zero && DatabaseManager.weaponStopMove == true)// isGround�� ������ 
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
            if (DatabaseManager.weaponStopMove == false && isUpLadder == false&& states != "dash")
            {

                Move();
            }


            // �뽬
            if (dashAction.triggered && dashTimer <= 0f && PlayerHealthManager.Instance.nowStemina > dashStemina)//&& DatabaseManager.weaponStopMove == false 
            {
                DatabaseManager.weaponStopMove = false;
                Debug.Log("�뽬�۵�");
                rb.gravityScale =3;
                isUpLadder = false;
                ChangeLadderLayerOrder();

                Dash();
            }



            // ����
            if (jumpAction.triggered && DatabaseManager.weaponStopMove == false)
            {
                if (verticalInput < -0.5f && currentOneWayPlatform != null)
                {
                  DisableCollision();
                }
                else if (isWall == true)
                {
                 //   WallJump();
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

            // �뽬 Ÿ�̸� ������Ʈ
            if (dashTimer > 0f)
            {
                dashTimer -= Time.deltaTime;
            }
        }

        if (states != "dash" && states != "wallJump" && isLadder == true && DatabaseManager.checkAttackLadder == false)
        {
            if(upAction.triggered == true|| downAction.triggered)
            {

                jumpsRemaining = maxJumps;
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0;
                isUpLadder = true;

                // ���� ���� ������Ű��
                Vector2 newPosition = new Vector2(nowLadder.transform.position.x, transform.position.y);
                
                //���� �ִϸ��̼� ���, ���̾� ���� ����
                SpriteRenderer sR = nowLadder.GetComponent<SpriteRenderer>();
                SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
                sR.sortingOrder = playerSR.sortingOrder + 1;

                // ���� ��ġ�� ���� ��ġ ������ �Ÿ� ���
                Vector2 distance = newPosition - (Vector2)transform.position;
                BoxCollider2D bc = this.GetComponent<BoxCollider2D>();
                boxColliderTrue = true;
                // Rigidbody �̵����� ���� �̵� ����
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
            horizontalInput = moveAction.ReadValue<float>();
            verticalInput = verticalAction.ReadValue<float>();
            if (verticalInput < -0.2f && isAttackAnim == false)
            {
                states = "moveDown";
                mainCharacter.Play("RopeDown");
                moveVelocity = Vector2.down * (isRun ? runSpeed * (1 + (DatabaseManager.SpeedBuff / 100)) : moveSpeed * (1 + (DatabaseManager.SpeedBuff / 100)));
            }
            else if (verticalInput > 0.2f && isAttackAnim == false)
            {

                Debug.Log("�ö󰡴���");
                states = "moveUp";
                mainCharacter.Play("RopeUp");
                moveVelocity = Vector2.up * (isRun ? runSpeed * (1 + (DatabaseManager.SpeedBuff / 100)) : moveSpeed * (1 + (DatabaseManager.SpeedBuff / 100)));
            }
            else if (verticalInput == 0 && states != "move" && check == false && isAttackAnim == false)
            {
                mainCharacter.Play("RopeStay");
                check = true;
                Sequence sequence = DOTween.Sequence()
                    .AppendInterval(0.3f)
                    .AppendCallback(() => VerticalRunChecker());

            }
            // Applying velocity to the Rigidbody2D
            rb.velocity = new Vector2(rb.velocity.x, moveVelocity.y);


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
        //    mainCharacter.Play("Jump");
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
                        mainCharacter.Play("Run");

                        }
                        else
                    {

                        PlayWalkSound();
                        mainCharacter.Play("Walk");
                        }
                    }


                states = "moveLeft";
                moveVelocity = Vector2.left * (isRun ? runSpeed * (1 + (DatabaseManager.SpeedBuff / 100)) : moveSpeed * (1 + (DatabaseManager.SpeedBuff / 100)));
                transform.localScale = new Vector3(-chInRommSize, chInRommSize, 1);
            }
            else if (horizontalInput > 0)
            {
                if (isJumpAnim == false && isAttackAnim == false)
                {
                    if (isRun)
                    {
                        mainCharacter.Play("Run");
                        PlayRunSound();
                    }
                    else
                    {
                        PlayWalkSound();
  
               
                        mainCharacter.Play("Walk");
                    }
                }

                states = "moveRight";
                moveVelocity = Vector2.right * (isRun ? runSpeed * (1 + (DatabaseManager.SpeedBuff / 100)) : moveSpeed * (1 + (DatabaseManager.SpeedBuff / 100)));
                transform.localScale = new Vector3(chInRommSize, chInRommSize, 1);
            }
            else if (horizontalInput == 0 && states != "move" && check ==false)
            {
                if (isJumpAnim == false && isUpLadder == false&& states != "dash" && isAttackAnim == false)
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
                mainCharacter.Play("RopeStay");
            }
        }
    }
    Sequence dashSequence;
    public float dashMovePoint;
    void Dash()
    {
        BoxCollider2D bc = this.GetComponent<BoxCollider2D>();
        if (isAttackAnim == false)
        {
            mainCharacter.Play("Dash");

        }
        DatabaseManager.isInvincibility = true;
        Invoke("EndInvincible", dashDuration);
        boxColliderTrue = false;
        PlayerHealthManager.Instance.SteminaDown(dashStemina);
        states = "dash";


        rb.gravityScale = 0f;
        // �뽬 �ӵ��θ� �̵��ϵ��� ����
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0f);
        // �뽬 Ÿ�̸� ����
        dashTimer = dashWait;

        moveSequence.Kill();
        dashSequence.Kill();
        dashSequence = DOTween.Sequence()
        .AppendInterval(dashDuration) // 2�� ���
        .AppendCallback(() => rb.gravityScale = 3f)
        .AppendCallback(() => rb.velocity = new Vector2(0f, 0f))
        .AppendCallback(() => states = "move");
    }

    public void SkillDash(float dashDu, float dashSpd, bool isDashInvins, bool isBackDash)
    {
        DatabaseManager.weaponStopMove = false;
        states = "dash";
        Debug.Log("�뽬����");
        if(isDashInvins == true)
        {
            DatabaseManager.isInvincibility = true;
        }

        DatabaseManager.isSuperArmor = true;
        Invoke("EndSuperArmor", dashDu);
        boxColliderTrue = false;
        rb.gravityScale = 0f;
        // �뽬 �ӵ��θ� �̵��ϵ��� ����
        if(isBackDash == true)
        {
            rb.velocity = new Vector2(transform.localScale.x * -dashSpd, 0f);
        }
        else
        {
            rb.velocity = new Vector2(transform.localScale.x * dashSpd, 0f);
        }

        moveSequence.Kill();
        dashSequence.Kill();
        dashSequence = DOTween.Sequence()
        .AppendInterval(dashDu) // 2�� ���
        .AppendCallback(() => rb.gravityScale = 3f)
        .AppendCallback(() => rb.velocity = new Vector2(0f, 0f))
         .AppendCallback(() => EndInvincibility(isDashInvins))
        .AppendCallback(() => states = "move");
    }

    void EndInvincibility(bool isDashInvins)
    {
        if (isDashInvins == true) DatabaseManager.isInvincibility = false;
    }

    void EndSuperArmor()
    {
        DatabaseManager.isSuperArmor = false;
    }
    void EndInvincible()
    {
        DatabaseManager.isInvincibility = false;
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
        .AppendInterval(0.4f) // 2�� ���
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
            mainCharacter.Play("Jump");
        }


        boxColliderTrue = false;
        isWallReset = false;
        states = "jump";
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpsRemaining--;
        isGround = false;
        // ���� �Ŀ��� isGrounded�� false�� �����Ͽ� FixedUpdate���� �ٽ� üũ�� �� �ֵ��� �մϴ�.
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

        // �ٴ� ���� ����ĳ��Ʈ

        if (((hit1.collider != null && hit1.collider.CompareTag("Ground") && IsGrounded(hit1.normal)) ||( hit2.collider != null && hit2.collider.CompareTag("Ground") && IsGrounded(hit2.normal))) && isOnGround == true && isPlafromCheck == true)
        {

            isOnGround = false;
            jumpsRemaining = maxJumps;
            if (isJumpAnim == true && isUpLadder == false && states != "dash" && isAttackAnim ==false)
            {
                isJumpAnim = false;
                mainCharacter.Play("Idle");

            }


        }

        // Ground �±׸� ���� ������Ʈ�� ��Ұ�, ������ ���� ���� ���� ������ ���� Ƚ�� �ʱ�ȭ
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
        // ������ ����Ͽ� ������ ���� ���� ���� �ִ��� Ȯ��
        float dot = Vector2.Dot(normal, Vector2.up);
        return dot >= 0.9f; // ���� ���� 0.9 �̻��̸� ���� ���� ���� �ִٰ� �Ǵ�
    }
    public bool isLadder;
    public bool isUpLadder;


    bool isAttackAnim = false;
    Sequence attackAnimSequence;
    public void ActiveAttackAnim(string anim, float time)
    {
        if(anim == null || anim == "")
        {
            return;
        }
        if(isJumpAnim == true)
        {
            isJumpAnim = false;
        }
        mainCharacter.Play(anim);
        isAttackAnim = true;
        if(isUpLadder == true)
        {
            Debug.Log("���ݽ���");
            isRopeAttack = true;
        }

        attackAnimSequence.Kill();
        attackAnimSequence = DOTween.Sequence()
       .AppendInterval(time)
       .AppendCallback(() => isAttackAnim = false)
       .AppendCallback(() => EndRopeAttackAnim());


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
            mainCharacter.Play("RopeStay");
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
                if (isJumpAnim == true && isUpLadder == false && rb.velocity.y == 0 && states != "dash" && isAttackAnim == false)
                {
                    isJumpAnim = false;
                    mainCharacter.Play("Idle");

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
                    Debug.Log("���ݳ�");
                    isRopeAttack = false;
                    mainCharacter.Play("RopeStay");
                }
            }
            if (collision.tag == "InGroundPlayer")
            {
                Debug.Log("����");
                platformTrue = false;
            }
        

    }

    void PlayRunSound()
    {

            if (!MasterAudio.IsSoundGroupPlaying("SoilRun"))
        {
            MasterAudio.PlaySound3DAtTransform("SoilRun", this.transform,1f,1.6f);
        }
    }
    void PlayWalkSound()
    {
        if (!MasterAudio.IsSoundGroupPlaying("SoilWalk"))
        {
            MasterAudio.PlaySound3DAtTransform("SoilWalk", this.transform);
        }
    }

}