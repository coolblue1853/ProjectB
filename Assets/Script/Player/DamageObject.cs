using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class DamageObject : MonoBehaviour
{
    public float holdingTime = 0;
    public int damage = 0;
    public float stiffnessTime = 0;   // 경직 시간.
    public float knockForce = 0;
    public Vector2 knockbackDir;
    public bool isNockBackChangeDir;
    public bool isPlayerAttack;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        DestroyObject();
    }

    private void DestroyObject()
    {
        Sequence sequence = DOTween.Sequence()
        .AppendInterval(holdingTime)
        .AppendCallback(() => Destroy(this.gameObject));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();

            if (isPlayerAttack == true)
            {
                if (player.transform.position.x > transform.position.x)
                {
                    knockbackDir.x = -knockbackDir.x;
                }


            }
            enemyHealth.damage2Enemy(damage, stiffnessTime, knockForce, knockbackDir,this.transform.position.x, isNockBackChangeDir);
            if (isPosionAttack)
            {
                enemyHealth.CreatPoisonPrefab(poisonDamage, damageInterval, damageCount);
            }
        }
    }

    public bool isPosionAttack;
    public int poisonDamage;
    public float damageInterval;
    public int damageCount;



}
