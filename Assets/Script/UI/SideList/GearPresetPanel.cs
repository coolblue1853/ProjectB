using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 장비 프리셋 목록(BasicFeatures.md 13절): 인벤토리만 열려 있을 때 창 옆에 프리셋 1~3 의 부위 수·무기 이름·키 힌트를 보여 준다.
/// 강조 줄 = GearPresets.Selected(패드 RT·길게 SELECT 의 대상). 실행은 GearPresets 가 한다.
/// </summary>
public class GearPresetPanel : SideListPanel
{
    private static GearPresetPanel _instance;
    private const int NameMax = 16;

    private readonly StringBuilder _sb = new StringBuilder();
    private int _version = -1, _selected = -1;
    private bool _korean;
    private Transform _root;
    private RectTransform _anchor;

    public static GearPresetPanel Instance => _instance;

    protected override float Width => 400f;

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
        var go = new GameObject("[GearPresetPanel]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<GearPresetPanel>();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected override bool Visible()
    {
        return GearPresets.Model != null && GearPresets.CanUse();
    }

    protected override RectTransform Anchor()
    {
        // 장비칸 배경(씬 계층 이름 "BackGround"). 없으면 인벤 루트. 루트가 바뀔 때만 다시 찾는다
        Transform root = InventoryManager.instance.inventory.transform;
        if (root != _root || _anchor == null)
        {
            _root = root;
            Transform equipBg = root.Find("BackGround");
            _anchor = (equipBg != null ? equipBg : root) as RectTransform;
        }
        return _anchor;
    }

    protected override bool Changed()
    {
        int v = GearPresets.Model.Version;
        int sel = GearPresets.Selected;
        bool ko = Loc.IsKorean;
        if (v == _version && sel == _selected && ko == _korean)
        {
            return false;
        }
        _version = v;
        _selected = sel;
        _korean = ko;
        return true;
    }

    protected override void Render(TextMeshProUGUI title, TextMeshProUGUI body)
    {
        GearPresetModel model = GearPresets.Model;
        title.text = Loc.T("GEAR PRESETS");
        _sb.Clear();
        for (int i = 0; i < GearPresetModel.PresetCount; i++)
        {
            _sb.Append(i == GearPresets.Selected ? "<color=#FFD040>> </color>" : "   ");
            _sb.Append("<color=#9ED0FF>[").Append(7 + i).Append("]</color> ");
            IReadOnlyDictionary<string, GearPresetModel.Entry> p = model.Get(i);
            if (p.Count == 0)
            {
                _sb.Append("<color=#808080>").Append(Loc.T("(empty)")).Append("</color>\n");
                continue;
            }
            _sb.Append(p.Count).Append('/').Append(GearPresetModel.Areas.Length).Append("  ");
            if (p.TryGetValue("Weapon", out GearPresetModel.Entry w))
            {
                ItemData d = LookUp(w.Name);
                _sb.Append(d != null ? TooltipBuilder.Name(Shorten(d.itemNameT, NameMax), d.rarity, w.Upgrade) : w.Name);
            }
            else
            {
                _sb.Append("<color=#808080>").Append(Loc.T("(no weapon)")).Append("</color>");
            }
            _sb.Append('\n');
        }
        _sb.Append("<size=80%><color=#A0A0A0>").Append(Loc.T("7/8/9 equip  ·  Shift+7/8/9 save")).Append('\n')
            .Append(Loc.T("Pad: SELECT pick (hold: save)  ·  RT equip")).Append("</color></size>");
        body.text = _sb.ToString();
    }
}
