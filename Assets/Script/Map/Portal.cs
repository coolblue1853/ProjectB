using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using Com.LuisPedroFonseca.ProCamera2D;
public class Portal : MonoBehaviour
{
    KeyAction action;
    InputAction upAction;
    public Transform destination; // 포탈의 목적지
    public GameObject closeMap;
    private PoolCheck closeMapCheck;
    public GameObject openMap;
    private PoolCheck openMapCheck;
    private PortalSeal seal; // 있으면 보스 첫 처치 전엔 이동을 막는다(Q-414)
    public ProCamera2D proCamera;
    bool isMapChange = true;
    private Collider2D _playerCol, _col;
    /// <summary>도착 직후 무적(초). 암전이 밝아지는 동안·도착 지점의 몹에게 바로 맞지 않게</summary>
    public const float ArrivalInvincible = 1.0f;
    /// <summary>라벨에 쓸 이름(비우면 목적지 구역 이름). 같은 맵 안 이동 포털용</summary>
    public string labelOverride;
    public GameObject player;
    private void OnEnable()
    {
        upAction.Enable();
    }
    private void OnDisable()
    {
        upAction.Disable();
        // 맵이 꺼지면 OnTriggerExit2D 가 오지 않아 player 가 남는다 → 다시 켜졌을 때 어디서든 ↑ 한 번에 이동하는 문제(11-02)
        player = null;
        _playerCol = null;
    }
    private void Awake()
    {
        AlignForestEntrance();
        action = new KeyAction();
        upAction = action.UI.UPInventory;
        // 열/닫을 맵이 없으면 같은 맵 안에서 위치만 옮기는 포털 (B-111)
        isMapChange = openMap != null;
        openMapCheck = openMap != null ? openMap.GetComponent<PoolCheck>() : null;
        closeMapCheck = closeMap != null ? closeMap.GetComponent<PoolCheck>() : null;
        seal = GetComponent<PortalSeal>();
        _col = GetComponentInChildren<Collider2D>();
        PortalLabel.Attach(this, seal);
    }

    private void AlignForestEntrance()
    {
        if (closeMap == null || openMap == null)
        {
            return;
        }
        bool toForest = closeMap.name == "Lv1_NearForest_SideRoad" && openMap.name == "Lv1_NormalForest";
        bool toSideRoad = closeMap.name == "Lv1_NormalForest" && openMap.name == "Lv1_NearForest_SideRoad";
        if (toForest || toSideRoad)
        {
            // 이 연결의 지면은 y=4.40. 옛 씬 좌표(-0.41)는 트리거까지 지면 밑에 묻힌다.
            Vector3 position = transform.position;
            position.y = 5.52f;
            transform.position = position;
            // 목적지 맵이 비활성이면 그쪽 Awake보다 먼저 좌표를 읽고 이동한다.
            if (destination != null)
            {
                Vector3 arrival = destination.position;
                arrival.y = position.y;
                destination.position = arrival;
            }
        }
    }
    private void Update()
    {
        if (player != null && _col != null && _playerCol != null && !_col.bounds.Intersects(_playerCol.bounds))
        {
            player = null; // 텔레포트·맵 교체로 Exit 이벤트 없이 벗어난 경우
            _playerCol = null;
        }
        if (seal != null && upAction.triggered && player != null && DatabaseManager.isUsePortal == false && !seal.IsOpen)
        {
            seal.NotifySealed();
            return;
        }
        if (upAction.triggered == true && player != null)
        {
            Use(player);
        }
    }

    /// <summary>
    /// 플레이어가 ↑로 포털을 탄다. 11-01: 화면이 어두워진 사이에 이동·맵 교체·카메라 맞춤 → 밝아지며 도착(카메라가 날아가는 모습을 가린다).
    /// 이미 이동 중이거나 목적지가 없으면 false. 봉인은 Update 에서 먼저 거른다.
    /// </summary>
    public bool Use(GameObject traveler)
    {
        if (DatabaseManager.isUsePortal || traveler == null || destination == null)
        {
            return false;
        }
        DatabaseManager.isUsePortal = true;
        MapTransit.Run(traveler, () =>
        {
            traveler.transform.position = destination.position;
            GrantArrivalInvincible();
            MoveMaps();
        }, isMapChange ? (System.Action)OpenParrelx : EndOnlyMove, proCamera);
        return true;
    }


    /// <summary>맵이 바뀌었을 때(새로 연 맵) — BGM 등에서 구독</summary>
    public static event System.Action<GameObject> MapChanged;
    public static void RaiseMapChanged(GameObject openMap) => MapChanged?.Invoke(openMap);

    /// <summary>입력 없이 이 포털로 이동 (사망 후 마을 귀환 등). 맵 전환 포털이면 맵을 바꾸고 스포너를 정리한다. PortalSeal 봉인은 확인하지 않는다(강제 이동용).</summary>
    public void TravelNow(GameObject traveler, System.Action arrived = null)
    {
        if (destination == null || traveler == null)
        {
            return;
        }
        DatabaseManager.isUsePortal = true;
        MapTransit.Run(traveler, () =>
        {
            traveler.transform.position = destination.position;
            MoveMaps();
            arrived?.Invoke();
        }, EndOnlyMove, proCamera);
    }

    /// <summary>맵 전환 포털이면 목적지 맵을 켜고 출발 맵을 끈다. 같은 맵 안 이동이면 카메라 경계만 다시 맞춘다</summary>
    private void MoveMaps()
    {
        if (isMapChange)
        {
            AbleOpenMap();
            AbleCloseMap();
        }
        else
        {
            ApplyLocalCameraBounds();
        }
    }

    private static void GrantArrivalInvincible()
    {
        PlayerStatus.AcquireFor(StatusFlag.Invincible, ArrivalInvincible, "PortalArrival");
    }

    private void ApplyLocalCameraBounds()
    {
        MapInfo map = GetComponentInParent<MapInfo>();
        if (map != null)
        {
            MapInfo.Apply(map);
        }
    }

    void AbleOpenMap()
    {
        openMap.SetActive(true);
        Sfx.Play(SfxId.Portal, 0.6f);
        GameLog.Info(GameLog.Cat.Map, "포털 " + name + ": " + (closeMap ? closeMap.name : "-") + " → " + openMap.name, this);
        MapChanged?.Invoke(openMap);
        if (openMapCheck != null)
            openMapCheck.AbleSpwaner();
    }
    void AbleCloseMap()
    {
        if (closeMap == null)
        {
            return;
        }
        closeMap.SetActive(false);
        if(closeMapCheck != null)
        {
            closeMapCheck.DisableSpwaner();
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 오브젝트가 Player 태그를 가지고 있다면
        if (collision.transform.tag == "Player")
        {
            player = collision.gameObject;
            _playerCol = collision;

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        // 충돌한 오브젝트가 Player 태그를 가지고 있다면
        if (collision.transform.tag == "Player")
        {
            player = null;
            _playerCol = null;
        }
    }

    void EndOnlyMove()
    {
        DatabaseManager.isUsePortal = false;

    }
    void OpenParrelx()
    {
        SaveManager.instance.SavePos();
        DatabaseManager.isUsePortal = false;
    }

}
