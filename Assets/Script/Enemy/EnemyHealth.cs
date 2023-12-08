using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHP = 0;
    public float nowHP = 0;



    public void damage2Enemy(float damage)
    {
        nowHP -= damage;
        if(nowHP <= 0)
        {
            Destroy(this.gameObject);
        }
    }


}
