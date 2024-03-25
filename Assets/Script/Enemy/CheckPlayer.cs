using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayer : MonoBehaviour
{
    public EnemyFSM enemyFSM;
    CircleCollider2D circle;
    public bool isPreemptive = true; // 선제공격인지 아닌지. 아니라면 맞았을때에만 플레이어를 추적하게 된다.
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
            Debug.Log("플레이어 나감");

            enemyFSM.KillBrainSequence();
            enemyFSM.StateChanger("Stay");
            enemyFSM.ReActiveBrainSequence();
        }
    }
}
