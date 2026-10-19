using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 몬스터 도감 (G10, 메이플 몬스터 컬렉션식). B 키로 연다. 처치 수에 따라 정보 공개: 1=이름·아이콘, 10=HP/방어, 50=드롭.
/// HP·방어·드롭은 씬의 스포너(비활성 맵 포함)를 스캔해 구한다. 코드 생성 UI.
/// </summary>
public class BestiaryWindow : MonoBehaviour
{
    const int Cols = 4, CellSize = 104;

    static BestiaryWindow instance;

    class Info
    {
        public int hp, def;
        public readonly List<(string name, float chance)> drops = new List<(string, float)>();
    }

    BestiaryData data;
    readonly Dictionary<string, Info> infos = new Dictionary<string, Info>();
    readonly List<(Image bg, Image icon)> cells = new List<(Image, Image)>();
    KeyAction action;
    InputAction up, down, left, right, back;
    GameObject panel;
    Image bigIcon;
    TextMeshProUGUI title, body;
    int cursor;
    bool open;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance == null)
        {
            var go = new GameObject("[BestiaryWindow]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<BestiaryWindow>();
        }
    }

    public static void Toggle()
    {
        if (instance != null)
        {
            if (instance.open)
            {
                instance.Close();
            }
            else
            {
                instance.Open();
            }
        }
    }

    void Awake()
    {
        action = new KeyAction();
        up = action.UI.UPInventory;
        down = action.UI.DownInventory;
        left = action.UI.LeftInventory;
        right = action.UI.RightInventory;
        back = action.UI.Back;
        data = Resources.Load<BestiaryData>("UI/BestiaryData");
    }

    void OnDestroy()
    {
        action?.Dispose();
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.bKey.wasPressedThisFrame && (open || !DatabaseManager.isOpenUI))
        {
            Toggle();
            return;
        }
        if (!open || data == null || data.entries.Count == 0)
        {
            return;
        }
        if (back.triggered)
        {
            Close();
            return;
        }
        int n = data.entries.Count, prev = cursor;
        if (left.triggered) cursor = (cursor - 1 + n) % n;
        else if (right.triggered) cursor = (cursor + 1) % n;
        else if (up.triggered) cursor = Mathf.Max(0, cursor - Cols);
        else if (down.triggered) cursor = Mathf.Min(n - 1, cursor + Cols);
        if (cursor != prev)
        {
            Sfx.Play(SfxId.UiMove, 0.6f);
            Redraw();
        }
    }

    void Open()
    {
        if (data == null)
        {
            GameLog.Warn(GameLog.Cat.Item, "Resources/UI/BestiaryData 없음");
            return;
        }
        if (panel == null)
        {
            Build();
        }
        ScanSpawners();
        open = true;
        UiLock.Set("Bestiary", true);
        action.UI.Enable();
        panel.SetActive(true);
        Redraw();
        Sfx.Play(SfxId.UiOpen);
    }

    void Close()
    {
        open = false;
        UiLock.Set("Bestiary", false);
        action.UI.Disable();
        panel.SetActive(false);
    }

    /// <summary>스포너 → 몬스터 HP/방어·드롭표 (비활성 맵 포함)</summary>
    void ScanSpawners()
    {
        infos.Clear();
        var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var infosField = typeof(EnemySpawner).GetField("objectInfos", bf);
        foreach (EnemySpawner sp in FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!(infosField?.GetValue(sp) is System.Collections.IEnumerable list))
            {
                continue;
            }
            foreach (object o in list)
            {
                var key = (string)o.GetType().GetField("objectName").GetValue(o);
                var prefab = (GameObject)o.GetType().GetField("perfab").GetValue(o);
                if (key != "enemy" || prefab == null || !prefab.TryGetComponent(out EnemyHealth eh))
                {
                    continue;
                }
                if (!infos.TryGetValue(prefab.name, out Info info))
                {
                    info = new Info { hp = eh.maxHP, def = eh.enemyDef };
                    infos[prefab.name] = info;
                }
                DropManager dm = sp.GetComponent<DropManager>();
                if (dm != null)
                {
                    foreach (var d in dm.AllDrops())
                    {
                        if (!info.drops.Exists(x => x.name == d.name))
                        {
                            info.drops.Add(d);
                        }
                    }
                }
            }
        }
    }

    void Redraw()
    {
        UiTheme theme = UiTheme.Get();
        for (int i = 0; i < cells.Count; i++)
        {
            bool has = i < data.entries.Count;
            cells[i].bg.gameObject.SetActive(has);
            if (!has)
            {
                continue;
            }
            BestiaryData.Entry e = data.entries[i];
            bool seen = BestiaryRecord.Kills(e.id) > 0;
            cells[i].icon.sprite = e.icon;
            cells[i].icon.color = seen ? Color.white : new Color(0f, 0f, 0f, 0.85f); // 미발견 = 실루엣
            cells[i].bg.color = i == cursor ? Color.white : new Color(0.55f, 0.55f, 0.6f, 1f);
        }
        if (data.entries.Count == 0)
        {
            return;
        }
        BestiaryData.Entry cur = data.entries[cursor];
        int kills = BestiaryRecord.Kills(cur.id);
        bigIcon.sprite = cur.icon;
        bigIcon.color = kills > 0 ? Color.white : new Color(0f, 0f, 0f, 0.85f);
        string tag = cur.isBoss ? " <color=#FF6060>" + Loc.T("BOSS") + "</color>" : cur.isElite ? " <color=#FFC640>" + Loc.T("ELITE") + "</color>" : "";
        title.text = (kills > 0 ? Loc.T(cur.displayName) : "???") + tag;

        var sb = new System.Text.StringBuilder();
        sb.Append(Loc.T("Region")).Append("  ").Append(kills > 0 ? Loc.Region(cur.region) : "???").Append('\n');
        sb.Append(Loc.T("Defeated")).Append("  ").Append(kills).Append("\n\n");
        infos.TryGetValue(cur.id, out Info info);
        if (kills >= BestiaryRecord.RevealStats && info != null)
        {
            sb.Append(Loc.T("HP")).Append(" ").Append(info.hp).Append("   ").Append(Loc.T("DEF")).Append(" ").Append(info.def).Append("\n\n");
        }
        else
        {
            sb.Append("<color=#999999>").Append(Loc.F("Stats: defeat {0} more", BestiaryRecord.RevealStats - kills)).Append("</color>\n\n");
        }
        if (kills >= BestiaryRecord.RevealDrops && info != null)
        {
            sb.Append(Loc.T("Drops")).Append("\n");
            foreach (var d in info.drops)
            {
                int idx = DatabaseManager.instance.FindItemDataIndex(d.name);
                string rarity = idx >= 0 ? DatabaseManager.instance.LoadItemData(idx).rarity : "C";
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(theme.RarityColor(rarity))).Append('>')
                  .Append(idx >= 0 ? DatabaseManager.instance.LoadItemData(idx).itemNameT : d.name).Append("</color>  ").Append((d.chance * 100f).ToString("0.#")).Append("%\n");
            }
        }
        else
        {
            sb.Append("<color=#999999>").Append(Loc.F("Drops: defeat {0} more", Mathf.Max(0, BestiaryRecord.RevealDrops - kills))).Append("</color>");
        }
        body.text = sb.ToString();
    }

    void Build()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 70;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        panel = theme.NewPanel("Panel", transform).gameObject;
        var pr = (RectTransform)panel.transform;
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(1180f, 640f);

        var head = NewText("Title", pr, theme, 42f);
        Place(head.rectTransform, new Vector2(30f, -18f), new Vector2(600f, 54f));
        head.text = Loc.T("BESTIARY");
        head.color = theme.unique;
        var hint = NewText("Hint", pr, theme, 22f);
        Place(hint.rectTransform, new Vector2(30f, -600f), new Vector2(1100f, 30f));
        hint.text = Loc.T("ARROWS select    B / BACK close");
        hint.color = new Color(1f, 1f, 1f, 0.55f);

        for (int i = 0; i < 16; i++)
        {
            Image bg = theme.NewPanel("Cell" + i, pr, true);
            Place(bg.rectTransform, new Vector2(30f + (i % Cols) * (CellSize + 8), -86f - (i / Cols) * (CellSize + 8)), new Vector2(CellSize, CellSize));
            Image icon = NewImage("Icon", bg.transform, Color.white);
            icon.rectTransform.anchorMin = Vector2.zero;
            icon.rectTransform.anchorMax = Vector2.one;
            icon.rectTransform.offsetMin = new Vector2(10f, 10f);
            icon.rectTransform.offsetMax = new Vector2(-10f, -10f);
            icon.preserveAspect = true;
            cells.Add((bg, icon));
        }

        bigIcon = NewImage("BigIcon", pr, Color.white);
        Place(bigIcon.rectTransform, new Vector2(520f, -86f), new Vector2(160f, 160f));
        bigIcon.preserveAspect = true;
        title = NewText("Name", pr, theme, 36f);
        Place(title.rectTransform, new Vector2(700f, -100f), new Vector2(460f, 50f));
        body = NewText("Body", pr, theme, 26f);
        Place(body.rectTransform, new Vector2(520f, -270f), new Vector2(640f, 320f));
        body.alignment = TextAlignmentOptions.TopLeft;
        body.enableWordWrapping = true;
        panel.SetActive(false);
    }

    static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static Image NewImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static TextMeshProUGUI NewText(string name, Transform parent, UiTheme theme, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = size;
        t.raycastTarget = false;
        t.enableWordWrapping = false;
        return t;
    }
}
