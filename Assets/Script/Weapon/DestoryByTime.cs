using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryByTime : MonoBehaviour
{
    public float time;
    // Start is called before the first frame update
    void Start()
    {

    }
    public void DestroyEffect(float waitTime)
    {
        Invoke("Destroy", waitTime);
    }
    void Destroy()
    {
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
