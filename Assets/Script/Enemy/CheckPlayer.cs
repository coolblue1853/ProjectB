using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayer : MonoBehaviour
{
    public EnemyFSM enemyFSM;
    CircleCollider2D circle;
    public bool isPreemptive = true; // ������������ �ƴ���. �ƴ϶�� �¾��������� �÷��̾ �����ϰ� �ȴ�.
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
        // �ִϸ����Ϳ� ��ϵ� ��� �Ķ���� ��������
        AnimatorControllerParameter[] parameters = animator.parameters;

        // ��� �Ҹ��� �Ķ������ ���� false�� ����
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
            Debug.Log("����!");
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
