using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBody : MonoBehaviour
{
    public float maxNForce = 10; // ÃÖ´ë ³Ë¹é Èû
    GameObject player;
   public GameObject parentEnemy;
    Vector2 knockbackDir;
    private void Start()
    {
        Invoke("DistroyIt", 3f);
        if(parentEnemy.transform.localScale.x < 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform T = transform.GetChild(i);

                T.localScale = new Vector3(-T.localScale.x, T.localScale.y, T.localScale.z); ;
            }

        }
    }
    void DistroyIt()
    {
        Destroy(this.gameObject);
    }
    public void Force2DeadBody(float knockbackForce)
    {
        if(knockbackForce > maxNForce) // ÃÖ´ë ³Ë¹é ¼Óµµ
        {
            knockbackForce = maxNForce;
        }
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
            DeadBodyEffect dBE = transform.GetChild(i).GetComponent<DeadBodyEffect>();

            if(dBE == null)
            {
                Rigidbody2D rb = transform.GetChild(i).GetComponent<Rigidbody2D>();
                float forceRange = Random.Range(knockbackForce * 0.2f, knockbackForce);
                rb.AddForce(knockbackDir.normalized * forceRange, ForceMode2D.Impulse);
            }


        }
    }

}
