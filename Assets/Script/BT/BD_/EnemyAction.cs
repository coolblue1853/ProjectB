using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class EnemyAction : Action
{
    public Rigidbody2D body;
    public Animator anim;
    public PlayerController player;
    public Vector2 originPosition;
    public GameObject enemyObject;
    public DG.Tweening.Sequence sequence;
    public bool isEnd;
    public float chInRommSize = 0;

    public override void OnAwake()
    {
        enemyObject = this.gameObject;
        chInRommSize = enemyObject.transform.localScale.x;
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        originPosition = new Vector2(this.transform.position.x, this.transform.position.y);
    }

    public virtual void StopAction()
    {
        if (sequence != null && sequence.IsActive())
        {  
            isEnd = true;
            sequence.Kill();
        }
    }
}
