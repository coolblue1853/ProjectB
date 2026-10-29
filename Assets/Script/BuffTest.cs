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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // 디버그: 9 키로 버프 생성(에디터·개발 빌드 전용, New Input System — B-021)
        UnityEngine.InputSystem.Keyboard kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.digit9Key.wasPressedThisFrame)
        {
            Instantiate(buffObject, Vector2.zero, Quaternion.identity, att.BuffSlot.transform);
        }
#endif
    }
}
