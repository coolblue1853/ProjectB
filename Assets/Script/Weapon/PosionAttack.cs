using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DamageNumbersPro;
using DarkTonic.MasterAudio;
public class PosionAttack : MonoBehaviour
{
    public DamageNumber damageNumber;
    int damage = 0;
    float Interval;
    int count;
    public EnemyHealth enemyHealth;

    public bool isBleeding = false;

    public void ActivePoison(int poisonDamage, float damageInterval, int damageCount)
    {
        damage = poisonDamage + DatabaseManager.addPoisonDmg;
        Interval = damageInterval;
        count = damageCount;

        
        if(count == 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            enemyHealth.onlyDamage2Enemy(damage);
            if (!MasterAudio.IsSoundGroupPlaying("Poison"))
            {
                MasterAudio.PlaySound("Poison");
            }
            damageNumber.Spawn(transform.position + Vector3.up, (int)damage);
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
            enemyHealth.onlyDamage2Enemy(damage);
            if (!MasterAudio.IsSoundGroupPlaying("Poison"))
            {
                MasterAudio.PlaySound("Poison");
            }
            damageNumber.Spawn(transform.position + Vector3.up, (int)damage);
            count -= 1;
        }

        Invoke("ActivePosionIn", Interval);

    }
}
