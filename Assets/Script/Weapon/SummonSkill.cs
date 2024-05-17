using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonSkill : MonoBehaviour
{
    public GameObject summonObject;
    public int summonCount;
    public float[] summonPosition;
    public float yPos;
    // Start is called before the first frame update
    void Start()
    {
        summonPosition = new float[summonCount];
        for (int i =0; i < summonCount; i++)
        {
            float zPosition = Random.Range(-1f, 1f);

            GameObject r = Instantiate(summonObject, this.transform.position + new Vector3(0, yPos, zPosition), this.transform.rotation);
        }

      // Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
