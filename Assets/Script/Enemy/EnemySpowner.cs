using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpowner : MonoBehaviour
{
    public bool isInPlayer = false;
    private void OnTriggerStay2D(Collider2D collision)
    {
     if(collision.tag == "Player"&& isInPlayer == false)
        {
            isInPlayer = true;
        }   
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" )
        {
            isInPlayer = false;
        }
        if (collision.tag == "Enemy")
        {
            EnemyHealth eh = collision.transform.GetComponent<EnemyHealth>();
            eh.DisaperByTime();
        }
    }
    public GameObject enemyPrefab;
    public int enemyCount;

    public float cycleFloat;
    public GameObject[] enemySlot;
    public GameObject enemyPositon;
    public GameObject[] enemyPositionArray;

    public bool isDaytimeSpowner;

    public bool isRandSpawn = true;
    // Start is called before the first frame update
    void Start()
    {
        enemySlot = new GameObject[enemyCount];
        enemyPositionArray = new GameObject[enemyCount];
        // Invoke("CreatRabbit", 1f);


        for(int i =0; i < enemyCount; i++)
        {
            enemyPositionArray[i] = enemyPositon.transform.GetChild(i).gameObject;
        }
    }


    void CycleCheck()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (enemySlot[i] == null)
            {
                if(isRandSpawn == true)
                {
                    float xPosition = Random.Range(-1f, 1f);
                    int x = Random.Range(0, enemyCount);
                    GameObject r = Instantiate(enemyPrefab, enemyPositionArray[x].transform.position + new Vector3(xPosition, 0), enemyPositionArray[x].transform.rotation);
                    enemySlot[i] = r;
                }
                else
                {
                    float xPosition = Random.Range(-1f, 1f);
                    GameObject r = Instantiate(enemyPrefab, enemyPositionArray[i].transform.position + new Vector3(xPosition, 0), enemyPositionArray[i].transform.rotation);
                    enemySlot[i] = r;
                }

            }

        }
    }
    void DeCycleCheck()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (enemySlot[i] != null)
            {
                EnemyHealth eh = enemySlot[i].GetComponent<EnemyHealth>();
                RabbitHole rH = enemySlot[i].GetComponent<RabbitHole>();
                if (eh != null)
                {
                    eh.DisaperByTime();
                }
                if (rH != null)
                {
                    rH.DestroyRabbit();
                    Destroy(enemySlot[i].gameObject);
                }
            }

        }
    }
    /*
    void CreatEnemy()
    {

        for (int i = 0; i < enemyCount; i++)
        {
            float xPosition = Random.Range(-1f, 1f);
            GameObject r = Instantiate(enemyPrefab, this.transform.position + new Vector3(xPosition, 0), this.transform.rotation);
            enemySlot[i] = r;
            RabbitReset RH = r.GetComponent<RabbitReset>();
            RH.rabbitHole = this.GetComponent<RabbitHole>();
        }
    }

    public void ResetRabbit()
    {
        Invoke("ResetR", 2);
    }

    private void ResetR()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (enemySlot[i] == null)
            {
                float xPosition = Random.Range(-1f, 1f);
                GameObject r = Instantiate(enemyPrefab, this.transform.position + new Vector3(xPosition, 0), this.transform.rotation);
                enemySlot[i] = r;
                RabbitReset RH = r.GetComponent<RabbitReset>();
                RH.rabbitHole = this.GetComponent<RabbitHole>();
            }
        }
    }
    */
    // Update is called once per frame
    void Update()
    {
        if (isInPlayer == true)
        {
            once = false;
            if (TimeChange.instance.isDaytime == true && isDaytimeSpowner == true && activeOnce == true)
            {
                StartInvokeDaytime();
            }
            else if (TimeChange.instance.isDaytime == false && isDaytimeSpowner == true && activeOnce == false)
            {
                CancelInvoke("CycleCheck");
                EndInvokeDaytime();
            }
            if (TimeChange.instance.isDaytime == false && isDaytimeSpowner == false && activeOnce == true)
            {
                StartInvokeDaytime();
            }
            else if (TimeChange.instance.isDaytime == true && isDaytimeSpowner == false && activeOnce == false)
            {
                CancelInvoke("CycleCheck");
                EndInvokeDaytime();
            }
        }




        else if(isInPlayer == false && once == false)
        {
            once = true;
            CancelInvoke("CycleCheck");
            EndInvokeDaytime();
        }

    }
    bool once = false;

    bool activeOnce = true;
    void StartInvokeDaytime()
    {
        activeOnce = false;
        InvokeRepeating("CycleCheck", 1f, cycleFloat);

    }
    void EndInvokeDaytime()
    {
        
        activeOnce = true;
        DeCycleCheck();

    }
}
