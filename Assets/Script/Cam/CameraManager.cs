using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class CameraManager : MonoBehaviour
{
    private static CameraManager instance = null;
    private void Awake()
    {
        if (null == instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    //게임 매니저 인스턴스에 접근할 수 있는 프로퍼티. static이므로 다른 클래스에서 맘껏 호출할 수 있다.
    public static CameraManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    public GameObject target; // 카메라가 따라갈 대상
    public float moveSpeed; //카메라 이동 속도
    private Vector3 targetPosition; // 대상의 현재위치
    public bool isCheckEnd = false;
    // Start is called before the first frame update
    public void CameraMove()
    {
        isCheckEnd = false;
    }
    public void CameraStop()
    {
        isCheckEnd = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "endStage")
        {
            isCheckEnd = true;
        }
    }
    public void ShakeCam()
    {
        this.transform.DOShakePosition(1, 2, 90);
    }
    // Update is called once per frame
    void Update()
    {
        if (target.gameObject != null)
        {
            if (isCheckEnd == false)
            {
                targetPosition.Set(target.transform.position.x, this.transform.position.y, -15);
                this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
    }
}
