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




#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // 디버그: F1 넉백 테스트(에디터·개발 빌드 전용, New Input System — B-021)
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.f1Key.wasPressedThisFrame)
        {
            rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);
        }
#endif
    }
}
