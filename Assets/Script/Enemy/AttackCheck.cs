using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCheck : MonoBehaviour
{
    public EnemyFSM enemyFSM;
    CircleCollider2D circle;

    public EndNode AttackEndNode;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {

            enemyFSM.KillBrainSequence();
            AttackEndNode.isNRepeat = false;

            enemyFSM.StateChanger("Attack");

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
                AttackEndNode.isNRepeat = true;
            AttackEndNode.state = "Chase";

            //enemyFSM.KillBrainSequence();
            //enemyFSM.StateChanger("Chase");

        }
    }
}
