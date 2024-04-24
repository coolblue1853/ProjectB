using UnityEngine;
using DG.Tweening;
using Com.LuisPedroFonseca.ProCamera2D;
public class Portal : MonoBehaviour
{
    public Transform destination; // ��Ż�� ������
    public GameObject CloseMap;
    public GameObject CloseParrlex;
    public GameObject OpenMap;
    public GameObject OpenParrlex;
    public ProCamera2D proCamera;
    private void Start()
    {
        
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        // �浹�� ������Ʈ�� Player �±׸� ������ �ִٸ�
        if (collision.transform.tag == "Player")
        {
            Debug.Log("�������");
            if( Input.GetKey(KeyCode.UpArrow) && DatabaseManager.isUsePortal == false)
            {
                DatabaseManager.isUsePortal = true;
              Sequence seq = DOTween.Sequence()
             .AppendCallback(() => OpenMap.SetActive(true))
             .AppendCallback(() => CloseMap.SetActive(false))
             .AppendCallback(() => collision.transform.position = destination.position)
             .AppendCallback(() => proCamera.CenterOnTargets())
             .AppendInterval(1f)
             .OnComplete(() => OpenParrelx());
            }

           

         ;
            
            // �������� �̵�
          
        }
    }


    void OpenParrelx()
    {
        DatabaseManager.isUsePortal = false;
        OpenParrlex.SetActive(true);
        CloseParrlex.SetActive(false);
    }
    void OnTriggerEnter(Collider other)
    {

    }
}