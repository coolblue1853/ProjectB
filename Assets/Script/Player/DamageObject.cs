using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class DamageObject : MonoBehaviour
{
    public float holdingTime = 0;
    public int damage = 0;
    public float stiffnessTime = 0;   // 공격 주기, 짧을수록 더 빠르게 공격 가능.
    public float knockForce = 0;
    public Vector2 knockbackDir;
    // Start is called before the first frame update
    void Start()
    {
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
         
            enemyHealth.damage2Enemy(damage, stiffnessTime, knockForce, knockbackDir,this.transform.position.x);
        }
    }

}
