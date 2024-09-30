using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
public class DeadBody : MonoBehaviour
{
    public IObjectPool<GameObject> deadbodyPool { get; set; }
    public float maxNForce = 10; // ÃÖ´ë ³Ë¹é Èû
    GameObject player;
    public GameObject parentEnemy;
    Vector2 knockbackDir;
    private Vector3[] partsPosition;
    private GameObject[] partsObject;
    private void Awake()
    {
        partsPosition = new Vector3[transform.childCount];
        partsObject = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform T = transform.GetChild(i);
            partsObject[i] = transform.GetChild(i).gameObject;
            partsPosition[i] = new Vector3(-T.localScale.x, T.localScale.y, T.localScale.z); ;
        }
    }

    private void Start()
    {

        // Invoke("ResetParts", 3f);


    }


    void ResetParts()
    {
        this.gameObject.SetActive(false);
        for (int i = 0; i < transform.childCount; i++)
        {   
            partsObject[i].transform.position = partsPosition[i];
        }
    }
    public void Force2DeadBody(float knockbackForce)
    {

        Invoke("ResetParts", 3f);
        if (knockbackForce > maxNForce) // ÃÖ´ë ³Ë¹é ¼Óµµ
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
