using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 세트 창(REWORK-12 P6): 장비창 추가 칸 줄의 "SET" 칸에 마우스를 올리거나 키보드·패드 커서를 두면 뜨는 착용 세트 요약.
/// 세트마다 기존 툴팁 세트 단락(InventoryManager.SetEffectText — 단계별 켜짐 세트색/회색 + 특수 효과 줄)을 그대로 쓰고,
/// 목록·순서·다음 단계는 순수 규칙 SetOverview 가 정한다. 새 그래픽 없이 반투명 판 + 기본 TMP 글꼴(추가 칸 패널과 같은 스킨).
/// </summary>
public sealed class SetOverviewPopup : MonoBehaviour
{
    const float Width = 290f, Pad = 6f;
    static readonly Dictionary<string, ItemData> RepCache = new Dictionary<string, ItemData>();
    static object _repCacheSource;
    static int _repCacheCount; // 캐시를 만든 시점의 DB 아이템 목록(재로드되면 참조가 달라져 비운다)

    TextMeshProUGUI _text;
    RectTransform _rt;

    public static SetOverviewPopup Create(Transform parent)
    {
        var go = new GameObject("SetOverviewPopup", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image bg = go.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.85f);
        bg.raycastTarget = false;
        var popup = go.AddComponent<SetOverviewPopup>();
        popup.Build();
        go.SetActive(false);
        return popup;
    }

    private void Build()
    {
        _rt = (RectTransform)transform;
        _rt.anchorMin = _rt.anchorMax = _rt.pivot = new Vector2(0f, 0f);
        _rt.anchoredPosition = new Vector2(0f, 64f); // 추가 칸 패널 위쪽(장비 격자 위에 툴팁처럼 겹침)
        var tgo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        tgo.transform.SetParent(transform, false);
        _text = tgo.GetComponent<TextMeshProUGUI>();
        _text.fontSize = 10f;
        _text.raycastTarget = false;
        _text.enableWordWrapping = true;
        RectTransform r = _text.rectTransform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 1f);
        r.anchoredPosition = new Vector2(Pad, -Pad);
        r.sizeDelta = new Vector2(Width - Pad * 2f, 10f);
    }

    public void Show()
    {
        _text.text = BuildText();
        float h = _text.GetPreferredValues(_text.text, Width - Pad * 2f, 0f).y;
        _text.rectTransform.sizeDelta = new Vector2(Width - Pad * 2f, h);
        _rt.sizeDelta = new Vector2(Width, h + Pad * 2f);
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    private void Update()
    {
        if (EffectDetailMode.InputAllowed() && EffectDetailMode.Poll())
        {
            Show(); // 효과 상세 토글: 다시 그린다
        }
    }

    public bool IsShown => gameObject.activeSelf;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>요약 문구: 머리줄(켜진 단계 합) + 세트마다 툴팁 세트 단락 + 다음 단계 안내</summary>
    public static string BuildText()
    {
        var sb = new StringBuilder();
        List<SetOverviewEntry> list = SetOverview.Build(DatabaseManager.setsEffectStack, LoadTable());
        sb.Append("<color=#FFD480>").Append(Loc.T("Worn Set Effects")).Append("</color>");
        if (list.Count == 0)
        {
            return sb.Append("\n<color=").Append(TooltipBuilder.ColorOff).Append(">").Append(Loc.T("No set worn")).Append("</color>").ToString();
        }
        sb.Append(" <size=80%>").Append(Loc.F("(active tiers {0})", SetOverview.TotalActiveTiers(list))).Append("</size>\n");
        foreach (SetOverviewEntry e in list)
        {
            ItemData rep = RepresentativeOf(e.SetKey);
            if (rep != null && InventoryManager.instance != null)
            {
                sb.Append(InventoryManager.instance.SetEffectText(rep));
            }
            else
            {
                sb.Append("< ").Append(e.SetKey).Append(" > (").Append(e.Worn).Append(")\n");
            }
            sb.Append("<size=80%><color=").Append(TooltipBuilder.ColorOff).Append(">");
            sb.Append(e.NextTier > 0 ? Loc.F("Next tier ({0}): {1} more pieces", e.NextTier, e.NeedForNext) : Loc.T("All tiers active"));
            sb.Append("</color></size>\n");
        }
        return sb.ToString().TrimEnd();
    }

    /// <summary>착용 중인 세트 키의 단계 표(SetItem.csv "키_단계")</summary>
    private static SetTierTable LoadTable()
    {
        var table = new SetTierTable();
        foreach (KeyValuePair<string, int> pair in DatabaseManager.setsEffectStack)
        {
            if (string.IsNullOrEmpty(pair.Key) || pair.Value <= 0)
            {
                continue;
            }
            for (int i = 1; i <= SetTierTable.MaxTier; i++)
            {
                string effect = SetItem.SetEffectStr(pair.Key, i);
                if (!string.IsNullOrEmpty(effect))
                {
                    table.Define(pair.Key, i, effect);
                }
            }
        }
        return table;
    }

    /// <summary>세트 키를 가진 장비 하나(세트 이름·부위 수 표시용)</summary>
    private static ItemData RepresentativeOf(string setKey)
    {
        if (DatabaseManager.instance == null || DatabaseManager.instance.Items == null)
        {
            return null;
        }
        if (!ReferenceEquals(_repCacheSource, DatabaseManager.instance.Items) || _repCacheCount != DatabaseManager.instance.Items.Count)
        {
            RepCache.Clear(); // DB 재로드: 옛 ItemData 참조 폐기
            _repCacheSource = DatabaseManager.instance.Items;
            _repCacheCount = DatabaseManager.instance.Items.Count;
        }
        if (RepCache.TryGetValue(setKey, out ItemData d))
        {
            return d;
        }
        foreach (ItemData item in DatabaseManager.instance.Items)
        {
            if (item.type == "Equip" && item.tfName == setKey)
            {
                d = item;
                break;
            }
        }
        if (d != null)
        {
            RepCache[setKey] = d; // 못 찾은 키는 캐시하지 않는다(DB 로드 전 호출 대비)
        }
        return d;
    }
}
