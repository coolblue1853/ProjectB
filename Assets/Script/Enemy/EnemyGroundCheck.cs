using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroundCheck : MonoBehaviour
{
    public GameObject groundCheck;
    public GameObject groundCheck2;
    RaycastHit2D hit1;
    RaycastHit2D hit2;
    EnemyFSM EnemyFSM;

    public PatrolNode patrolNode;

    // Start is called before the first frame update
    void Start()
    {
        EnemyFSM = transform.parent.GetComponent<EnemyFSM>();
        Invoke("GroundCheck", 3f);
    }

    bool isGroundCheck = false;

    void GroundCheck()
    {
        isGroundCheck = true;

    }
    // Update is called once per frame
    private void LateUpdate()
    {
        if(isGroundCheck == true)
        {
            hit1 = Physics2D.Raycast(groundCheck.transform.position, Vector2.down, 0.3f, LayerMask.GetMask("Ground"));
            hit2 = Physics2D.Raycast(groundCheck2.transform.position, Vector2.down, 0.3f, LayerMask.GetMask("Ground"));

            if (hit1.collider == null) //left
            {
                EnemyFSM.KillBrainSequence();
                patrolNode.isLeftEnd = true;
                EnemyFSM.ReActiveBrainSequence();
            }
            else if (hit2.collider == null) //right
            {
                EnemyFSM.KillBrainSequence();
                patrolNode.isRightEnd = true;
                EnemyFSM.ReActiveBrainSequence();
            }
        }

    }
}
