using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayer : MonoBehaviour
{
    public EnemyFSM enemyFSM;
    CircleCollider2D circle;
    public bool isPreemptive = true; // ������������ �ƴ���. �ƴ϶�� �¾��������� �÷��̾ �����ϰ� �ȴ�.
    public bool isPlayerCheckActive = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            enemyFSM.KillBrainSequence();
            enemyFSM.StateChanger("Chase");
            enemyFSM.ReActiveBrainSequence();

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log("�÷��̾� ����");

            enemyFSM.KillBrainSequence();
            enemyFSM.StateChanger("Stay");
            enemyFSM.ReActiveBrainSequence();
        }
    }
}
