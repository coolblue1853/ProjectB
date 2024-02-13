using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class DamageObject : MonoBehaviour
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

    private void DestroyObject()
    {
        Sequence sequence = DOTween.Sequence()
        .AppendInterval(holdingTime)
        .AppendCallback(() => Destroy(this.gameObject));
    }

    private Dictionary<Collider2D, bool> damagedEnemies = new Dictionary<Collider2D, bool>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            if (!damagedEnemies.ContainsKey(collision) || !damagedEnemies[collision])
            {
                EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();

                if (isPlayerAttack == true)
                {
                    if (player.transform.position.x > transform.position.x)
                    {
                        knockbackDir.x = -knockbackDir.x;
                    }
                }

                enemyHealth.damage2Enemy(damage, stiffnessTime, knockForce, knockbackDir, this.transform.position.x, isNockBackChangeDir);
                if (isPosionAttack)
                {
                    enemyHealth.CreatPoisonPrefab(poisonDamage, damageInterval, damageCount);
                }

                // Mark the enemy as damaged
                damagedEnemies[collision] = true;
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
