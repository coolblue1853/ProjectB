using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class testFunc : MonoBehaviour
{
    KeyAction action;

    private void OnEnable()
    {


    }
    private void OnDisable()
    {



    }


    private void Awake()
    {
        action = new KeyAction();




    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    bool once = false;
    // Update is called once per frame
    void Update()
    {




        if (Input.GetKeyDown(KeyCode.F1))
        {
            PlayerHealthManager.Instance.HpUp(50);

        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
           // PlayerHealthManager.Instance.FullnessUp(10);

        }
    }
}
