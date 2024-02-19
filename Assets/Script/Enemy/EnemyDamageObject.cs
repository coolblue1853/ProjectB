using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class EnemyDamageObject : MonoBehaviour
{
    public bool isDestroyByTime = true;
    public float holdingTime = 0;
    public int damage = 0;
    public float stiffnessTime = 0;   // 경직 시간.
    public float knockForce = 0;
    public Vector2 knockbackDir;
    public bool isNockBackChangeDir;
    public bool isPlayerAttack;
    public GameObject player;
    // Start is called before the first frame update
    public bool isLaunch;
    public float launchForce = 0;
    public Vector2 launchDir;

    public bool isDeletByGround = false;

    private Sequence sequence; // 시퀀스를 저장하기 위한 변수 추가

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (isDestroyByTime)
        {
            DestroyObject();
        }

        if (isLaunch)
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
            sequence.Kill(); // 시퀀스를 중지
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
                    if (player.transform.position.x > transform.position.x)
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



        if (collision.tag == "Ground")
        {
            if (isDeletByGround)
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
