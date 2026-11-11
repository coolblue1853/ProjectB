using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// 가방 이름 검색 (REWORK-1 S4). 맞지 않는 칸은 필터와 같이 반투명(InventoryFilterView.Apply 가 함께 판정).
/// 키보드: / 또는 Ctrl+F 로 입력 시작 → 글자·한글(IME) 입력, Backspace 지움, Enter 확정(검색어 유지), Esc 해제.
/// 패드(글자판 없음): LT+RB 로 가방 아이템 이름 첫 글자를 하나씩 넘김, 끝에서 해제.
/// 마우스: 인벤 창 "검색" 버튼(InventoryToolbar). 입력 중에는 UiLock + IsBlocking 으로 R·G·T 같은 단축키가 글자로 먹히지 않게 한다. 인벤을 닫으면 해제.
/// </summary>
public sealed class InventorySearch : MonoBehaviour
{
    private const string LockOwner = "InvSearch";

    private static InventorySearch _instance;
    private static string _query = "";
    private static string _composition = "";
    private static bool _typing;
    private static int _closedFrame = -1;
    private static int _startFrame = -1;
    private static readonly List<string> NameBuffer = new List<string>();
    private static Keyboard _kb; // 구독한 키보드(해제 때 같은 기기에서 뺀다)

    /// <summary>적용 중인 검색어(입력 중인 IME 조합 글자 포함)</summary>
    public static string Query => string.IsNullOrEmpty(_composition) ? _query : _query + _composition;

    public static bool Active => !string.IsNullOrWhiteSpace(Query);

    public static bool Typing => _typing;

    /// <summary>입력 중이거나 방금 닫은 프레임: 같은 키를 인벤·창고가 이중 처리하지 않게 모달로 본다</summary>
    public static bool IsBlocking => _typing || _closedFrame == Time.frameCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
        _query = "";
        _composition = "";
        _typing = false;
        _closedFrame = -1;
        _startFrame = -1;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[InventorySearch]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<InventorySearch>();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
        if (_typing)
        {
            StopTyping();
        }
    }

    private static bool Open(InventoryManager m)
    {
        return m != null && m.inventory != null && m.inventory.activeInHierarchy;
    }

    private static bool ShopOpen(InventoryManager m)
    {
        return m.ShopGameObject != null && m.ShopGameObject.activeSelf; // 상점 열림엔 되사기 [ ] U 가 글자와 겹친다
    }

    /// <summary>검색 버튼에 보이는 글자</summary>
    public static string ButtonLabel()
    {
        if (_typing)
        {
            return "검색: " + Query + "_";
        }
        return Active ? "검색: " + Query : "검색 /";
    }

    /// <summary>화면 버튼용: 입력 중이면 확정, 아니면 입력 시작</summary>
    public static void Toggle()
    {
        if (_typing)
        {
            StopTyping();
        }
        else
        {
            StartTyping();
        }
    }

    public static void StartTyping()
    {
        Keyboard kb = Keyboard.current;
        InventoryManager m = InventoryManager.instance;
        if (_typing || kb == null || !Open(m) || QuantityPopup.IsBlocking || ShopOpen(m))
        {
            return;
        }
        _typing = true;
        _startFrame = Time.frameCount;
        _kb = kb;
        kb.onTextInput += OnTextInput;
        kb.onIMECompositionChange += OnComposition;
        kb.SetIMEEnabled(true);
        UiLock.Set(LockOwner, true);
    }

    public static void StopTyping()
    {
        Keyboard kb = _kb;
        _kb = null;
        if (kb != null)
        {
            kb.onTextInput -= OnTextInput;
            kb.onIMECompositionChange -= OnComposition;
            kb.SetIMEEnabled(false);
        }
        if (!string.IsNullOrEmpty(_composition))
        {
            foreach (char c in _composition) // 조합 중이던 글자는 확정으로 본다
            {
                _query = InventorySearchRules.Append(_query, c);
            }
            _composition = "";
        }
        _typing = false;
        _closedFrame = Time.frameCount;
        UiLock.Set(LockOwner, false);
        Refresh();
    }

    public static void Clear()
    {
        if (_typing)
        {
            StopTyping();
        }
        if (_query.Length > 0 || _composition.Length > 0)
        {
            _query = "";
            _composition = "";
            Refresh();
        }
    }

    private static void OnTextInput(char c)
    {
        if (!_typing || _startFrame == Time.frameCount) // 시작 키 "/" 자체는 글자로 넣지 않는다
        {
            return;
        }
        _query = InventorySearchRules.Append(_query, c);
        Refresh();
    }

    private static void OnComposition(IMECompositionString comp)
    {
        _composition = comp.ToString();
        Refresh();
    }

    private static void Refresh()
    {
        InventoryFilterView.Apply(InventoryManager.instance);
    }

    private void Update()
    {
        InventoryManager m = InventoryManager.instance;
        if (!Open(m))
        {
            if (_typing || Active)
            {
                Clear(); // 인벤을 닫으면 해제
            }
            return;
        }
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;
        if (_typing)
        {
            UpdateTyping(kb);
            return;
        }
        if (QuantityPopup.IsBlocking || ItemDrag.IsDragging || QuickUseRules.IsModal(m.state) || !(string.IsNullOrEmpty(m.state) || m.state == "chestOpen"))
        {
            return;
        }
        bool start = kb != null && (kb.slashKey.wasPressedThisFrame || (kb.ctrlKey.isPressed && kb.fKey.wasPressedThisFrame));
        if (start)
        {
            StartTyping();
        }
        else if (pad != null && pad.leftTrigger.isPressed && pad.rightShoulder.wasPressedThisFrame)
        {
            _query = InventorySearchRules.NextInitial(ItemNames(m), _query);
            Refresh();
        }
    }

    private static void UpdateTyping(Keyboard kb)
    {
        if (kb == null)
        {
            StopTyping();
            return;
        }
        if (UiLock.TryBack(LockOwner, kb.escapeKey.wasPressedThisFrame))
        {
            Clear();
            return;
        }
        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
        {
            StopTyping();
            return;
        }
        if (kb.backspaceKey.wasPressedThisFrame && string.IsNullOrEmpty(_composition))
        {
            _query = InventorySearchRules.Backspace(_query);
            Refresh();
        }
    }

    private static List<string> ItemNames(InventoryManager m)
    {
        NameBuffer.Clear();
        if (m.inventoryUI == null)
        {
            return NameBuffer;
        }
        foreach (GameObject page in m.inventoryUI)
        {
            if (page == null)
            {
                continue;
            }
            foreach (Transform box in page.transform)
            {
                ItemCheck ic = box.childCount > 0 ? box.GetChild(0).GetComponent<ItemCheck>() : null;
                if (ic != null && ic.itemData != null && !string.IsNullOrEmpty(ic.itemData.itemNameT))
                {
                    NameBuffer.Add(ic.itemData.itemNameT);
                }
            }
        }
        return NameBuffer;
    }
}
