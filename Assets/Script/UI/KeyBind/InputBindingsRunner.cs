using UnityEngine;

/// <summary>InputBindings 의 지연 적용용 러너(BeforeSceneLoad 에 생성, DontDestroyOnLoad)</summary>
[DefaultExecutionOrder(-1000)]
public class InputBindingsRunner : MonoBehaviour
{
    private void Update()
    {
        InputBindings.FlushPending();
    }
}
