using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBody : MonoBehaviour
{
    GameObject player;
    Vector2 knockbackDir;
    private void Start()
    {
        Invoke("DistroyIt", 3f);
    }
    void DistroyIt()
    {
        Destroy(this.gameObject);
    }
    public void Force2DeadBody(float knockbackForce)
    {
        player = GameObject.FindWithTag("Player");
        if (player.transform.position.x > this.transform.position.x)
        {
            knockbackDir = new Vector2(-1, 1);
        }
        else
        {
            knockbackDir = new Vector2(1, 1);
        }

        for(int i =0; i < transform.childCount; i++)
        {

            Rigidbody2D rb = transform.GetChild(i).GetComponent<Rigidbody2D>();
            float forceRange = Random.Range(knockbackForce*0.2f, knockbackForce);
            rb.AddForce(knockbackDir.normalized * forceRange, ForceMode2D.Impulse);
        }
    }

}
