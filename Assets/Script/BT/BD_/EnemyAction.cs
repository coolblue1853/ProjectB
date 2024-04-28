using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class EnemyAction : Action
{
    BehaviorTree behaviorTree;
    public Rigidbody2D body;
    public Animator anim;
    public PlayerController player;
    public Vector2 originPosition;
    public GameObject enemyObject;
    public static DG.Tweening.Sequence sequence;
    public bool isEnd;
    public float chInRommSize = 0;

    public override void OnAwake()
    {
        behaviorTree = GetComponent<BehaviorTree>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
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
    public void StopAllActions()
    {
        // GetActiveTasks �޼��尡 null�� ��ȯ�� �� �����Ƿ� ���� ó��
        List<BehaviorDesigner.Runtime.Tasks.Task> activeTasks = behaviorTree.GetActiveTasks();
        if (activeTasks != null)
        {
            // Behavior Tree���� ���� ���� ��� Task�� ã�Ƽ� ����
            foreach (BehaviorDesigner.Runtime.Tasks.Task task in activeTasks)
            {
                if (task is EnemyAction)
                {
                    EnemyAction enemyAction = (EnemyAction)task;
                    if (enemyAction != null)
                    {
                        enemyAction.StopAction();
                    }
                }
            }
        }
    }


}
