using UnityEngine;

/// <summary>기존 저장 경로를 사용하는 마을 기록지기. 저장 형식·부활 규칙은 바꾸지 않는다.</summary>
public sealed class TownSavePoint : MonoBehaviour
{
    private KeyAction _action;

    private void Awake()
    {
        _action = new KeyAction();
    }

    private void OnEnable()
    {
        _action.UI.Up.Enable();
    }

    private void OnDisable()
    {
        _action.UI.Up.Disable();
    }

    private void OnDestroy()
    {
        _action.Dispose();
    }

    private void Update()
    {
        if (_action.UI.Up.triggered && !DatabaseManager.isOpenUI && IsNear())
        {
            SaveNow();
        }
    }

    private bool IsNear()
    {
        if (PlayerController.instance == null)
        {
            return false;
        }
        Vector3 delta = PlayerController.instance.transform.position - transform.position;
        return Mathf.Abs(delta.x) <= 1.1f && Mathf.Abs(delta.y) <= 2f;
    }

    public bool SaveNow()
    {
        if (!isActiveAndEnabled || !IsNear() || DatabaseManager.isOpenUI || SaveManager.instance == null || TitleScreen.IsShowing ||
            PixelCrushers.DialogueSystem.DialogueManager.isConversationActive)
        {
            return false;
        }
        SaveManager.instance.SaveProgress();
        PickupLog.Warn("마을에서 여정을 기록했습니다");
        return true;
    }
}
