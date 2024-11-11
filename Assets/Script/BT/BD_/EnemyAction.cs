using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
public class EnemyAction : Action
{

    public BehaviorTree behaviorTree;
    public Rigidbody2D body;
    public Animator anim;
    public PlayerController player;
    public Vector2 originPosition;
    public GameObject enemyObject;
    public DG.Tweening.Sequence sequence;
    public bool isEnd;
    public float tfLocalScale = 0;

    public override void OnAwake()
    {
        // GetComponent ȣ���� �ּ�ȭ�Ͽ� ���� ĳ��
        behaviorTree = GetComponent<BehaviorTree>();
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Singleton ���� ��� �� PlayerController �ν��Ͻ� ĳ��
        player = PlayerController.instance;
        enemyObject = gameObject;

        // transform.localScale ���� ������ �ʴ´ٸ� ĳ��
        tfLocalScale = enemyObject.transform.localScale.x;

        // �ʱ� ��ġ ĳ��
        originPosition = transform.position;
    }

    public virtual void StopAction()
    {
        if (sequence != null && sequence.IsActive())
        {
            isEnd = true;
            sequence.Kill();
            sequence = null; // GC�� ������ �� �ֵ��� ����
        }
    }

    public void StartAction()
    {
        // �ʿ�� Sequence �ʱ�ȭ �� ���� ó��
        if (sequence == null || !sequence.IsActive())
        {
            sequence = DOTween.Sequence();

            // �ִϸ��̼� ���� �߰� �� ��ũ ����� �ڵ� ���� ����
            sequence.SetLink(enemyObject);
        }
    }

}
