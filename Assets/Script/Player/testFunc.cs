using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testFunc : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PlayerHealthManager.Instance.SteminaDown(1);
            // PlayerHealthManager.Instance.HpDown(1);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            PlayerHealthManager.Instance.FullnessUp(10);

        }
    }
}
