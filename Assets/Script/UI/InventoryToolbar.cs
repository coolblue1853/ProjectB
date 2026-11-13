using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 인벤 창 화면 버튼(REWORK-1 S3·S5, U39): 정렬·쌓기·필터·잠금·가방 확장. 런타임 생성(씬 수정 없음).
/// 각 버튼은 기존 키·패드 기능의 마우스 경로다 — 정렬 R / 패드 R3, 필터 G / LT+RT, 잠금 L / LT+R3, 가방 확장 N / LT+X.
/// 쌓기(같은 아이템을 앞 칸으로 합침)는 이 파일에서 T / 패드 LT+LB 를 함께 받는다.
/// 상점이 열려 있으면 숨기고(상점 쪽과 겹침), 수량 창·모달 상태에서는 눌러도 무시한다.
/// </summary>
public sealed class InventoryToolbar : MonoBehaviour
{
    private const float ButtonW = 270f, ButtonH = 104f, Gap = 10f; // 화면 픽셀(캔버스 배율로 나눠 쓴다)

    private static InventoryToolbar _instance;

    private GameObject _root;
    private TextMeshProUGUI _bagLabel;
    private TextMeshProUGUI _filterLabel;
    private TextMeshProUGUI _searchLabel;
    private Transform _builtFor;
    private float _nextLabel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (_instance != null)
        {
            return;
        }
        var go = new GameObject("[InventoryToolbar]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<InventoryToolbar>();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private static bool Open(InventoryManager m)
    {
        return m != null && m.inventory != null && m.inventory.activeInHierarchy;
    }

    private static bool ShopOpen(InventoryManager m)
    {
        return m.ShopGameObject != null && m.ShopGameObject.activeSelf;
    }

    /// <summary>가방 확장 키(N)를 상점 밖에서도 받는 때: 인벤이 열려 있고 닫힌 상태(커서 이동 중)</summary>
    public static bool CanUseBag()
    {
        InventoryManager m = InventoryManager.instance;
        return Open(m) && !QuantityPopup.IsBlocking && (string.IsNullOrEmpty(m.state) || m.state == "chestOpen");
    }

    /// <summary>인벤 칸을 누르는 동작이 가능한 상태(모달·교체 중이 아님)</summary>
    private static bool Idle(InventoryManager m)
    {
        return !QuantityPopup.IsBlocking && !QuickUseRules.IsModal(m.state) && !ItemDrag.IsDragging;
    }

    private void Update()
    {
        InventoryManager m = InventoryManager.instance;
        bool show = Open(m) && !ShopOpen(m);
        if (!show)
        {
            if (_root != null && _root.activeSelf)
            {
                _root.SetActive(false);
            }
            return;
        }
        if (_root == null || _builtFor != m.inventory.transform)
        {
            EnsureBuilt(m);
        }
        if (_root == null)
        {
            return;
        }
        if (!_root.activeSelf)
        {
            _root.SetActive(true);
        }
        if (Time.unscaledTime >= _nextLabel)
        {
            _nextLabel = Time.unscaledTime + 0.25f; // 글자는 0.25초마다만 갱신(문자열 할당·TMP 재생성 억제)
            string bag = "N · " + BagExpansion.ButtonLabel();
            if (bag != _bagLabel.text)
            {
                _bagLabel.text = bag;
            }
            string filter = "필터 G · " + InventoryFilterRules.Label(InventoryFilterView.Current);
            if (filter != _filterLabel.text)
            {
                _filterLabel.text = filter;
            }
            string search = InventorySearch.ButtonLabel();
            if (search != _searchLabel.text)
            {
                _searchLabel.text = search;
            }
        }
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;
        bool stackKey = (kb != null && kb.tKey.wasPressedThisFrame)
            || (pad != null && pad.leftTrigger.isPressed && pad.leftShoulder.wasPressedThisFrame);
        if (stackKey && Idle(m) && (string.IsNullOrEmpty(m.state) || m.state == "chestOpen"))
        {
            m.StackAll();
        }
    }

    private void EnsureBuilt(InventoryManager m)
    {
        Transform anchor = m.inventory.transform.Find("BoxButton");
        if (anchor == null)
        {
            return;
        }
        if (_root != null && _builtFor == m.inventory.transform)
        {
            return;
        }
        if (_root != null)
        {
            Destroy(_root);
        }
        _builtFor = m.inventory.transform;
        _root = new GameObject("InventoryToolbar", typeof(RectTransform));
        _root.transform.SetParent(m.inventory.transform, false);
        var rt = (RectTransform)_root.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 1f);
        float scale = Mathf.Max(0.01f, _root.transform.lossyScale.x);
        rt.sizeDelta = new Vector2(ButtonW / scale, 8f * (ButtonH + Gap) / scale);
        // "BOX n" 탭 아래(탭 중심 x)에 세로로 쌓는다
        Vector3 tab = anchor.position;
        float tabHalf = 0f;
        var tabRt = anchor as RectTransform;
        if (tabRt != null)
        {
            tabHalf = tabRt.rect.height * 0.5f * tabRt.lossyScale.y;
        }
        _root.transform.position = new Vector3(tab.x, tab.y - tabHalf - 60f, 0f);
        Action[] actions = { OnSort, OnStack, OnFilter, OnLock, OnBag, OnSearch };
        string[] names = { "정렬 R", "쌓기 T", "필터", "잠금 L", "가방", "검색 /" };
        for (int i = 0; i < names.Length; i++)
        {
            TextMeshProUGUI label = MakeButton(rt, i, names[i], scale, actions[i]);
            if (i == 2)
            {
                _filterLabel = label;
            }
            else if (i == 4)
            {
                _bagLabel = label;
            }
            else if (i == 5)
            {
                _searchLabel = label;
            }
        }
        MakeHint(rt, 6, "분해 D · 버리기 Del · 비교 Shift\n프리셋 7 8 9 · 되사기 U · 효과 상세 Tab\n가방 확장 N (+5칸씩, 최대 125칸)", scale);
        MakeHint(rt, 7, "패드: 분해 L3 두 번 · 비교 LT\n잠금 LT+R3 · 필터 LT+RT\n확장 LT+X · 효과 상세 Y\n검색 LT+RB(첫 글자)", scale);
    }

    /// <summary>버튼 아래 조작 안내 글(U39): 화면 버튼이 없는 단축키를 보여 준다. 클릭 대상 아님</summary>
    private static void MakeHint(RectTransform parent, int index, string text, float scale)
    {
        UiTheme theme = UiTheme.Get();
        var go = new GameObject("Hint" + index, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var label = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            label.font = theme.pixelFont;
        }
        label.enableAutoSizing = true;
        label.fontSizeMax = 26f / scale;
        label.fontSizeMin = 8f / scale;
        label.fontSize = 26f / scale;
        label.raycastTarget = false;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.8f, 0.8f, 0.8f, 0.9f);
        label.text = text;
        RectTransform r = label.rectTransform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 1f);
        r.sizeDelta = new Vector2(ButtonW / scale, ButtonH / scale);
        r.anchoredPosition = new Vector2(0f, -index * (ButtonH + Gap) / scale);
    }

    private static TextMeshProUGUI MakeButton(RectTransform parent, int index, string text, float scale, Action onClick)
    {
        UiTheme theme = UiTheme.Get();
        Image img = theme.NewPanel("Btn" + index, parent, true);
        img.raycastTarget = true;
        RectTransform r = img.rectTransform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 1f);
        r.sizeDelta = new Vector2(ButtonW / scale, ButtonH / scale);
        r.anchoredPosition = new Vector2(0f, -index * (ButtonH + Gap) / scale);
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => Click(onClick));
        var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(r, false);
        var label = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            label.font = theme.pixelFont;
        }
        label.enableAutoSizing = true;
        label.fontSizeMax = 36f / scale;
        label.fontSizeMin = 8f / scale;
        label.fontSize = 36f / scale;
        label.raycastTarget = false;
        label.alignment = TextAlignmentOptions.Center;
        label.text = text;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = new Vector2(4f, 2f);
        label.rectTransform.offsetMax = new Vector2(-4f, -2f);
        return label;
    }

    private static void Click(Action action)
    {
        InventoryManager m = InventoryManager.instance;
        if (m == null || !Idle(m) || (!string.IsNullOrEmpty(m.state) && m.state != "chestOpen" && m.state != "detail"))
        {
            return;
        }
        action();
    }

    private static void OnSort()
    {
        InventoryManager.instance.SortCurrentPage();
    }

    private static void OnStack()
    {
        InventoryManager m = InventoryManager.instance;
        if (string.IsNullOrEmpty(m.state) || m.state == "chestOpen")
        {
            m.StackAll();
        }
    }

    private static void OnFilter()
    {
        InventoryManager m = InventoryManager.instance;
        if (m.chest == null && string.IsNullOrEmpty(m.state))
        {
            InventoryFilterView.Cycle(m);
        }
    }

    private static void OnLock()
    {
        InventoryManager m = InventoryManager.instance;
        InventoryConvenience.ToggleLock(m, m.state == "detail" ? m.detail : m.CursorItem());
    }

    private static void OnSearch()
    {
        InventorySearch.Toggle();
    }

    private static void OnBag()
    {
        BagExpansion.Press();
    }
}
