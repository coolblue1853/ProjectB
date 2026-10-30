using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 채집 지점(④ 표본 채집 / ⑤ 광맥 채굴, MiniGames.md "채집·채굴 구현"). 낚시터(FishingSpot)와 같은 틀:
/// 트리거 안에서 머리 위 안내 → ↑(KeyAction UI/Up) 시작 → UiLock "Gather" 로 조작 막음 → 이동·경직 피격·사망·이탈이면 중단.
/// 보상은 InventoryService.Add(AddSource.Gathering/Mining) 한 곳에서만 지급하고, 성공한 지점은 쿨타임 동안 흐리게 쉰다(세이브 안 함).
/// 처음 켜질 때 한 번 아래 지면(Ground)에 붙는다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GatherNode : MonoBehaviour
{
    public const string LockOwner = "Gather";
    private const float ResultHold = 1.4f;
    private const float PromptHeight = 2.2f;
    private const float SnapUp = 0.5f; // 머리 위 발판에 올라가지 않게 낮게(배치 y 는 잰 지면 높이)
    private const float SnapDown = 10f;

    [SerializeField] private GatherKind _kind;
    [SerializeField] private int _floor = 1;
    [SerializeField] private SpriteRenderer _mark;

    private KeyAction _action;
    private TextMeshPro _prompt;
    private bool _playerNear;
    private bool _granted;
    private bool _bagFull;
    private bool _snapped;
    private float _readyAt;
    private bool _shownReady = true;

    /// <summary>지금 진행 중인 지점(없으면 null). GatherUI 가 읽는다</summary>
    public static GatherNode Current { get; private set; }

    public GatherSession Session { get; private set; }
    public GatherKind Kind => _kind;
    public bool BagFull => _bagFull;
    public bool Ready => Time.time >= _readyAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Current = null;
    }

    /// <summary>배치기가 만든 직후 한 번 부른다(부모 맵이 꺼져 있으면 Awake 보다 먼저)</summary>
    public void Init(GatherKind kind, int floor, SpriteRenderer mark)
    {
        _kind = kind;
        _floor = floor;
        _mark = mark;
        if (_prompt != null)
        {
            _prompt.text = PromptText(); // 켜진 맵이면 Awake 가 Init 보다 먼저 돈다
        }
    }

    private string PromptText()
    {
        return Loc.T(_kind == GatherKind.Ore ? "UP  MINE" : "UP  GATHER");
    }

    private void Awake()
    {
        _action = new KeyAction();
        BuildPrompt();
    }

    private void OnEnable()
    {
        _action.UI.Up.Enable();
        _action.Player.Move.Enable();
    }

    private void OnDisable()
    {
        _action.UI.Up.Disable();
        _action.Player.Move.Disable();
        End();
        _playerNear = false;
    }

    private void OnDestroy()
    {
        _action?.Dispose();
    }

    private void Update()
    {
        if (!_snapped)
        {
            _snapped = true;
            SnapToGround();
        }
        if (Time.timeScale <= 0f)
        {
            return; // 일시정지 메뉴의 ↑ 입력이 새지 않게
        }
        if (Session == null)
        {
            UpdateIdle();
            return;
        }
        if (!Session.Done && ShouldCancel())
        {
            Session.Cancel();
        }
        bool pressed = _action.UI.Up.WasPressedThisFrame();
        Session.Tick(Time.deltaTime, pressed);
        if (Session.Done)
        {
            UpdateResult(pressed);
        }
    }

    private void UpdateIdle()
    {
        bool ready = Ready;
        if (_mark != null && ready != _shownReady)
        {
            _shownReady = ready;
            _mark.color = ready ? Color.white : new Color(0.45f, 0.45f, 0.5f, 0.55f);
        }
        bool can = _playerNear && CanBegin();
        if (_prompt.gameObject.activeSelf != can)
        {
            _prompt.gameObject.SetActive(can);
        }
        if (can && _action.UI.Up.WasPressedThisFrame())
        {
            Begin();
        }
    }

    /// <summary>시작(프로브용 공개 창구). 다른 채집 중이거나 쿨타임이면 false</summary>
    public bool Begin()
    {
        if (!CanBegin())
        {
            return false;
        }
        GatherDef[] table = GatherRules.TableFor(_kind, _floor);
        Session = _kind == GatherKind.Ore
            ? (GatherSession)new MiningSession(table, RandomValue)
            : new SpecimenSession(table, RandomValue);
        Current = this;
        _granted = false;
        _bagFull = false;
        _prompt.gameObject.SetActive(false);
        UiLock.Set(LockOwner, true);
        return true;
    }

    /// <summary>시작 조건(근접 제외): 쿨타임 끝·다른 미니게임/UI 없음·대화 중 아님(대화도 같은 ↑ 키)</summary>
    private bool CanBegin()
    {
        return Ready && Current == null && !DatabaseManager.isOpenUI
            && !PixelCrushers.DialogueSystem.DialogueManager.isConversationActive;
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
        return Mathf.Abs(_action.Player.Move.ReadValue<float>()) > 0.5f; // Move 는 1D 축(좌우)
    }

    private void UpdateResult(bool pressed)
    {
        if (Session.Result == GatherResult.Cancelled)
        {
            End(); // 이동으로 중단했으면 바로 움직일 수 있게
            return;
        }
        if (!_granted)
        {
            _granted = true;
            Grant();
        }
        if (Session.Elapsed >= ResultHold || (pressed && Session.Elapsed > 0.25f))
        {
            End();
        }
    }

    private void Grant()
    {
        if (Session.Result != GatherResult.Success)
        {
            return;
        }
        InventoryManager inv = InventoryManager.instance;
        GatherDef item = Session.Item;
        AddSource source = _kind == GatherKind.Ore ? AddSource.Mining : AddSource.Gathering;
        if (inv == null || !inv.Service.Add(item.Id, Session.Count, source))
        {
            _bagFull = true; // 지점은 쉬지 않는다(가방 정리 뒤 다시)
            PickupLog.ReportFull();
            return;
        }
        _readyAt = Time.time + GatherRules.Cooldown(_kind);
        inv.SaveInventory();
        PickupLog.Report(FishingUI.DisplayName(item.Id), Session.Count, item.Rarity.ToString());
        GameLog.Info(GameLog.Cat.Item, "채집: " + item.Id + " x" + Session.Count);
    }

    /// <summary>종료·잠금 해제(결과 표시가 끝났거나 지점이 꺼질 때)</summary>
    public void End()
    {
        if (Current == this)
        {
            Current = null;
            UiLock.Set(LockOwner, false);
        }
        Session = null;
    }

    /// <summary>쿨타임 즉시 해제(프로브용)</summary>
    public void ResetCooldown()
    {
        _readyAt = 0f;
    }

    private void SnapToGround()
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Ground"));
        filter.useTriggers = false;
        var hits = new RaycastHit2D[4];
        Vector2 from = (Vector2)transform.position + Vector2.up * SnapUp;
        int n = Physics2D.Raycast(from, Vector2.down, filter, hits, SnapUp + SnapDown);
        if (n > 0)
        {
            Vector3 p = transform.position;
            transform.position = new Vector3(p.x, hits[0].point.y, p.z);
        }
        else
        {
            Debug.LogWarning("[PB][Gather] 지면 못 찾음: " + name + " @" + transform.position);
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
            if (Session != null)
            {
                Session.Cancel();
            }
        }
    }

    private void BuildPrompt()
    {
        var go = new GameObject("GatherPrompt");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, PromptHeight, 0f);
        _prompt = go.AddComponent<TextMeshPro>();
        UiTheme theme = UiTheme.Get();
        if (theme.pixelFont != null)
        {
            _prompt.font = theme.pixelFont;
        }
        _prompt.text = PromptText(); // 꺼진 맵이면 Init 뒤 처음 켜질 때 Awake
        _prompt.fontSize = 3f;
        _prompt.alignment = TextAlignmentOptions.Center;
        _prompt.color = theme.unique;
        _prompt.rectTransform.sizeDelta = new Vector2(6f, 1f);
        _prompt.sortingOrder = 500;
        go.SetActive(false);
    }

    private static float RandomValue()
    {
        return Random.value;
    }
}
