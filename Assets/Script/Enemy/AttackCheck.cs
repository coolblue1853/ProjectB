using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCheck : MonoBehaviour
{
    public EnemyFSM enemyFSM;
    CircleCollider2D circle;

    public AttackNode attackNode;
    public EndNode ChaseEndNode;
    public EndNode AttackEndNode;
    public bool isNearPlayer;
    private void Update()
    {
        if (attackNode.isAttackRepeat == true && isNearPlayer == true)
        {
            enemyFSM.KillBrainSequence();
            AttackEndNode.isNRepeat = false;
            enemyFSM.StateChanger("Attack");

            if (enemyFSM.CheckBrainActive() == false)
            {
                enemyFSM.ReActiveBrainSequence();
            }

        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {

            isNearPlayer = true;


        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            isNearPlayer = false;
            Debug.Log("Ãß°Ý");
            AttackEndNode.isNRepeat = true;
            AttackEndNode.state = "Chase";

            //enemyFSM.KillBrainSequence();
            //enemyFSM.StateChanger("Chase");

        }
    }
}
