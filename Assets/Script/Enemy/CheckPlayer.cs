using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayer : MonoBehaviour
{
    public EnemyFSM enemyFSM;
    CircleCollider2D circle;
    public bool isPreemptive = true; // 선제공격인지 아닌지. 아니라면 맞았을때에만 플레이어를 추적하게 된다.
    public bool isPlayerCheckActive = false;
    private Animator animator;
    EnemyHealth enemyHealth;
    void Start()
    {
        enemyHealth = transform.parent.parent.GetComponent<EnemyHealth>();
        animator =transform.parent.parent.GetComponent<Animator>();
        ResetBoolParameters();
    }

    public void ResetBoolParameters()
    {
        // 애니메이터에 등록된 모든 파라미터 가져오기
        AnimatorControllerParameter[] parameters = animator.parameters;

        // 모든 불리언 파라미터의 값을 false로 설정
        foreach (AnimatorControllerParameter parameter in parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool)
            {
                Debug.Log(parameter.name);
                animator.SetBool(parameter.name, false);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && enemyHealth.isStun == false)
        {
            Debug.Log("들어옴!");
         //   ResetBoolParameters();
            enemyFSM.KillBrainSequence();
            enemyFSM.StateChanger("Chase");
            enemyFSM.ReActiveBrainSequence();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" && enemyHealth.isStun == false)
        {
          ResetBoolParameters();
               enemyFSM.KillBrainSequence();
              enemyFSM.StateChanger("Stay");
              enemyFSM.ReActiveBrainSequence();

        }
    }
}
