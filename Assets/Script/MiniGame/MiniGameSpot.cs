using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>미니게임 보상 희귀도(ItemSheet rarity 와 같은 글자). 낚시·채집·채굴 공용</summary>
public enum ItemRarity { C, R, U }

/// <summary>
/// 생활 미니게임 지점 공용 틀(MiniGames.md "공용 베이스"): 낚시터(FishingSpot)·채집 지점(GatherNode)이 물려받는다.
/// 트리거 안에서 머리 위 안내 → ↑(KeyAction UI/Up, NPC 대화·게시판과 같은 키) 로 시작 → UiLock 으로 조작을 막고,
/// 좌우 이동·경직 피격(isAttacked)·사망·트리거 이탈이면 중단(결과 없이 바로 해제). 결과는 ResultHold 초 또는 ↑ 로 닫는다.
/// 한 번에 하나만 진행(Active). 보상은 GiveReward 한 곳에서 InventoryService.Add 로 지급한다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class MiniGameSpot : MonoBehaviour
{
    private const float PromptHeight = 2.2f;
    private const float ResultSkipAfter = 0.25f;

    private static KeyAction _input;
    private static int _enabledSpots;

    private TextMeshPro _prompt;
    private bool _playerNear;
    private bool _granted;

    /// <summary>지금 진행 중인 지점(종류 무관, 없으면 null)</summary>
    public static MiniGameSpot Active { get; private set; }

    /// <summary>결과 단계에서 가방이 가득 차 보상을 못 받았는가</summary>
    public bool BagFull { get; private set; }

    /// <summary>머리 위 안내 문구 키(Loc)</summary>
    protected abstract string PromptKey { get; }
    /// <summary>UiLock 주인 이름</summary>
    protected abstract string LockOwner { get; }
    /// <summary>결과 표시 시간(초)</summary>
    protected abstract float ResultHold { get; }
    /// <summary>세션이 있는가(시작했고 아직 End 전)</summary>
    protected abstract bool HasSession { get; }
    /// <summary>결과가 났는가</summary>
    protected abstract bool SessionDone { get; }
    /// <summary>중단으로 끝났는가</summary>
    protected abstract bool SessionCancelled { get; }
    /// <summary>결과가 난 뒤 경과 초</summary>
    protected abstract float ResultElapsed { get; }

    /// <summary>시작 가능 여부 추가 조건(쿨타임 등)</summary>
    protected virtual bool Ready => true;

    protected abstract void StartSession();
    protected abstract void TickSession(float dt, bool held, bool pressed);
    protected abstract void CancelSession();
    protected abstract void ClearSession();
    /// <summary>결과가 난 첫 프레임에 한 번. 성공이면 GiveReward 를 부른다</summary>
    protected abstract void OnResult();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Active = null;
        _enabledSpots = 0;
        if (_input != null)
        {
            _input.Disable();
            _input.Dispose();
            _input = null;
        }
    }

    /// <summary>모든 지점이 같이 쓰는 입력(↑·좌우). 켜진 지점이 하나라도 있을 때만 켠다(OnEnable/OnDisable 짝)</summary>
    protected static KeyAction SharedInput => _input ?? (_input = new KeyAction());

    protected virtual void Awake()
    {
        BuildPrompt();
    }

    protected virtual void OnEnable()
    {
        if (_enabledSpots++ == 0)
        {
            SharedInput.UI.Up.Enable();
            SharedInput.Player.Move.Enable();
        }
    }

    protected virtual void OnDisable()
    {
        End();
        _playerNear = false;
        if (_enabledSpots > 0 && --_enabledSpots == 0 && _input != null)
        {
            _input.UI.Up.Disable();
            _input.Player.Move.Disable();
        }
    }

    protected virtual void Update()
    {
        if (Time.timeScale <= 0f)
        {
            return; // 일시정지 메뉴의 ↑ 입력이 새지 않게
        }
        InputAction up = SharedInput.UI.Up;
        if (!HasSession)
        {
            UpdateIdle(up.WasPressedThisFrame());
            return;
        }
        if (!SessionDone && ShouldCancel())
        {
            CancelSession();
        }
        bool pressed = up.WasPressedThisFrame();
        TickSession(Time.deltaTime, up.IsPressed(), pressed);
        if (SessionDone)
        {
            UpdateResult(pressed);
        }
    }

    private void UpdateIdle(bool pressed)
    {
        bool can = _playerNear && CanBegin();
        if (_prompt.gameObject.activeSelf != can)
        {
            _prompt.gameObject.SetActive(can);
        }
        if (can && pressed)
        {
            Begin();
        }
    }

    /// <summary>시작 조건(근접 제외): Ready·다른 미니게임/UI 없음·대화 중 아님(대화도 같은 ↑ 키)</summary>
    public bool CanBegin()
    {
        return Ready && Active == null && !DatabaseManager.isOpenUI
            && !PixelCrushers.DialogueSystem.DialogueManager.isConversationActive;
    }

    /// <summary>시작(프로브용 공개 창구). 조건이 안 되면 false</summary>
    public bool Begin()
    {
        if (!CanBegin())
        {
            return false;
        }
        StartSession();
        Active = this;
        _granted = false;
        BagFull = false;
        _prompt.gameObject.SetActive(false);
        UiLock.Set(LockOwner, true);
        return true;
    }

    private bool ShouldCancel()
    {
        PlayerController pc = PlayerController.instance;
        if (pc == null || pc.isAttacked)
        {
            return true;
        }
        PlayerHealthManager hp = PlayerHealthManager.Instance;
        if (hp != null && hp.IsDead)
        {
            return true;
        }
        return Mathf.Abs(SharedInput.Player.Move.ReadValue<float>()) > 0.5f; // Move 는 1D 축(좌우)
    }

    private void UpdateResult(bool pressed)
    {
        if (SessionCancelled)
        {
            End(); // 이동으로 중단했으면 바로 움직일 수 있게
            return;
        }
        if (!_granted)
        {
            _granted = true;
            OnResult();
        }
        if (ResultElapsed >= ResultHold || (pressed && ResultElapsed > ResultSkipAfter))
        {
            End();
        }
    }

    /// <summary>보상 지급(유일한 지급 창구). 가방이 가득 차면 BagFull·알림 후 false</summary>
    protected bool GiveReward(string id, int count, AddSource source, ItemRarity rarity)
    {
        InventoryManager inv = InventoryManager.instance;
        if (inv == null || !inv.Service.Add(id, count, source))
        {
            BagFull = true;
            PickupLog.ReportFull();
            return false;
        }
        inv.SaveInventory();
        PickupLog.Report(ItemText.DisplayName(id), count, rarity.ToString());
        GameLog.Info(GameLog.Cat.Item, source + ": " + id + " x" + count);
        return true;
    }

    /// <summary>종료·잠금 해제(결과 표시가 끝났거나 지점이 꺼질 때)</summary>
    public void End()
    {
        if (Active == this)
        {
            Active = null;
            UiLock.Set(LockOwner, false);
        }
        ClearSession();
    }

    /// <summary>안내 문구를 다시 번역(종류가 Awake 뒤에 정해질 때)</summary>
    protected void RefreshPrompt()
    {
        if (_prompt != null)
        {
            _prompt.text = Loc.T(PromptKey);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _playerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _playerNear = false;
            if (HasSession && !SessionDone)
            {
                CancelSession();
            }
        }
    }

    private void BuildPrompt()
    {
        var go = new GameObject("SpotPrompt");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, PromptHeight, 0f);
        _prompt = go.AddComponent<TextMeshPro>();
        UiTheme theme = UiTheme.Get();
        if (theme.pixelFont != null)
        {
            _prompt.font = theme.pixelFont;
        }
        _prompt.text = Loc.T(PromptKey);
        _prompt.fontSize = 3f;
        _prompt.alignment = TextAlignmentOptions.Center;
        _prompt.color = theme.unique;
        _prompt.rectTransform.sizeDelta = new Vector2(6f, 1f);
        _prompt.sortingOrder = 500;
        go.SetActive(false);
    }

    protected static float RandomValue()
    {
        return Random.value;
    }
}
