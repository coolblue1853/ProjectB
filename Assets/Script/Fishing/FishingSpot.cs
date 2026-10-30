using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 물가 낚시터(MiniGames.md "층계 낚시 구현"). 플레이어가 트리거 안에 들어오면 머리 위에 "UP  FISH",
/// 위 방향키(KeyAction UI/Up — NPC 대화·퀘스트 게시판과 같은 상호작용 키)를 누르고 있다가 떼면 캐스팅한다.
/// 낚시 중에는 UiLock "Fishing" 으로 조작을 막고, 좌우 이동 입력·경직 피격(isAttacked)·사망·트리거 이탈이면 중단한다(중단은 결과 표시 없이 바로 풀림).
/// 보상은 InventoryService.Add(AddSource.Fishing) 한 곳에서만 지급한다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FishingSpot : MonoBehaviour
{
    public const string LockOwner = "Fishing";
    private const float ResultHold = 1.6f;
    private const float PromptHeight = 2.2f;

    [SerializeField] private int _floor = 1;

    private KeyAction _action;
    private TextMeshPro _prompt;
    private bool _playerNear;
    private bool _granted;
    private bool _bagFull;

    /// <summary>지금 낚시 중인 낚시터(없으면 null). FishingUI 가 읽는다</summary>
    public static FishingSpot Current { get; private set; }

    public FishingSession Session { get; private set; }

    /// <summary>결과 단계에서 가방이 가득 차 물고기를 놓아줬는가</summary>
    public bool BagFull => _bagFull;

    public int Floor
    {
        get => _floor;
        set => _floor = value;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Current = null;
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
        if (Time.timeScale <= 0f)
        {
            return; // 일시정지 메뉴의 ↑ 입력이 낚시로 새지 않게
        }
        if (Session == null)
        {
            UpdateIdle();
            return;
        }
        if (ShouldCancel())
        {
            Session.Cancel();
        }
        InputAction up = _action.UI.Up;
        Session.Tick(Time.deltaTime, up.IsPressed(), up.WasPressedThisFrame());
        if (Session.Phase == FishingPhase.Result)
        {
            UpdateResult(up.WasPressedThisFrame());
        }
    }

    private void UpdateIdle()
    {
        bool canFish = _playerNear && Current == null && !DatabaseManager.isOpenUI
            && !PixelCrushers.DialogueSystem.DialogueManager.isConversationActive; // 대화도 같은 ↑ 키
        if (_prompt.gameObject.activeSelf != canFish)
        {
            _prompt.gameObject.SetActive(canFish);
        }
        if (canFish && _action.UI.Up.WasPressedThisFrame())
        {
            Begin();
        }
    }

    /// <summary>낚시 시작(프로브용 공개 창구). 이미 다른 낚시 중이면 false</summary>
    public bool Begin()
    {
        if (Current != null)
        {
            return false;
        }
        Session = new FishingSession(FishingRules.Rod(FishingSave.LoadRod()), FishingRules.TableFor(_floor), RandomValue);
        Current = this;
        _granted = false;
        _bagFull = false;
        _prompt.gameObject.SetActive(false);
        UiLock.Set(LockOwner, true);
        return true;
    }

    private bool ShouldCancel()
    {
        if (Session.Phase == FishingPhase.Result)
        {
            return false;
        }
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
        if (Session.Result == FishingResult.Cancelled)
        {
            End(); // 이동으로 중단했으면 바로 움직일 수 있게
            return;
        }
        if (!_granted)
        {
            _granted = true;
            Grant();
        }
        if (Session.PhaseTime >= ResultHold || (pressed && Session.PhaseTime > 0.25f))
        {
            End();
        }
    }

    private void Grant()
    {
        if (Session.Result != FishingResult.Caught)
        {
            return;
        }
        InventoryManager inv = InventoryManager.instance;
        FishDef fish = Session.Fish;
        if (inv == null || !inv.Service.Add(fish.Id, Session.Count, AddSource.Fishing))
        {
            _bagFull = true;
            PickupLog.ReportFull();
            return;
        }
        inv.SaveInventory();
        PickupLog.Report(FishingUI.DisplayName(fish.Id), Session.Count, fish.Rarity.ToString());
        GameLog.Info(GameLog.Cat.Item, "낚시: " + fish.Id + " x" + Session.Count);
    }

    /// <summary>낚시 종료·잠금 해제(결과 표시가 끝났거나 낚시터가 꺼질 때)</summary>
    public void End()
    {
        if (Current == this)
        {
            Current = null;
            UiLock.Set(LockOwner, false);
        }
        Session = null;
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
        var go = new GameObject("FishPrompt");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, PromptHeight, 0f);
        _prompt = go.AddComponent<TextMeshPro>();
        UiTheme theme = UiTheme.Get();
        if (theme.pixelFont != null)
        {
            _prompt.font = theme.pixelFont;
        }
        _prompt.text = Loc.T("UP  FISH");
        _prompt.fontSize = 3f;
        _prompt.alignment = TextAlignmentOptions.Center;
        _prompt.color = theme.unique;
        _prompt.rectTransform.sizeDelta = new Vector2(6f, 1f);
        _prompt.sortingOrder = 500;
        go.SetActive(false);
    }

    private static float RandomValue()
    {
        return UnityEngine.Random.value;
    }
}
