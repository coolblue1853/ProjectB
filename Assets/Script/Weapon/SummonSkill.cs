using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonSkill : MonoBehaviour
{
    public GameObject summonObject;
    public int summonCount;
    // Start is called before the first frame update
    void Start()
    {
        float zPosition = Random.Range(-1f, 1f);
        float xPosition = Random.Range(-1f, 1f);
        GameObject r = Instantiate(summonObject, this.transform.position + new Vector3(xPosition, 0, zPosition), this.transform.rotation);
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
