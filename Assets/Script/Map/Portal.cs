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
    }
    private void Update()
    {
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

    void AbleOpenMap()
    {
        openMap.SetActive(true);
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