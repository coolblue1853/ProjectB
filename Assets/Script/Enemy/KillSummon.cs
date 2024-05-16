using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillSummon : MonoBehaviour
{
    // Start is called before the first frame update
    public float killTime;
    void Start()
    {
        Invoke("DestorySummon", killTime);
    }

    void DestorySummon()
    {
        Destroy(this.gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
