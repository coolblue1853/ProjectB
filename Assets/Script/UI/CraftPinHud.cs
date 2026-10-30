using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 제작 재료 추적 HUD(몬헌식 핀, BasicFeatures.md 7절). 제작 창에서 P(패드 R3)로 지금 레시피를 고정/해제하면
/// 오른쪽 위에 결과와 재료 보유/필요를 띄운다. 재료가 다 모이면 획득 로그로 한 번 알린다. 저장은 ES3 craftpin(슬롯 파일).
/// 수량은 InventoryManager 표시 캐시(inventoryItemStack)를 0.5초마다 읽고, 바뀐 경우에만 글자를 다시 만든다.
/// </summary>
public class CraftPinHud : MonoBehaviour
{
    public const string SaveKey = "craftpin";
    private const float RefreshInterval = 0.5f;

    private static CraftPinHud _instance;

    private CraftPinModel _pin = new CraftPinModel();
    private readonly List<string> _names = new List<string>();
    private int[] _shown = new int[0];
    private string _resultName;
    private bool _wasReady;
    private float _timer;
    private GameObject _chip;
    private TextMeshProUGUI _text;
    private readonly StringBuilder _sb = new StringBuilder(256);
    private bool _namesResolved;
    private float _silentUntil;
    private string _header;
    private const float SilentAfterLoad = 1.5f;

    /// <summary>읽기 전용 조회용(프로브·표시). 바꾸려면 Toggle/SyncIfPinned 를 쓸 것 — 직접 Set 하면 저장·표시가 안 된다</summary>
    public static CraftPinModel Pin => _instance != null ? _instance._pin : null;

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
        var go = new GameObject("[CraftPinHud]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<CraftPinHud>();
    }

    private void Awake()
    {
        Build();
        _pin = CraftPinModel.Parse(LoadSaved());
        // 부트 시점엔 아이템 DB(Start 에서 CSV 로드)·인벤토리가 아직 비어 있다 → 이름은 DB 준비 후 다시 풀고, 첫 1.5초는 알림 없이 기준만 잡는다
        _silentUntil = Time.unscaledTime + SilentAfterLoad;
        ApplyPin();
    }

    private static string LoadSaved()
    {
        try
        {
            return ES3.KeyExists(SaveKey) ? ES3.Load<string>(SaveKey) : "";
        }
        catch (System.Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "제작 핀 읽기 실패: " + e.Message);
            return "";
        }
    }

    /// <summary>레시피 고정/해제(같은 결과면 해제). 고정했으면 true</summary>
    public static bool Toggle(string result, IList<string> ids, IList<int> counts)
    {
        if (_instance == null || string.IsNullOrEmpty(result))
        {
            return false;
        }
        CraftPinHud h = _instance;
        string old = h._pin.Result;
        var next = new CraftPinModel();
        next.Set(result, ids, counts);
        // 같은 결과·같은 재료면 해제, 그 밖(다른 레시피·같은 결과의 다른 재료 목록)은 교체. 재료가 빈 레시피는 무시
        bool unpin = old == result && (next.IsEmpty || next.Serialize() == h._pin.Serialize());
        if (!unpin && next.IsEmpty)
        {
            return false;
        }
        h._pin = unpin ? new CraftPinModel() : next;
        h.Save();
        h.ApplyPin();
        Sfx.Play(SfxId.UiMove, 0.8f);
        PickupLog.Report(Loc.T(unpin ? "Unpinned: " : "Pinned: ") + DisplayName(unpin ? old : result), 1, "C");
        return !unpin;
    }

    /// <summary>
    /// 제작 창에서 고정한 레시피를 다시 볼 때 재료 목록을 조용히 맞춘다(레시피 데이터가 바뀌어도 옛 목록이 세이브에 남지 않게).
    /// 결과가 다르거나 목록이 같으면 아무것도 하지 않는다
    /// </summary>
    public static void SyncIfPinned(string result, IList<string> ids, IList<int> counts)
    {
        if (_instance == null || _instance._pin.IsEmpty || _instance._pin.Result != result)
        {
            return;
        }
        var next = new CraftPinModel();
        next.Set(result, ids, counts);
        if (next.IsEmpty || next.Serialize() == _instance._pin.Serialize())
        {
            return;
        }
        _instance._pin = next;
        _instance.Save();
        _instance.ApplyPin();
        GameLog.Info(GameLog.Cat.Item, "제작 핀 재료 목록 갱신: " + result);
    }

    private void Save()
    {
        try
        {
            ES3.Save(SaveKey, _pin.Serialize());
        }
        catch (System.Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "제작 핀 저장 실패: " + e.Message);
        }
    }

    private static bool DbReady => DatabaseManager.instance != null && DatabaseManager.instance.Items != null && DatabaseManager.instance.Items.Count > 0;

    private void ApplyPin()
    {
        _namesResolved = DbReady;
        UiTheme theme = UiTheme.Get();
        _header = "<color=#" + ColorUtility.ToHtmlStringRGB(theme.unique) + ">" + Loc.T("CRAFT PIN") + "</color>  ";
        _names.Clear();
        for (int i = 0; i < _pin.Count; i++)
        {
            _names.Add(DisplayName(_pin.IdAt(i)));
        }
        _resultName = _pin.IsEmpty ? null : DisplayName(_pin.Result);
        _shown = new int[_pin.Count];
        for (int i = 0; i < _shown.Length; i++)
        {
            _shown[i] = -1;
        }
        _wasReady = _pin.IsReady(Have); // 고정 순간 이미 모였으면 알림 없음
        _timer = 0f;
        Refresh();
    }

    private static string DisplayName(string id)
    {
        if (!DbReady)
        {
            return Loc.T(id); // DB 로드 전(부트 직후) — Update 에서 다시 푼다
        }
        DatabaseManager db = DatabaseManager.instance;
        ItemData d = db.LoadItemData(db.FindItemDataIndex(id));
        return d != null && !string.IsNullOrEmpty(d.itemNameT) ? d.itemNameT : Loc.T(id);
    }

    private static int Have(string id)
    {
        DatabaseManager.inventoryItemStack.TryGetValue(id, out int n);
        return n;
    }

    private void Update()
    {
        bool show = !_pin.IsEmpty && !DatabaseManager.isOpenUI;
        if (_chip.activeSelf != show)
        {
            _chip.SetActive(show);
        }
        if (_pin.IsEmpty)
        {
            return;
        }
        if (!_namesResolved && DbReady)
        {
            ApplyPin(); // DB 가 늦게 로드된 경우 표시 이름을 다시 푼다
        }
        _timer -= Time.unscaledDeltaTime;
        if (_timer <= 0f)
        {
            _timer = RefreshInterval;
            Refresh();
        }
    }

    private void Refresh()
    {
        if (_pin.IsEmpty)
        {
            return;
        }
        bool changed = false;
        for (int i = 0; i < _pin.Count; i++)
        {
            int n = Have(_pin.IdAt(i));
            if (n != _shown[i])
            {
                _shown[i] = n;
                changed = true;
            }
        }
        if (!changed)
        {
            return;
        }
        _sb.Clear();
        _sb.Append(_header).Append(_resultName);
        bool ready = true;
        for (int i = 0; i < _pin.Count; i++)
        {
            int need = _pin.NeedAt(i);
            bool ok = _shown[i] >= need;
            ready &= ok;
            _sb.Append('\n').Append(ok ? "<color=#4CD964>" : "<color=#D8D8D8>").Append(_names[i]).Append("  ")
                .Append(Mathf.Min(_shown[i], need)).Append('/').Append(need).Append("</color>");
        }
        _text.text = _sb.ToString();
        if (ready && !_wasReady && Time.unscaledTime >= _silentUntil)
        {
            PickupLog.Report(Loc.T("Ready to craft: ") + _resultName, 1, "U");
            Sfx.Play(SfxId.Craft, 0.6f);
        }
        _wasReady = ready;
    }

    private void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image chip = theme.NewPanel("Chip", transform, false, 0.85f);
        _chip = chip.gameObject;
        RectTransform cr = chip.rectTransform;
        cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(1f, 1f);
        cr.anchoredPosition = new Vector2(-16f, -16f);
        cr.sizeDelta = new Vector2(380f, 60f);
        var fitter = chip.gameObject.AddComponent<VerticalLayoutGroup>();
        fitter.padding = new RectOffset(14, 14, 10, 10);
        fitter.childControlHeight = true;
        fitter.childControlWidth = true;
        fitter.childForceExpandHeight = false;
        chip.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _text = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        _text.transform.SetParent(chip.transform, false);
        if (theme.pixelFont != null)
        {
            _text.font = theme.pixelFont;
        }
        _text.fontSize = 22f;
        _text.alignment = TextAlignmentOptions.TopLeft;
        _text.textWrappingMode = TextWrappingModes.NoWrap;
        _text.outlineWidth = 0.25f;
        _text.outlineColor = new Color32(0, 0, 0, 220);
        _text.raycastTarget = false;
        _chip.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
