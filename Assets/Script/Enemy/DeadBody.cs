using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
public class DeadBody : PoolAble
{

    public float maxNForce = 10; // �ִ� �˹� ��
    GameObject player;
    public GameObject parentEnemy;
    Vector2 knockbackDir;
    private Vector3[] partsPosition;
    private GameObject[] partsObject;
    private Quaternion[] partsRotation; // �������� ���� ȸ�� ���¸� ������ �迭
    private void Awake()
    {
        partsPosition = new Vector3[transform.childCount];
        partsObject = new GameObject[transform.childCount];
        partsRotation = new Quaternion[transform.childCount]; // ���� ȸ�� �迭 ����

        // �������� ���� ��ǥ�� ����
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform T = transform.GetChild(i);
            partsObject[i] = T.gameObject;
            partsPosition[i] = T.localPosition; // ���� ��ǥ�� �ƴ� ���� ��ǥ�� ����
            partsRotation[i] = T.localRotation;    // ���� ȸ�� ����
        }
    }
    private void OnEnable()
    {
        
    }



    void ResetParts()
    {


        // �� ������ ���� ��ǥ�� ������� �ǵ���
        for (int i = 0; i < transform.childCount; i++)
        {
            partsObject[i].transform.localPosition = partsPosition[i]; // ���� ��ǥ ����
            partsObject[i].transform.localRotation = partsRotation[i];  // ���� ȸ�� ����
            // ������ ���������� �������� ��츦 ����� Rigidbody2D �ʱ�ȭ
            Rigidbody2D rb = partsObject[i].GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero; // �ӵ� �ʱ�ȭ
                rb.angularVelocity = 0f;    // ȸ�� �ӵ� �ʱ�ȭ
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
        if (knockbackForce > maxNForce) // �ִ� �˹� �ӵ�
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
