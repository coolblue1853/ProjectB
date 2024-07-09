using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DamageNumbersPro;
public class BleedingAttack : MonoBehaviour
{
    public DamageNumber damageNumber;
    int damage = 0;
    float Interval;
    int count;
    public EnemyHealth enemyHealth;

    public void ActivePoison(int poisonDamage, float damageInterval, int damageCount)
    {
        enemyHealth.isBleeding = true;
        damage = poisonDamage;
        Interval = damageInterval;
        count = damageCount;

        
        if(count == 0)
        {
            enemyHealth.isBleeding = false;
            Destroy(this.gameObject);
        }
        else
        {
            enemyHealth.onlyDamage2Enemy(damage);
            damageNumber.Spawn(transform.position + Vector3.up, (int)damage);
            count -= 1;
        }

        Invoke("ActivePosionIn", Interval);
    }

    void ActivePosionIn()
    {

        if (count == 0)
        {
            enemyHealth.isBleeding = false;
            Destroy(this.gameObject);
        }
        else
        {
            enemyHealth.onlyDamage2Enemy(damage);
            damageNumber.Spawn(transform.position + Vector3.up, (int)damage);
            count -= 1;
        }

        Invoke("ActivePosionIn", Interval);

    }
}
