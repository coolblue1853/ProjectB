using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DarkTonic.MasterAudio;
public class EnemyDamageObject : PoolAble
{
    public string attackSound;
    public bool isDestroyByTime = true;
    public float holdingTime = 0;
    public int damage = 0;
    public float stiffnessTime = 0;   // ���� �ð�.
    public float knockForce = 0;
    public Vector2 knockbackDir;
    public bool isNockBackChangeDir;
    public bool isPlayerAttack;
    public GameObject player;
    // Start is called before the first frame update
    public bool isLaunch;
    public bool isNonTrakingPlayer =false;
    public float launchForce = 0;
    public Vector2 launchDir;
    public bool isGroundDestroy = false;
    float desiredSpeed = 5f; // ���ϴ� �ӵ� ����

    private Vector3 prevPosition;
    private Sequence sequence; // �������� �����ϱ� ���� ���� �߰�
     GameObject enemyOb;
    Rigidbody2D rigidbody2D;
    public bool isRandForce;
    public float maxForce;
    public float minForce;
    bool isEndInit = false;

    private void FixedUpdate()
    {
        if (isLaunch && isGravityFall == true)
        {
            Vector3 deltaPos = transform.position - prevPosition;                     // ������ġ - ������ġ = ����
            float angle = Mathf.Atan2(deltaPos.y, deltaPos.x) * Mathf.Rad2Deg;// �ﰢ�Լ��� ������ ����.
            if (0 != angle)    // ��������� ������������ ���̸� ���ؼ� üũ
            {

                transform.rotation = Quaternion.Euler(0, 0, angle);
                prevPosition = transform.position;
            }
        }
    }
    public bool isGravityFall = false;

    private void Update()
    {
        if (isLaunch && isGravityFall ==false && isEndInit == true)
        {
            float currentSpeed = rigidbody2D.velocity.magnitude;
            if (currentSpeed != desiredSpeed)
            {
                Vector2 newVelocity = rigidbody2D.velocity.normalized * desiredSpeed;
                rigidbody2D.velocity = newVelocity;
            }
        }


    }
    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        rigidbody2D = GetComponent<Rigidbody2D>();
    }
    private void OnEnable()
    {
        isEndInit = false;

        DOTween.Kill(this.gameObject);

        // �߻� ���� �浹 �����ϰ� ����
        if (rigidbody2D != null)
        {
            rigidbody2D.velocity = Vector2.zero;
            rigidbody2D.angularVelocity = 0f;
        }

        DOTween.Kill(this.gameObject);  // Ʈ�� ����
        transform.rotation = Quaternion.identity;
        damagedEnemies.Clear();
        damagedPlayer.Clear();
        if (isRandForce)
        {
            launchForce = Random.Range(minForce, maxForce);
        }
        prevPosition = transform.position;  

        if (transform.parent != null)
            enemyOb = transform.parent.gameObject;

        if (isDestroyByTime)
        {
            DestroyObject();
        }


       
    }
    public void LaunchObject()
    {
        if (isLaunch)
        {
            if (isNonTrakingPlayer)
            {
                this.transform.parent = null;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
                transform.parent = null;
                Vector3 direction = transform.up;
                Rigidbody2D rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
                rigidbody2D.AddForce(direction * launchForce, ForceMode2D.Impulse);

                // ������Ʈ�� ���ư��� ������ �������� ȸ���ϱ� ���� ������ ����մϴ�.
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                // Z���� �������� ȸ���ϱ� ������, Z���� �������� ȸ���ǵ��� ���ʹϾ�(quaternion)�� �����մϴ�.
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                // ȸ���� �����մϴ�.
                transform.rotation = rotation;
            }
            else
            {

                this.transform.SetParent(null);
                Vector3 direction = (player.transform.position - this.transform.position).normalized;
                Rigidbody2D rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
                rigidbody2D.AddForce(direction * launchForce, ForceMode2D.Impulse);

                // ������Ʈ�� ���ư��� ������ �������� ȸ���ϱ� ���� ������ ����մϴ�.
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                // Z���� �������� ȸ���ϱ� ������, Z���� �������� ȸ���ǵ��� ���ʹϾ�(quaternion)�� �����մϴ�.
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                // ȸ���� �����մϴ�.
                transform.rotation = rotation;
                isEndInit = true;

            }



        }
    }
    void Start()
    {

  
    }

    private void OnDestroy()
    {
        // Stop any active DOTween sequences when the object is destroyed
        if (sequence != null)
        {
            sequence.Kill(); // �������� ����
        }
    }

    private void DestroyObject()
    {
        sequence = DOTween.Sequence()
            .AppendInterval(holdingTime)
            .AppendCallback(() => transform.parent = null)
            .AppendCallback(() => ReleaseObject());
    }

    private Dictionary<Collider2D, bool> damagedPlayer = new Dictionary<Collider2D, bool>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && isApcAttack == false)
        {
            if (!damagedPlayer.ContainsKey(collision) || !damagedPlayer[collision])
            {
                PlayerHealthManager playerHealth = collision.transform.GetChild(0).GetComponent<PlayerHealthManager>();

                if (isPlayerAttack == true && enemyOb !=null)
                {

                    if (collision.gameObject.transform.position.x < enemyOb.transform.position.x )
                    {
                        knockbackDir.x = - Mathf.Abs( knockbackDir.x);
                    }


                }
                if (attackSound != null)
                    MasterAudio.PlaySound(attackSound);
                playerHealth.damage2Player(damage, stiffnessTime, knockForce, knockbackDir, this.transform.position.x, isNockBackChangeDir);

                /*
                if (isPosionAttack)
                {
                    enemyHealth.CreatPoisonPrefab(poisonDamage, damageInterval, damageCount);
                }
                */
                // Mark the enemy as damaged
                damagedPlayer[collision] = true;
            }
        }




        if (collision.tag == "Enemy" && isApcAttack == true)
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
                        if (collision.gameObject.transform.position.x < transform.position.x)
                        {
                            knockbackDir.x = -Mathf.Abs(knockbackDir.x);
                        }
                    }
                    if (attackSound != null)
                        MasterAudio.PlaySound(attackSound);

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
        if (collision.tag == "APC" && isApcAttack == false)
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
                        if (collision.gameObject.transform.position.x < transform.position.x)
                        {
                            knockbackDir.x = -Mathf.Abs(knockbackDir.x);
                        }
                    }

                    int[] damageArr = { damage, damage,0,0,0,0,0,0,0,0 };
                    // ������ �������� ������ �������� ���� �� ����Ʈ�� �߰�
                    enemyHealth.damage2Enemy(damageArr, stiffnessTime, knockForce, knockbackDir, this.transform.position.x, isNockBackChangeDir, isSkill, ShakeTime+0.2f, dmgRatio);
                    if (isPosionAttack)
                    {
                        enemyHealth.CreatPoisonPrefab(poisonDamage, damageInterval, damageCount);
                    }

                    // �������� ���� �� ����Ʈ�� �߰�
                    damagedEnemies.Add(collision);
                }
            }
        }
        if (collision.tag == "Ground" )
        {
            if (isGroundDestroy== true)
            {
                ReleaseObject();
            }
        }
    
    }

    public bool isPosionAttack;
    public int poisonDamage;
    public float damageInterval;
    public int damageCount;

    public bool isApcAttack = false;
    public int maxDamagedEnemies = 1; // �ִ� �������� ���� ���� ��
    private List<Collider2D> damagedEnemies = new List<Collider2D>(); // �������� ���� ������ ����Ʈ
    public int[] damageArr;
    public float dmgRatio = 1.0f;
    public float ShakeTime;
    public bool isSkill = false;

}
