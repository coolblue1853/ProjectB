using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroundCheck : MonoBehaviour
{

    public GameObject groundCheck2;
    RaycastHit2D hit2;
    public EnemyFSM EnemyFSM;
    Rigidbody2D rb;
    public PatrolNode patrolNode;
    public EnemyHealth enemyHealth;

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
        if(isGroundCheck == true && enemyHealth.isAttackGround == false)
        {
            hit2 = Physics2D.Raycast(groundCheck2.transform.position, Vector2.down, 0.3f, LayerMask.GetMask("Ground"));
            Debug.DrawRay(groundCheck2.transform.position, Vector2.down, Color.red, 0.3f);
             if (hit2.collider == null) //right
            {
     

            }

        }

    }
}
