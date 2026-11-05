using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 가방 보기 필터 (INV-FILTER, 임시 기본 UI). 인벤토리가 열린 채 닫힌 상태(state "")에서 G(패드 LT+R3)로 전체→장비→소비→재료 순환.
/// 맞지 않는 칸은 반투명으로만 표시한다(데이터·슬롯 인덱스·세이브 불변). 필터 중에는 정렬을 막는다.
/// </summary>
public class InventoryFilterView : MonoBehaviour
{
    private const float RefreshInterval = 0.25f;
    private const float DimAlpha = 0.25f;

    public static InventoryFilter Current { get; private set; }

    private static InventoryFilterView _instance;
    private float _nextRefresh;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Current = InventoryFilter.All;
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[InventoryFilterView]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<InventoryFilterView>();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private static bool InvOpen(InventoryManager m)
    {
        return m != null && m.inventory != null && m.inventory.activeSelf;
    }

    private void Update()
    {
        InventoryManager m = InventoryManager.instance;
        if (!InvOpen(m))
        {
            if (Current != InventoryFilter.All)
            {
                Current = InventoryFilter.All; // 닫으면 해제
                Apply(m);
            }
            return;
        }
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;
        bool pressed = (kb != null && kb.gKey.wasPressedThisFrame) || (pad != null && pad.leftTrigger.isPressed && pad.rightStickButton.wasPressedThisFrame);
        if (pressed && m.state == "" && m.chest == null)
        {
            Current = InventoryFilterRules.Next(Current);
            Apply(m);
            _nextRefresh = Time.unscaledTime + RefreshInterval;
        }
        else if (Current != InventoryFilter.All && Time.unscaledTime >= _nextRefresh)
        {
            _nextRefresh = Time.unscaledTime + RefreshInterval;
            Apply(m); // 새로 들어온 아이템 반영
        }
    }

    /// <summary>모든 페이지 칸의 아이템에 필터 알파를 적용(All 이면 원복)</summary>
    public static void Apply(InventoryManager m)
    {
        if (m == null || m.inventoryUI == null)
        {
            return;
        }
        foreach (GameObject page in m.inventoryUI)
        {
            if (page == null)
            {
                continue;
            }
            foreach (Transform box in page.transform)
            {
                if (box.childCount == 0)
                {
                    continue;
                }
                Transform item = box.GetChild(0);
                ItemCheck ic = item.GetComponent<ItemCheck>();
                if (ic == null || ic.itemData == null)
                {
                    continue;
                }
                bool show = InventoryFilterRules.Matches(Current, ic.itemData.type);
                CanvasGroup cg = item.GetComponent<CanvasGroup>();
                if (cg == null)
                {
                    if (show)
                    {
                        continue;
                    }
                    cg = item.gameObject.AddComponent<CanvasGroup>();
                }
                cg.alpha = show ? 1f : DimAlpha;
            }
        }
    }

    private void OnGUI()
    {
        InventoryManager m = InventoryManager.instance;
        if (Current == InventoryFilter.All || !InvOpen(m))
        {
            return;
        }
        GUI.Label(new Rect(20f, 20f, 300f, 30f), "가방 필터: " + InventoryFilterRules.Label(Current) + "  (G)");
    }
}
