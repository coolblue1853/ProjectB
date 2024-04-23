using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class EnemyConditional : Conditional
{
    public Rigidbody2D body;
    public Animator anim;
    public PlayerController player;
    public EnemyHealth enemyHealth;
    public override void OnAwake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
    }


}
