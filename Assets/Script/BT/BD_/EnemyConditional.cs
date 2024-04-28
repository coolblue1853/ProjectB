using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class EnemyConditional : Conditional
{
    public GameObject enemyObject;
    public float chInRommSize = 0;
    public Rigidbody2D body;
    public Animator anim;
    public PlayerController player;
    public EnemyHealth enemyHealth;
    public BehaviorTree behaviorTree;
    public override void OnAwake()
    {
        enemyObject = this.gameObject;
        chInRommSize = enemyObject.transform.localScale.x;
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        behaviorTree = GetComponent<BehaviorTree>();
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
