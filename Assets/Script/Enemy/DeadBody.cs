using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
public class DeadBody : PoolAble
{

    public float maxNForce = 10; // 최대 넉백 힘
    GameObject player;
    public GameObject parentEnemy;
    Vector2 knockbackDir;
    private Vector3[] partsPosition;
    private GameObject[] partsObject;
    private Quaternion[] partsRotation; // 파츠들의 로컬 회전 상태를 저장할 배열
    private void Awake()
    {
        partsPosition = new Vector3[transform.childCount];
        partsObject = new GameObject[transform.childCount];
        partsRotation = new Quaternion[transform.childCount]; // 로컬 회전 배열 생성

        // 파츠들의 로컬 좌표를 저장
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform T = transform.GetChild(i);
            partsObject[i] = T.gameObject;
            partsPosition[i] = T.localPosition; // 전역 좌표가 아닌 로컬 좌표로 저장
            partsRotation[i] = T.localRotation;    // 로컬 회전 저장
        }
    }
    private void OnEnable()
    {
        
    }



    void ResetParts()
    {


        // 각 파츠의 로컬 좌표를 원래대로 되돌림
        for (int i = 0; i < transform.childCount; i++)
        {
            partsObject[i].transform.localPosition = partsPosition[i]; // 로컬 좌표 복구
            partsObject[i].transform.localRotation = partsRotation[i];  // 로컬 회전 복구
            // 파츠가 물리적으로 움직였을 경우를 대비해 Rigidbody2D 초기화
            Rigidbody2D rb = partsObject[i].GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero; // 속도 초기화
                rb.angularVelocity = 0f;    // 회전 속도 초기화
            }
        }
        if(this.gameObject.activeSelf != false)
        {
            ReleaseObject();
        }
       
    }
    public void Force2DeadBody(float knockbackForce)
    {

        Invoke("ResetParts", 2f);
        if (knockbackForce > maxNForce) // 최대 넉백 속도
        {
            knockbackForce = maxNForce;
        }
        player = GameObject.FindWithTag("Player");
        if (player.transform.position.x > this.transform.position.x)
        {
            knockbackDir = new Vector2(-1, 1);
        }
        else
        {
            knockbackDir = new Vector2(1, 1);
        }

        for(int i =0; i < transform.childCount; i++)
        {
            DeadBodyEffect dBE = transform.GetChild(i).GetComponent<DeadBodyEffect>();

            if(dBE == null)
            {
                Rigidbody2D rb = transform.GetChild(i).GetComponent<Rigidbody2D>();
                float forceRange = Random.Range(knockbackForce * 0.2f, knockbackForce);
                rb.AddForce(knockbackDir.normalized * forceRange, ForceMode2D.Impulse);
            }


        }
    }

}
