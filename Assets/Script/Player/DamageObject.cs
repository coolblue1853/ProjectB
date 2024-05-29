using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DamageObject : MonoBehaviour
{

    Transform playerTf;
    public float dmgRatio = 1.0f;
    public float ShakeTime;
    public  bool isSkill = false;
    public bool isDestroyByTime = true;
    [ConditionalHide("isDestroyByTime")]
    public float holdingTime = 0;
    public int hitCount = 1 ; //�ٴ���Ʈ��.
    public int[] damageArr;
    public float stiffnessTime = 0;   // ���� �ð�.
    public float knockForce = 0;
    public Vector2 knockbackDir;
    public bool isNockBackChangeDir;
    public bool isPlayerAttack;
    public GameObject player;
    // Start is called before the first frame update
    public bool isLaunch;
    [ConditionalHide("isLaunch")]
    public float launchForce = 0;
    [ConditionalHide("isLaunch")]
    public Vector2 launchDir;

    public bool isDeletByGround = false;

    public bool isTrackingEnemy = false;
    [ConditionalHide("isTrackingEnemy")]
    public float desiredSpeed;
    private Sequence sequence; // �������� �����ϱ� ���� ���� �߰�
    Rigidbody2D rigidbody2D;
    [ConditionalHide("isTrackingEnemy")]
    public Vector2 boxSize; // Ȯ���� ���簢�� ������ ũ��
    [ConditionalHide("isTrackingEnemy")]
    public float trackingPivot = 0f;

    // ������ʹ� ��ų ���� ����/����� ���� ��ũ��Ʈ.
    public bool isHoldingBuffObject = false;
    [ConditionalHide("isHoldingBuffObject")]
    public string holdingBuffEffect;
    [ConditionalHide("isHoldingBuffObject")]
    public int holdingBuffPower;

    public bool addSuperArmor =false;

    public bool isDashAttack = false;
    [ConditionalHide("isDashAttack")]
    public float dashSpeed = 10;
    [ConditionalHide("isDashAttack")]
    public bool isDashInvins = false;
    [ConditionalHide("isDashAttack")]
    public bool isBackDash = false;
    BoxCollider2D boxCol;
    [ConditionalHide("isHoldingBuffObject")]
    public GameObject buffObject;
    private void Update()
    {

        if (damagedEnemies.Count == maxDamagedEnemies && isLaunch == true)
        {
            Destroy(this.gameObject);
        }
        if (isLaunch && isTrackingEnemy)
        {
            float currentSpeed = rigidbody2D.velocity.magnitude;
            if (currentSpeed != desiredSpeed)
            {
                Vector2 newVelocity = rigidbody2D.velocity.normalized * desiredSpeed;
                rigidbody2D.velocity = newVelocity;
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (player != null)
        {
            // ���� ������Ʈ�� ��ġ���� boxSize�� �������� ���簢�� �׸���
            if (player.transform.localScale.x > 0)
            {
                Gizmos.DrawWireCube(new Vector2(transform.position.x + (trackingPivot), transform.position.y), new Vector3(boxSize.x, boxSize.y, 1));
            }
            else
            {
                Gizmos.DrawWireCube(new Vector2(transform.position.x - (trackingPivot), transform.position.y), new Vector3(boxSize.x, boxSize.y, 1));
            }
        }


    }
    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        playerTf = player.GetComponent<Transform>();
    }

    void BuffOn()
    {
        if (addSuperArmor == true)
        {
            DatabaseManager.isSuperArmor = true;
        }
        if (holdingBuffEffect == "Def")
        {
            DatabaseManager.playerDef += holdingBuffPower;

        }
    }
    void BuffOff()
    {
        if (DatabaseManager.weaponStopMove == true)
        {
            DatabaseManager.weaponStopMove = false;
        }
        if (addSuperArmor == true)
        {
            DatabaseManager.isSuperArmor = false;
        }
        if (holdingBuffEffect == "Def")
        {
            
            DatabaseManager.playerDef -= holdingBuffPower;

        }
    }


    void Start()
    {
        if(buffObject != null)
        {
            AttackManager att = player.GetComponent<AttackManager>();
            GameObject buff = Instantiate(buffObject, Vector2.zero, Quaternion.identity, att.BuffSlot.transform);
        }
        boxCol = this.GetComponent<BoxCollider2D>();
        if (hitCount != 1)
        {
            ResetDamagedEnemies();
        }
        if (isDashAttack)
        {
            PlayerController.instance.SkillDash(holdingTime, dashSpeed, isDashInvins, isBackDash);
        }
        if (isHoldingBuffObject)
        {
            BuffOn();
        }
        rigidbody2D = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");
        if (isDestroyByTime && isHoldingBuffObject == false)
        {
            DestroyObject();
        }

        if (isTrackingEnemy)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
            transform.parent = null;

            GameObject enemy = CheckEnemy();
            Vector3 direction;
            if (enemy != null)
            {
                 direction = (enemy.transform.position - this.transform.position).normalized;

            }
            else
            {
                if (player.transform.position.x > transform.position.x)
                {
                    launchDir.x = -launchDir.x;
                }
                direction = launchDir;
            }
           
            Rigidbody2D rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
            rigidbody2D.AddForce(direction * launchForce, ForceMode2D.Impulse);

            // ������Ʈ�� ���ư��� ������ �������� ȸ���ϱ� ���� ������ ����մϴ�.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // Z���� �������� ȸ���ϱ� ������, Z���� �������� ȸ���ǵ��� ���ʹϾ�(quaternion)�� �����մϴ�.
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            // ȸ���� �����մϴ�.
            transform.rotation = rotation;


        }
        else if (isLaunch)
        {
            transform.parent = null;
            if (player.transform.position.x > transform.position.x)
            {
                launchDir.x = -launchDir.x;
            }
            Rigidbody2D rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
            rigidbody2D.AddForce(launchDir * launchForce, ForceMode2D.Impulse);
        }

    }

    private void OnDestroy()
    {
        // Stop any active DOTween sequences when the object is destroyed
        if (sequence != null)
        {
            sequence.Kill(); // �������� ����
        }
    }

    public void DestroyObject()
    {
        if (isHoldingBuffObject)
        {
            BuffOff();
        }
        sequence = DOTween.Sequence()
            .AppendInterval(holdingTime)
            .AppendCallback(() => Destroy(this.gameObject));
    }

    public int maxDamagedEnemies = 1; // �ִ� �������� ���� ���� ��
    private List<Collider2D> damagedEnemies = new List<Collider2D>(); // �������� ���� ������ ����Ʈ

    void ResetDamagedEnemies()
    {
        if(this.gameObject != null)
        {

            float resetTime = holdingTime / hitCount + 0.02f;

            Sequence seq = DOTween.Sequence()
           .AppendInterval(resetTime)
            .AppendCallback(() => damagedEnemies.Clear())
            .AppendCallback(() => boxCol.enabled =false)
            .AppendInterval(0.01f)
            .AppendCallback(() => boxCol.enabled = true)
           .AppendCallback(() => ResetDamagedEnemies());
        }


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            // �������� ���� ���� ���� �ִ�ġ�� �ʰ����� ���� ��쿡�� ����
            if (damagedEnemies.Count < maxDamagedEnemies)
            {
                // �̹� �������� ���� ������ Ȯ���ϰ�, �������� ������ ���� ��쿡�� ����
                if (!damagedEnemies.Contains(collision))
                {
                    EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();

                    if (isPlayerAttack == true)
                    {
                        if (player.transform.position.x > transform.position.x)
                        {
                            knockbackDir.x = -knockbackDir.x;
                        }
                    }


                    // ������ �������� ������ �������� ���� �� ����Ʈ�� �߰�
                    enemyHealth.damage2Enemy(damageArr, stiffnessTime, knockForce, knockbackDir, this.transform.position.x, isNockBackChangeDir, isSkill, ShakeTime, dmgRatio);
                    if (isPosionAttack)
                    {
                        enemyHealth.CreatPoisonPrefab(poisonDamage, damageInterval, damageCount);
                    }

                    // �������� ���� �� ����Ʈ�� �߰�
                    damagedEnemies.Add(collision);
                }
            }
        }


        else if (collision.tag == "Ground")
        {
            if (isDeletByGround)
            {
                Destroy(this.gameObject);
            }
        }
    }



    public void SetDamge(int[] damgeArray)
    {
        damageArr = damgeArray;
        //this.gameObject.SetActive(true);
    }

    public bool isPosionAttack;
    public int poisonDamage;
    public float damageInterval;
    public int damageCount;


    public LayerMask layerMask; // ������ ���̾�
    public string enemyTag = "Enemy"; // �÷��̾� �±�

    public GameObject CheckEnemy()
    {

        // �ֺ��� �ִ� ��� Collider�� ������
        Collider2D[] colliders;

        if (player.transform.localScale.x > 0)
        {
             colliders = Physics2D.OverlapBoxAll(new Vector2(transform.position.x + (trackingPivot), transform.position.y), boxSize, 0f, layerMask);
        }
        else
        {
              colliders = Physics2D.OverlapBoxAll(new Vector2(transform.position.x - (trackingPivot), transform.position.y), boxSize, 0f, layerMask);
        }
        // ������ Collider�� ��ȸ�ϸ鼭 Player �±׸� ���� ������Ʈ�� �ִ��� Ȯ��
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(enemyTag))
            {
                // Player �±׸� ���� ������Ʈ�� ������ Success ��ȯ
                return  col.gameObject;
            }
        }

        // �ֺ��� Player �±׸� ���� ������Ʈ�� ������ Failure ��ȯ
        return null;
    }



}
