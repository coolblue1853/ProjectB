using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroundCheck : MonoBehaviour
{
    public GameObject groundCheck;
    public GameObject groundCheck2;
    RaycastHit2D hit1;
    RaycastHit2D hit2;
    public EnemyFSM EnemyFSM;
    Rigidbody2D rb;
    public PatrolNode patrolNode;

    // Start is called before the first frame update
    void Start()
    {
        rb = transform.parent.GetComponent<Rigidbody2D>();

        Invoke("GroundCheck", 0.1f);
    }

   public bool isGroundCheck = false;

    void GroundCheck()
    {
        isGroundCheck = true;

    }
    // Update is called once per frame
    private void Update()
    {
        if(isGroundCheck == true)
        {
            hit2 = Physics2D.Raycast(groundCheck2.transform.position, Vector2.down, 0.3f, LayerMask.GetMask("Ground"));

             if (hit2.collider == null) //right
            {

                EnemyFSM.KillBrainSequence();
                rb.velocity = Vector2.zero;
           patrolNode.isStop = true;
             EnemyFSM.ReActiveBrainSequence();
             isGroundCheck = false;
            }
        }

    }
}
