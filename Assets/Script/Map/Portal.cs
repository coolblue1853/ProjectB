using UnityEngine;
using DG.Tweening;
public class Portal : MonoBehaviour
{
    public Transform destination; // 포탈의 목적지
    public GameObject CloseMap;
    public GameObject CloseParrlex;
    public GameObject OpenMap;
    public GameObject OpenParrlex;
    private void OnTriggerStay2D(Collider2D collision)
    {
        // 충돌한 오브젝트가 Player 태그를 가지고 있다면
        if (collision.transform.tag == "Player")
        {
            Debug.Log("닿아있음");
            if( Input.GetKey(KeyCode.UpArrow) && DatabaseManager.isUsePortal == false)
            {
                DatabaseManager.isUsePortal = true;
              Sequence seq = DOTween.Sequence()
             .AppendCallback(() => OpenMap.SetActive(true))
             .AppendCallback(() => CloseMap.SetActive(false))
             .AppendCallback(() => collision.transform.position = destination.position)
             .AppendInterval(1f)
             .OnComplete(() => OpenParrelx());
            }

           

         ;
            
            // 목적지로 이동
          
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