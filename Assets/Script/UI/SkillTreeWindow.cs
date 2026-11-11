using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 스킬창 (WeaponSkillProgression.md V.7). K 키 / 패드 LT+Y 로 연다. 탭 [무기 타입][전투][생활][편의성],
/// 노드 행 투자(+), 트리별 초기화. 마우스(탭·행·+·버튼), 키보드(↑↓ Enter R Tab ←→), 패드(십자 A X LB/RB Start)를 모두 지원한다.
/// </summary>
public sealed class SkillTreeWindow : MonoBehaviour
{
    const string LockOwner = "SkillTree";
    static SkillTreeWindow _instance;

    readonly SkillTreeWindowView _v = new SkillTreeWindowView();
    KeyAction _action;
    InputAction _up, _down, _left, _right, _confirm, _back;
    bool _open;
    bool _built;
    bool _respecAsk;
    SkillTab _tab;
    int _cursor;
    WeaponKind _kind;
    string _notice = "";
    List<SkillNode> _nodes = new List<SkillNode>();

    public static bool IsOpen
    {
        get { return _instance != null && _instance._open; }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (_instance == null)
        {
            var go = new GameObject("[SkillTreeWindow]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<SkillTreeWindow>();
        }
    }

    public static void Toggle()
    {
        if (_instance == null)
        {
            return;
        }
        if (_instance._open)
        {
            _instance.Close();
        }
        else if (CombatState.IsInCombat())
        {
            PickupLog.Notice("전투 중에는 스킬창을 열 수 없습니다");
        }
        else
        {
            _instance.Open();
        }
    }

    /// <summary>프로브·테스트용: 특정 탭으로 연다</summary>
    public static void OpenAt(SkillTab tab)
    {
        if (_instance != null && !_instance._open)
        {
            _instance.Open();
        }
        if (_instance != null)
        {
            _instance.SetTab(tab);
        }
    }

    void Awake()
    {
        _action = new KeyAction();
        _up = _action.UI.UPInventory;
        _down = _action.UI.DownInventory;
        _left = _action.UI.LeftInventory;
        _right = _action.UI.RightInventory;
        _confirm = _action.UI.Enter;
        _back = _action.UI.Back;
    }

    void OnDestroy()
    {
        if (_open)
        {
            UiLock.Set(LockOwner, false);
        }
        _action?.Dispose();
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;
        bool padOpen = pad != null && PadFieldInputRules.IsLtChord(pad.leftTrigger.isPressed, pad.buttonNorth.wasPressedThisFrame);
        if ((Hotkeys.Pressed(Hotkeys.SkillWindow) || padOpen) && (_open || !DatabaseManager.isOpenUI))
        {
            Toggle();
            return;
        }
        if (!_open)
        {
            return;
        }
        bool back = _back.triggered || (pad != null && pad.startButton.wasPressedThisFrame);
        if (UiLock.TryBack(LockOwner, back))
        {
            if (_respecAsk)
            {
                CancelRespec();
            }
            else
            {
                Close();
            }
            return;
        }
        bool confirm = _confirm.triggered || (pad != null && pad.buttonSouth.wasPressedThisFrame);
        if (_respecAsk)
        {
            if (confirm)
            {
                DoRespec();
            }
            else if (_up.triggered || _down.triggered || _left.triggered || _right.triggered)
            {
                CancelRespec();
            }
            return;
        }
        HandleNavigation(kb, pad);
        if (confirm)
        {
            Invest(_cursor);
        }
        else if ((kb != null && kb.rKey.wasPressedThisFrame) || (pad != null && pad.buttonWest.wasPressedThisFrame))
        {
            AskRespec();
        }
    }

    void HandleNavigation(Keyboard kb, Gamepad pad)
    {
        int tabStep = (_left.triggered || (pad != null && pad.leftShoulder.wasPressedThisFrame)) ? -1
            : (_right.triggered || (kb != null && kb.tabKey.wasPressedThisFrame) || (pad != null && pad.rightShoulder.wasPressedThisFrame)) ? 1 : 0;
        if (tabStep != 0)
        {
            SetTab((SkillTab)PadFieldInputRules.StepTab((int)_tab, 4, tabStep));
            return;
        }
        int n = _nodes.Count;
        float wheel = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
        int prev = _cursor;
        if (n > 0 && (_up.triggered || wheel > 0.5f))
        {
            _cursor = (_cursor - 1 + n) % n;
        }
        else if (n > 0 && (_down.triggered || wheel < -0.5f))
        {
            _cursor = (_cursor + 1) % n;
        }
        if (_cursor != prev)
        {
            _notice = "";
            Sfx.Play(SfxId.UiMove, 0.6f);
            Redraw();
        }
    }

    void Open()
    {
        if (!_built)
        {
            _v.Build(gameObject, i => SetTab((SkillTab)i), OnRowClick, OnPlusClick, OnRespecClick, Close);
            _built = true;
        }
        _kind = SkillTreeEffects.CurrentKind();
        _open = true;
        _respecAsk = false;
        _notice = "";
        _cursor = 0;
        _tab = _kind == WeaponKind.None ? SkillTab.Combat : SkillTab.Type;
        UiLock.Set(LockOwner, true);
        _action.UI.Enable();
        _v.Panel.SetActive(true);
        RebuildNodes();
        Redraw();
        Sfx.Play(SfxId.UiOpen);
    }

    void Close()
    {
        _open = false;
        _respecAsk = false;
        UiLock.Set(LockOwner, false);
        _action.UI.Disable();
        _v.Panel.SetActive(false);
    }

    void SetTab(SkillTab tab)
    {
        _tab = tab;
        _cursor = 0;
        _notice = "";
        _respecAsk = false;
        RebuildNodes();
        Sfx.Play(SfxId.UiMove, 0.6f);
        Redraw();
    }

    void RebuildNodes()
    {
        _nodes = (_tab == SkillTab.Type && _kind == WeaponKind.None) ? new List<SkillNode>() : SkillTreeRules.NodesOf(_tab);
    }

    string Tree
    {
        get { return _tab == SkillTab.Type ? SkillTreeService.TreeOf(_kind) : SkillTreeRules.CommonTree; }
    }

    void OnRowClick(int idx)
    {
        ClearUiFocus();
        if (idx >= _nodes.Count || _respecAsk)
        {
            return;
        }
        _cursor = idx;
        _notice = "";
        Sfx.Play(SfxId.UiMove, 0.6f);
        Redraw();
    }

    void OnPlusClick(int idx)
    {
        ClearUiFocus();
        if (!_respecAsk)
        {
            _cursor = Mathf.Min(idx, Mathf.Max(0, _nodes.Count - 1));
            Invest(idx);
        }
    }

    void OnRespecClick()
    {
        ClearUiFocus();
        if (_respecAsk)
        {
            DoRespec();
        }
        else
        {
            AskRespec();
        }
    }

    static void ClearUiFocus()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // 클릭한 버튼에 포커스가 남아 Enter 가 한 번 더 눌리는 것을 막는다
        }
    }

    void Invest(int idx)
    {
        string tree = Tree;
        if (tree == null || idx < 0 || idx >= _nodes.Count)
        {
            return;
        }
        SkillNode n = _nodes[idx];
        InvestResult r = SkillTreeService.Invest(tree, n.Id);
        if (r == InvestResult.Ok)
        {
            _notice = n.Name + " Lv" + SkillTreeService.LevelOf(tree, n.Id);
            Sfx.Play(SfxId.UiOpen, 0.7f);
        }
        else
        {
            _notice = "<color=#FF8070>" + SkillTreeText.InvestMessage(r, n) + "</color>";
            Sfx.Play(SfxId.UiMove, 0.4f);
        }
        Redraw();
    }

    void AskRespec()
    {
        string tree = Tree;
        if (tree == null || SkillTreeService.Spent(tree) <= 0)
        {
            _notice = "초기화할 포인트가 없습니다";
            Redraw();
            return;
        }
        _respecAsk = true;
        int cost = SkillTreeService.RespecCost(tree);
        string scope = _tab == SkillTab.Type ? SkillTreeText.KindName(_kind) + " 트리" : "공용 트리(전투·생활·편의성 전체)";
        _notice = "<color=#FFC640>" + scope + "를 초기화합니까? " + (cost > 0 ? cost + "G" : "무료") + " - Enter/A 확인, 다른 입력 취소</color>";
        Redraw();
    }

    void CancelRespec()
    {
        _respecAsk = false;
        _notice = "";
        Redraw();
    }

    void DoRespec()
    {
        _respecAsk = false;
        RespecResult r = SkillTreeService.Respec(Tree);
        _notice = r == RespecResult.Ok ? "초기화했습니다. 포인트를 모두 돌려받았습니다"
            : r == RespecResult.NoGold ? "<color=#FF8070>골드가 부족합니다</color>" : "초기화할 포인트가 없습니다";
        Sfx.Play(SfxId.UiOpen, 0.7f);
        Redraw();
    }

    void Redraw()
    {
        UiTheme theme = UiTheme.Get();
        for (int i = 0; i < _v.Tabs.Length; i++)
        {
            _v.TabText[i].text = SkillTreeText.TabName((SkillTab)i, _kind);
            _v.TabText[i].color = i == (int)_tab ? theme.unique : Color.white;
        }
        string tree = Tree;
        DrawHeader(tree);
        for (int i = 0; i < SkillTreeWindowView.Rows; i++)
        {
            bool has = i < _nodes.Count;
            _v.RowButtons[i].gameObject.SetActive(has);
            if (has)
            {
                DrawRow(i, _nodes[i], tree);
            }
        }
        if (_nodes.Count == 0)
        {
            string empty = _tab == SkillTab.Life ? "생활 노드(채집·낚시·요리·제작)는 준비 중입니다"
                : _tab == SkillTab.Type ? "무기를 장착하면 그 무기 종류의 트리를 열 수 있습니다" : "";
            _v.Hint.text = _notice.Length > 0 ? empty + "   " + _notice : empty;
        }
        else
        {
            _v.Hint.text = _notice.Length > 0 ? _notice : "↑↓ 선택   Enter/A 투자   R/X 초기화   ←→·Tab·LB/RB 탭   K·Esc·Start 닫기";
        }
        DrawRespec(tree);
    }

    void DrawHeader(string tree)
    {
        int earned = tree != null ? SkillTreeService.EarnedPoints(tree) : 0;
        int avail = tree != null ? SkillTreeService.Available(tree) : 0;
        _v.Points.text = tree == null ? "" : "남은 포인트 <color=#FFC640>" + avail + "</color> / 획득 " + earned;
        if (_tab == SkillTab.Type)
        {
            if (_kind == WeaponKind.None)
            {
                _v.Header.text = "장착한 무기가 없습니다";
                return;
            }
            int exp = TypeMasteryService.ExpOf(_kind);
            int lv = TypeMasteryService.LevelOf(_kind);
            string expText = lv >= TypeMasteryRules.MaxLevel ? "최대" : "EXP " + TypeMasteryRules.ExpInLevel(exp) + "/" + TypeMasteryRules.ExpToNext(lv);
            _v.Header.text = SkillTreeText.KindName(_kind) + " 숙련  Lv" + lv + " " + SkillTreeText.TitleOf(lv) + "  " + expText;
            return;
        }
        _v.Header.text = "공용 성장  플레이어 Lv" + LevelSystem.Level + " (레벨당 1포인트, 세 탭 공유)";
    }

    void DrawRow(int i, SkillNode n, string tree)
    {
        int lv = SkillTreeService.LevelOf(tree, n.Id);
        bool prereqOk = n.PrereqId == null || SkillTreeService.LevelOf(tree, n.PrereqId) >= n.PrereqLevel;
        bool maxed = lv >= n.MaxLevel;
        string cost = n.CostPerLevel > 1 ? " (비용 " + n.CostPerLevel + ")" : "";
        string color = !prereqOk && lv == 0 ? "#808080" : maxed ? "#FFC640" : "#FFFFFF";
        _v.RowName[i].text = "<color=" + color + ">" + n.Name + "  " + lv + "/" + n.MaxLevel + cost + "</color>";
        string effect = prereqOk ? SkillTreeText.EffectLine(n, lv, _kind) : "<color=#FF8070>" + SkillTreeText.PrereqText(n) + "</color>";
        float modAtk = SkillTreeService.Mod(TreeStat.AttackDamage, _kind);
        float modSkl = SkillTreeService.Mod(TreeStat.SkillDamage, _kind);
        if (SkillTreeText.IsCapped(n.Stat, modAtk, modSkl, SkillTreeService.Mod(n.Stat, _kind)))
        {
            effect += "  <color=#FFC640>[상한]</color>";
        }
        _v.RowEffect[i].text = effect;
        bool canBuy = prereqOk && !maxed && SkillTreeService.Available(tree) >= n.CostPerLevel;
        _v.RowPlus[i].interactable = canBuy;
        _v.RowBg[i].color = i == _cursor ? new Color(1f, 0.85f, 0.4f, 0.5f) : Color.white;
    }

    void DrawRespec(string tree)
    {
        int spent = tree != null ? SkillTreeService.Spent(tree) : 0;
        int cost = tree != null ? SkillTreeService.RespecCost(tree) : 0;
        _v.RespecText.text = spent <= 0 ? "초기화" : cost > 0 ? "초기화 " + cost + "G" : "초기화 (무료)";
        _v.Respec.interactable = spent > 0;
    }
}
