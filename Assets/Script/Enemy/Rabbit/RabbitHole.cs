using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitHole : MonoBehaviour
{
    public GameObject rabbit;
    public int rabbitCount;

    public GameObject[] rabbitActive;
     
    // Start is called before the first frame update
    void Start()
    {
        rabbitActive = new GameObject[rabbitCount];
       Invoke("CreatRabbit", 1f);
    }

    void CreatRabbit()
    {

        for (int i = 0; i< rabbitCount; i++)
        {
            float xPosition =  Random.Range(-1f, 1f);
            GameObject r = Instantiate(rabbit, this.transform.position + new Vector3(xPosition,0), this.transform.rotation);
            rabbitActive[i] = r;
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
        for (int i = 0; i < rabbitCount; i++)
        {
            if (rabbitActive[i] == null)
            {
                float xPosition = Random.Range(-1f, 1f);
                GameObject r = Instantiate(rabbit, this.transform.position + new Vector3(xPosition, 0), this.transform.rotation);
                rabbitActive[i] = r;
                RabbitReset RH = r.GetComponent<RabbitReset>();
                RH.rabbitHole = this.GetComponent<RabbitHole>();
            }
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
