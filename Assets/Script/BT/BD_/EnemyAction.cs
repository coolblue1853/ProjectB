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
        // GetComponent 호출을 최소화하여 참조 캐싱
        behaviorTree = GetComponent<BehaviorTree>();
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Singleton 패턴 사용 시 PlayerController 인스턴스 캐싱
        player = PlayerController.instance;
        enemyObject = gameObject;

        // transform.localScale 값이 변하지 않는다면 캐싱
        tfLocalScale = enemyObject.transform.localScale.x;

        // 초기 위치 캐싱
        originPosition = transform.position;
    }

    public virtual void StopAction()
    {
        if (sequence != null && sequence.IsActive())
        {
            isEnd = true;
            sequence.Kill();
            sequence = null; // GC가 수집할 수 있도록 설정
        }
    }

    public void StartAction()
    {
        // 필요시 Sequence 초기화 및 재사용 처리
        if (sequence == null || !sequence.IsActive())
        {
            sequence = DOTween.Sequence();

            // 애니메이션 설정 추가 및 링크 연결로 자동 해제 관리
            sequence.SetLink(enemyObject);
        }
    }

}
