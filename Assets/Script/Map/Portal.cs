using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using Com.LuisPedroFonseca.ProCamera2D;
public class Portal : MonoBehaviour
{
    KeyAction action;
    InputAction upAction;
    public Transform destination; // ��Ż�� ������
    public GameObject CloseMap;
    public GameObject CloseParrlex;
    public GameObject OpenMap;
    public GameObject OpenParrlex;
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
    }
    private void Update()
    {
        if (upAction.triggered == true && DatabaseManager.isUsePortal == false && isMapChange == true && player != null)
        {
            player.transform.position = destination.position;
            Debug.Log("��Ż�۵�");
            DatabaseManager.isUsePortal = true;
            Sequence seq = DOTween.Sequence()
           .AppendCallback(() => OpenMap.SetActive(true))
           .AppendCallback(() => CloseMap.SetActive(false))
       //    .AppendCallback(() => player.transform.position = destination.position)
           .AppendCallback(() => proCamera.CenterOnTargets())
           .AppendInterval(1f)
           .OnComplete(() => OpenParrelx());
        }
        else if (upAction.triggered == true && DatabaseManager.isUsePortal == false && isMapChange == false && player != null)
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
    private void OnTriggerStay2D(Collider2D collision)
    {

        // �浹�� ������Ʈ�� Player �±׸� ������ �ִٸ�
        if (collision.transform.tag == "Player")
        {




         ;
            
            // �������� �̵�
          
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �浹�� ������Ʈ�� Player �±׸� ������ �ִٸ�
        if (collision.transform.tag == "Player")
        {
            player = collision.gameObject;

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        // �浹�� ������Ʈ�� Player �±׸� ������ �ִٸ�
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
        OpenParrlex.SetActive(true);
        CloseParrlex.SetActive(false);
    }

}