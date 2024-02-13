using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosionAttack : MonoBehaviour
{
    int damage = 0;
    float Interval;
    int count;
    public EnemyHealth enemyHealth;



    public void ActivePoison(int poisonDamage, float damageInterval, int damageCount)
    {
        damage = poisonDamage;
        Interval = damageInterval;
        count = damageCount;

        
        if(count == 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            enemyHealth.damage2Enemy(damage, 0, 0, new Vector2(0, 0), 0, false);
            count -= 1;
        }

        Invoke("ActivePosionIn", Interval);
    }

    void ActivePosionIn()
    {

        if (count == 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            enemyHealth.damage2Enemy(damage, 0, 0, new Vector2(0, 0), 0, false);
            count -= 1;
        }

        Invoke("ActivePosionIn", Interval);

    }
}
