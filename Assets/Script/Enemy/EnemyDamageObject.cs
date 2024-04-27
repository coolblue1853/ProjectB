using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class EnemyDamageObject : MonoBehaviour
{
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
    private void FixedUpdate()
    {
        Vector3 deltaPos = transform.position - prevPosition;                     // ������ġ - ������ġ = ����

        float angle = Mathf.Atan2(deltaPos.y, deltaPos.x) * Mathf.Rad2Deg;// �ﰢ�Լ��� ������ ����.



        if (0 != angle)    // ��������� ������������ ���̸� ���ؼ� üũ
        {

            transform.rotation = Quaternion.Euler(0, 0, angle);

            prevPosition = transform.position;
        }
    }
    public bool isGravityFall = false;
    void LookAtDirection(Vector2 direction)
    {
        // źȯ�� �������� ȸ��
        transform.up = direction.normalized;
    }
    private void Update()
    {
        if (isLaunch && isGravityFall ==false)
        {
            float currentSpeed = rigidbody2D.velocity.magnitude;
            if (currentSpeed != desiredSpeed)
            {
                Vector2 newVelocity = rigidbody2D.velocity.normalized * desiredSpeed;
                rigidbody2D.velocity = newVelocity;
            }
        }


    }
    void Start()
    {
        if (isRandForce)
        {
            launchForce = Random.Range(minForce, maxForce);
        }
        prevPosition = transform.position;
        rigidbody2D = GetComponent<Rigidbody2D>();
        enemyOb = transform.parent.gameObject;
        player = GameObject.FindWithTag("Player");
        if (isDestroyByTime)
        {
            DestroyObject();
        }

        
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
                this.transform.parent = null;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
                transform.parent = null;
                Vector3 direction = (player.transform.position - this.transform.position).normalized;
                Rigidbody2D rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
                rigidbody2D.AddForce(direction * launchForce, ForceMode2D.Impulse);

                // ������Ʈ�� ���ư��� ������ �������� ȸ���ϱ� ���� ������ ����մϴ�.
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                // Z���� �������� ȸ���ϱ� ������, Z���� �������� ȸ���ǵ��� ���ʹϾ�(quaternion)�� �����մϴ�.
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                // ȸ���� �����մϴ�.
                transform.rotation = rotation;
            }



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

    private void DestroyObject()
    {
        sequence = DOTween.Sequence()
            .AppendInterval(holdingTime)
            .AppendCallback(() => Destroy(this.gameObject));
    }

    private Dictionary<Collider2D, bool> damagedPlayer = new Dictionary<Collider2D, bool>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (!damagedPlayer.ContainsKey(collision) || !damagedPlayer[collision])
            {
                PlayerHealthManager playerHealth = collision.transform.GetChild(0).GetComponent<PlayerHealthManager>();

                if (isPlayerAttack == true)
                {

                    if (player.transform.position.x < enemyOb.transform.position.x )
                    {
                        knockbackDir.x = -knockbackDir.x;
                    }


                }

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



        if (collision.tag == "Ground" )
        {
            if (isGroundDestroy== true)
            {
                Destroy(this.gameObject);
            }
        }
    
    }

    public bool isPosionAttack;
    public int poisonDamage;
    public float damageInterval;
    public int damageCount;



}
