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
    public GameObject player;
    private void OnEnable()
    {
        upAction.Enable();
    }
    private void OnDisable()
    {
        upAction.Disable();
    }
    private void Awake()
    {
        action = new KeyAction();
        upAction = action.UI.UPInventory;
        // 열/닫을 맵이 없으면 같은 맵 안에서 위치만 옮기는 포털 (B-111)
        isMapChange = openMap != null;
        openMapCheck = openMap != null ? openMap.GetComponent<PoolCheck>() : null;
        closeMapCheck = closeMap != null ? closeMap.GetComponent<PoolCheck>() : null;
        seal = GetComponent<PortalSeal>();
    }
    private void Update()
    {
        if (seal != null && upAction.triggered && player != null && DatabaseManager.isUsePortal == false && !seal.IsOpen)
        {
            seal.NotifySealed();
            return;
        }
        if (upAction.triggered == true && DatabaseManager.isUsePortal == false && isMapChange == true && player != null && destination != null)
        {
            player.transform.position = destination.position;
            Debug.Log("포탈작동");
            DatabaseManager.isUsePortal = true;
            Sequence seq = DOTween.Sequence()
           .AppendCallback(() => AbleOpenMap())
           .AppendCallback(() => AbleCloseMap())
       //    .AppendCallback(() => player.transform.position = destination.position)
           .AppendCallback(() => proCamera.CenterOnTargets())
           .AppendInterval(1f)
           .OnComplete(() => OpenParrelx());
        }
        else if (upAction.triggered == true && DatabaseManager.isUsePortal == false && isMapChange == false && player != null && destination != null)
        {
            player.transform.position = destination.position;
            DatabaseManager.isUsePortal = true;
            Sequence seq = DOTween.Sequence()
          // .AppendCallback(() => player .transform.position = destination.position)
           .AppendCallback(() => proCamera.CenterOnTargets())
           .AppendInterval(1f)
           .OnComplete(() => EndOnlyMove());
        }



    }


    /// <summary>맵이 바뀌었을 때(새로 연 맵) — BGM 등에서 구독</summary>
    public static event System.Action<GameObject> MapChanged;
    public static void RaiseMapChanged(GameObject openMap) => MapChanged?.Invoke(openMap);

    /// <summary>입력 없이 이 포털로 이동 (사망 후 마을 귀환 등). 맵 전환 포털이면 맵을 바꾸고 스포너를 정리한다. PortalSeal 봉인은 확인하지 않는다(강제 이동용).</summary>
    public void TravelNow(GameObject traveler)
    {
        if (destination == null || traveler == null)
        {
            return;
        }
        traveler.transform.position = destination.position;
        if (isMapChange)
        {
            AbleOpenMap();
            AbleCloseMap();
        }
        if (proCamera != null)
        {
            proCamera.CenterOnTargets();
        }
        DatabaseManager.isUsePortal = false;
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

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        // 충돌한 오브젝트가 Player 태그를 가지고 있다면
        if (collision.transform.tag == "Player")
        {
            player = null;

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