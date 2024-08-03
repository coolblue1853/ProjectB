using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffTest : MonoBehaviour
{
   public GameObject buffObject;
    public AttackManager att;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            GameObject buff = Instantiate(buffObject, Vector2.zero, Quaternion.identity, att.BuffSlot.transform);
        }
    }
}
