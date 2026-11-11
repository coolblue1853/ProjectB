using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 퀘스트 창 (REWORK-5 P2): 받을 수 있는 / 진행 중 / 완료 3탭, 구역별 묶음, 우측 상세(요약·요구 사항·보상·안내).
/// 일반 모드(Q 키·패드 SELECT 길게): 보기·추적·포기. 입구 모드(게시판·푯말 ↑): 같은 창에서 수락·보고도 가능, 목록은 입구 필터.
/// 조작 3경로 — 마우스: 탭·줄·버튼 클릭, 휠. 키보드: ↑↓ ←→ Z(추적·접기) Enter(입구 수락·보고) Del(포기) H(추적창 접기) Q/Esc.
/// 패드: D패드·스틱, LB/RB 탭, A·X 추적(입구는 A 수락·보고), Y 포기, LT 추적창 접기, B/Start 닫기.
/// 열려 있는 동안 UiLock "NpcQuest". 화면 구성은 QuestWindowView, 문구는 QuestWindowText.
/// </summary>
public partial class NpcQuestWindow : MonoBehaviour
{
    private const float ActGuard = 0.25f; // 열 때 누른 ↑·A 가 곧바로 수락으로 이어지지 않게

    private static NpcQuestWindow s_instance;

    private KeyAction _action;
    private InputAction _up, _down, _left, _right, _select, _confirm, _back;
    private readonly QuestWindowView _v = new QuestWindowView();
    private readonly List<Row> _rows = new List<Row>(); // 커서 단위: 구역 머리글(접기 대상) + 펼쳐진 구역의 퀘스트
    private QuestFilter _entrance; // null = 일반 모드
    private QuestListRules.Tab _tab;
    private int _cursor, _first;
    private bool _open;
    private float _openedAt;
    private NpcQuestDef _confirmTarget;
    private string _notice = "";

    private sealed class Row
    {
        public NpcQuestDef Quest; // null = 머리글
        public string Key;
        public string Text;
        public bool Folded;
        public int Count;
    }

    public static bool IsOpen => s_instance != null && s_instance._open;

    /// <summary>현재 열린 입구 필터 (테스트·프로브용). 일반 모드·닫힘이면 null</summary>
    public static QuestFilter EntranceFilter => IsOpen ? s_instance._entrance : null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_instance = null;
    }

    public static void Toggle()
    {
        if (s_instance == null)
        {
            return;
        }
        if (s_instance._open)
        {
            s_instance.Close();
        }
        else
        {
            s_instance.OpenWindow(null);
        }
    }

    /// <summary>입구 모드로 연다. giver(필터)에 맞는 퀘스트만, 수락·보고 버튼 활성 (QuestEntrance 가 부른다)</summary>
    public static bool OpenEntrance(QuestFilter filter)
    {
        if (s_instance == null || s_instance._open || DatabaseManager.isOpenUI)
        {
            return false;
        }
        s_instance.OpenWindow(filter);
        return true;
    }

    /// <summary>목표 문장: "슬라임 처치 3/8" 같은 한 줄 (QuestService.Objective 로 이동 — REWORK-5)</summary>
    public static string ObjectiveText(NpcQuestDef d)
    {
        return QuestService.Objective(d);
    }

    private void Awake()
    {
        s_instance = this;
        _action = new KeyAction();
        _up = _action.UI.UPInventory;
        _down = _action.UI.DownInventory;
        _left = _action.UI.LeftInventory;
        _right = _action.UI.RightInventory;
        _select = _action.UI.Select;
        _confirm = _action.UI.Enter;
        _back = _action.UI.Back;
    }

    private void OnDestroy()
    {
        _action?.Dispose();
    }

    private void OnEnable()
    {
        NpcQuestLog.Changed += OnChanged;
    }

    private void OnDisable()
    {
        NpcQuestLog.Changed -= OnChanged;
        Close();
    }

    private void OnChanged()
    {
        if (_open)
        {
            Redraw();
        }
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        Gamepad pad = Gamepad.current;
        PadFieldInput.Refresh();
        if (ConfirmPopup.IsBlocking)
        {
            return; // 확인창이 입력을 가진다(같은 프레임 닫힘 포함)
        }
        // 패드는 SELECT 길게(0.8초). R3 는 아티팩트 사용과 겹쳐서 뺐다(AUTO-49). 눌린 순간 UI 가 열려 있었으면(인벤 프리셋 저장 등) 무시
        bool padHold = PadFieldInput.Select == PadGesture.Hold && (_open || !PadFieldInput.SelectInUi);
        bool pressed = Hotkeys.Pressed(Hotkeys.QuestWindow) || padHold;
        if (pressed && (_open || !DatabaseManager.isOpenUI))
        {
            Toggle();
            return;
        }
        if (!_open)
        {
            return;
        }
        bool back = _back.triggered || (pad != null && pad.startButton.wasPressedThisFrame); // 패드 Start 로도 닫는다(T11-4)
        if (UiLock.TryBack("NpcQuest", back))
        {
            Close();
            return;
        }
        HandleNavigation(kb, pad);
        HandleActions(kb, pad);
    }

    private void HandleNavigation(Keyboard kb, Gamepad pad)
    {
        int n = _rows.Count;
        int prevCursor = _cursor;
        int tabStep = (_left.triggered || (pad != null && pad.leftShoulder.wasPressedThisFrame)) ? -1
            : (_right.triggered || (pad != null && pad.rightShoulder.wasPressedThisFrame)) ? 1 : 0;
        if (tabStep != 0)
        {
            SetTab(QuestListRules.Step(_tab, tabStep));
            return;
        }
        float wheel = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
        if (_up.triggered || wheel > 0.5f)
        {
            _cursor = n > 0 ? (_cursor - 1 + n) % n : 0;
        }
        else if (_down.triggered || wheel < -0.5f)
        {
            _cursor = n > 0 ? (_cursor + 1) % n : 0;
        }
        if (_cursor != prevCursor)
        {
            _notice = "";
            Sfx.Play(SfxId.UiMove, 0.6f);
            Redraw();
        }
    }

    private void HandleActions(Keyboard kb, Gamepad pad)
    {
        bool hudFold = (kb != null && kb.hKey.wasPressedThisFrame) || (pad != null && pad.leftTrigger.wasPressedThisFrame);
        if (hudFold)
        {
            NpcQuestTrack.SetHudFolded(!NpcQuestTrack.HudFolded);
            _notice = NpcQuestTrack.HudFolded ? "추적창을 접었습니다" : "추적창을 폈습니다";
            Redraw();
            return;
        }
        bool padX = pad != null && pad.buttonWest.wasPressedThisFrame;
        bool padY = pad != null && pad.buttonNorth.wasPressedThisFrame;
        bool del = kb != null && kb.deleteKey.wasPressedThisFrame;
        bool confirm = _confirm.triggered && Time.unscaledTime > _openedAt + ActGuard;
        bool select = _select.triggered && Time.unscaledTime > _openedAt + ActGuard;
        bool z = kb != null && kb.zKey.wasPressedThisFrame;
        Row row = CurrentRow();
        if (row == null)
        {
            return;
        }
        if (row.Quest == null)
        {
            if (select || confirm || padX || z)
            {
                ToggleFold(row);
            }
            return;
        }
        if (del || padY)
        {
            AskAbandon();
        }
        else if (_entrance != null ? (z || padX) : (select || padX))
        {
            ToggleTracking();
        }
        else if (_entrance != null && confirm)
        {
            Primary();
        }
    }

    // ───── 열기·닫기 ─────

    private void OpenWindow(QuestFilter entrance)
    {
        if (_v.Panel == null)
        {
            _v.Build(gameObject, i => SetTab((QuestListRules.Tab)i), OnRowClick, Primary, ToggleTracking, AskAbandon, Close);
        }
        _entrance = entrance;
        _open = true;
        _cursor = 0;
        _first = 0;
        _tab = entrance != null ? EntranceStartTab(entrance) : QuestListRules.Tab.Active;
        _notice = "";
        _openedAt = Time.unscaledTime;
        UiLock.Set("NpcQuest", true);
        _action.UI.Enable();
        _v.Panel.SetActive(true);
        Redraw();
        Sfx.Play(SfxId.UiOpen);
    }

    /// <summary>입구는 받을 게 있으면 받을 수 있는 탭, 아니면 진행 중 탭부터</summary>
    private static QuestListRules.Tab EntranceStartTab(QuestFilter f)
    {
        QuestFilter probe = Clone(f, QuestListRules.Tab.Available);
        if (QuestService.Query(probe).Items.Count > 0)
        {
            return QuestListRules.Tab.Available;
        }
        return QuestService.Query(Clone(f, QuestListRules.Tab.Active)).Items.Count > 0 ? QuestListRules.Tab.Active : QuestListRules.Tab.Available;
    }

    private void Close()
    {
        bool was = _open;
        _open = false;
        _entrance = null;
        UiLock.Set("NpcQuest", false);
        if (was)
        {
            _action.UI.Disable();
        }
        if (_v.Panel != null)
        {
            _v.Panel.SetActive(false);
        }
    }

    private static QuestFilter Clone(QuestFilter f, QuestListRules.Tab tab)
    {
        return new QuestFilter { Tab = tab, Giver = f?.Giver, Source = f?.Source, Zone = f?.Zone, ReportToo = f != null && f.ReportToo, ReportOnly = f != null && f.ReportOnly };
    }

    // ───── 동작 ─────

    private void SetTab(QuestListRules.Tab tab)
    {
        _tab = tab;
        _cursor = 0;
        _first = 0;
        _notice = "";
        Sfx.Play(SfxId.UiMove, 0.6f);
        Redraw();
    }

    private Row CurrentRow()
    {
        return _rows.Count == 0 ? null : _rows[Mathf.Clamp(_cursor, 0, _rows.Count - 1)];
    }

    private NpcQuestDef CurrentQuest()
    {
        return CurrentRow()?.Quest;
    }

    private void OnRowClick(int slot)
    {
        ClearUiFocus();
        int idx = _first + slot;
        if (idx >= _rows.Count)
        {
            return;
        }
        _cursor = idx;
        _notice = "";
        if (_rows[idx].Quest == null)
        {
            ToggleFold(_rows[idx]);
            return;
        }
        Sfx.Play(SfxId.UiMove, 0.6f);
        Redraw();
    }

    private static void ClearUiFocus()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // 클릭한 버튼에 포커스가 남아 Enter 가 한 번 더 눌리는 것을 막는다
        }
    }

    private void ToggleFold(Row row)
    {
        QuestFoldRules.Toggle(row.Key);
        _notice = "";
        Sfx.Play(SfxId.UiMove, 0.6f);
        Redraw();
    }

    private void ToggleTracking()
    {
        ClearUiFocus();
        NpcQuestDef cur = CurrentQuest();
        if (cur == null || _tab != QuestListRules.Tab.Active)
        {
            return;
        }
        List<NpcQuestDef> active = QuestService.ActiveAll();
        if (NpcQuestTrack.Toggle(cur.Id, active))
        {
            _notice = NpcQuestTrack.IsTracked(cur.Id, active) ? "<color=#FFD633>추적을 켰습니다</color>" : "<color=#BBBBBB>추적을 껐습니다</color>";
            Sfx.Play(SfxId.UiMove, 0.6f);
        }
        else
        {
            _notice = "<color=#FF7766>추적은 최대 " + NpcQuestTrack.Max + "개 - 다른 퀘스트의 추적을 먼저 끄세요</color>";
        }
        NpcQuestLog.NotifyTrackingChanged(); // 표시 갱신(Redraw 포함) + 저장
        Redraw();
    }

    private void AskAbandon()
    {
        ClearUiFocus();
        NpcQuestDef cur = CurrentQuest();
        if (cur == null || _tab != QuestListRules.Tab.Active)
        {
            return;
        }
        _confirmTarget = cur;
        Sfx.Play(SfxId.UiMove, 0.6f);
        ConfirmPopup.Open(new ConfirmRequest
        {
            Title = "퀘스트 포기",
            Message = "<color=#FFD633>" + cur.Title + "</color>\n퀘스트를 포기할까요?\n<color=#BBBBBB>진행 기록이 지워집니다. 아이템·장비는 그대로입니다.</color>",
            YesLabel = "포기",
            NoLabel = "취소",
            OnYes = DoAbandon,
            OnNo = CancelConfirm,
        });
    }

    private void CancelConfirm()
    {
        ClearUiFocus();
        _confirmTarget = null;
    }

    private void DoAbandon()
    {
        ClearUiFocus();
        NpcQuestDef d = _confirmTarget;
        _confirmTarget = null;
        if (d != null && QuestService.Abandon(d.Id))
        {
            _notice = "<color=#BBBBBB>포기했습니다: " + d.Title + "</color>";
            Sfx.Play(SfxId.UiMove, 0.6f);
        }
        Redraw();
    }

    /// <summary>입구 모드의 수락·보고 (일반 모드에서는 버튼이 없다)</summary>
    private void Primary()
    {
        ClearUiFocus();
        NpcQuestDef d = CurrentQuest();
        if (d == null || _entrance == null)
        {
            return;
        }
        NpcQuestStatus s = QuestService.StatusOf(d);
        if (_tab == QuestListRules.Tab.Available)
        {
            if (!QuestService.CanAccept(d, out string reason))
            {
                _notice = "<color=#FF7766>" + reason + "</color>";
            }
            else if (QuestService.Accept(d.Id))
            {
                _notice = "<color=#FFD633>수락: " + d.Title + "</color>";
                PickupLog.Notice("퀘스트 수락 - " + d.Title);
                Sfx.Play(SfxId.UiOpen, 0.6f);
            }
        }
        else if (_tab == QuestListRules.Tab.Active)
        {
            if (s != NpcQuestStatus.Ready)
            {
                _notice = "<color=#FF9966>아직 목표를 달성하지 못했습니다</color>";
            }
            else if (!QuestEntrance.CanReportHere(d, _entrance))
            {
                _notice = "<color=#FF9966>" + QuestService.ReportTarget(d) + "에게 보고해야 합니다</color>";
            }
            else if (QuestService.Report(d.Id))
            {
                _notice = "<color=#88DD88>보고 완료: " + d.Title + "</color>";
                Sfx.Play(SfxId.UiOpen, 0.6f);
            }
            else
            {
                _notice = "<color=#FF7766>가방 공간이 부족합니다</color>";
            }
        }
        Redraw();
    }
}
