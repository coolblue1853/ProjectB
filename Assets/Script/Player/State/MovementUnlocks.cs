using UnityEngine;

/// <summary>이동 개방의 단일 소유자. 새 ES3 키만 저장하고 기존 활공 키는 읽기만 한다.</summary>
public sealed class MovementUnlocks : MonoBehaviour
{
    public const string FloorKey = "movement.highestFloor";
    public const string BroochKey = "movement.broochStage";
    public static MovementUnlocks Instance { get; private set; }
    public int HighestFloor { get; private set; } = 1;
    public int BroochStage { get; private set; }
#if UNITY_EDITOR
    public bool DebugAll { get; set; }
#else
    public bool DebugAll => false;
#endif
    public bool Glide => DebugAll || MovementUnlockRules.Glide(HighestFloor);
    public bool AirTurn => DebugAll || MovementUnlockRules.AirTurn(HighestFloor);
    public int JumpLimit(int configured) => MovementUnlockRules.JumpLimit(configured, HighestFloor, DebugAll);
    public bool CanDash(bool grounded) => MovementUnlockRules.CanDash(HighestFloor, BroochStage, grounded, DebugAll);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        Reload();
    }

    private void OnEnable()
    {
        Portal.MapChanged += OnMapChanged;
        NpcQuestLog.Changed += OnQuestChanged;
    }

    private void OnDisable()
    {
        Portal.MapChanged -= OnMapChanged;
        NpcQuestLog.Changed -= OnQuestChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this) { Instance = null; }
    }

    private void Start()
    {
        // 시작 맵은 포털 이벤트가 없으므로 가장 가까운 활성 맵으로 확인한다.
        GameObject root = GameObject.Find("Map");
        MapInfo closest = null;
        float distance = float.MaxValue;
        float playerY = SaveManager.instance != null ? SaveManager.instance.datas.playerPos.y : transform.position.y;
        if (root != null)
        {
            foreach (Transform map in root.transform)
            {
                MapInfo info = map.GetComponent<MapInfo>();
                float dy = Mathf.Abs(map.position.y - playerY);
                if (info != null && map.gameObject.activeInHierarchy && dy < distance)
                {
                    closest = info;
                    distance = dy;
                }
            }
        }
        if (closest != null) { ReachFloor(closest.floor, false); }
    }

    public void Reload()
    {
        HighestFloor = Mathf.Clamp(ES3.Load<int>(FloorKey, 1), 1, 7);
        BroochStage = Mathf.Clamp(ES3.Load<int>(BroochKey, 0), 0, 1);
        // 구 세이브 활공 개방 보존. 기존 키는 변경하지 않는다.
        if (ES3.Load<bool>(GlideMotor.UnlockKey, false)) { HighestFloor = Mathf.Max(2, HighestFloor); }
    }

    private void OnMapChanged(GameObject map)
    {
        MapInfo info = map != null ? map.GetComponent<MapInfo>() : null;
        if (info != null) { ReachFloor(info.floor); }
    }

    private void OnQuestChanged()
    {
        if (NpcQuestLog.IsDone("Q604")) { ReleaseBrooch(1); }
    }

    public bool ReachFloor(int floor, bool notify = true)
    {
        floor = Mathf.Clamp(floor, 1, 7);
        if (floor <= HighestFloor) { return false; }
        int before = HighestFloor;
        HighestFloor = floor;
        ES3.Save(FloorKey, HighestFloor);
        if (Application.isPlaying && notify && before < 3 && floor >= 2)
        {
            string line = before < 2 ? "Movement unlocked: Double Jump / Air Turn / Glide" : "Movement unlocked: Air Dash";
            if (before < 2 && floor >= 3) { line += " / Air Dash"; }
            BossBanner.Show("", line, true);
        }
        return true;
    }

    public bool ReleaseBrooch(int stage)
    {
        stage = Mathf.Clamp(stage, 0, 1);
        if (stage <= BroochStage) { return false; }
        bool wasOpen = MovementUnlockRules.GroundDash(HighestFloor, BroochStage);
        BroochStage = stage;
        ES3.Save(BroochKey, BroochStage);
        if (Application.isPlaying && !wasOpen) { BossBanner.Show("", "Movement unlocked: Ground Dash", true); }
        return true;
    }
}
