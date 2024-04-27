using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBodyEffect : MonoBehaviour
{
    public float gravity;
    public float mass;
    public Rigidbody2D rb;

    private void Start()
    {
        Rigidbody2D rb =this.transform.GetComponent<Rigidbody2D>();
        gravity = rb.gravityScale;
        mass = rb.mass;

        rb.gravityScale=0;
       rb.mass=0;
    }

    public void AnimEnd()
    {
        rb.gravityScale= gravity;
        rb.mass= mass;
    }




}
