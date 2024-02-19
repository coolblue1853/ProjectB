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
    public Rigidbody2D rb;
    public Vector2 knockbackDir;
    public float knockbackForce;
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
            rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);

        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
           // PlayerHealthManager.Instance.FullnessUp(10);

        }
    }
}
