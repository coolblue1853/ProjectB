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
    // MonsterBuilder 시체(Death 애니가 붙은 단일 몸체): 애니 뒤 조각으로 부서진다(DeathShards). 원본 파츠 시체와 연출 통일
    private SpriteRenderer singleBody;
    private float shatterDir = 1f;
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
        if (transform.childCount == 1)
        {
            Transform only = transform.GetChild(0);
            if (only.TryGetComponent(out Animator _) && only.TryGetComponent(out SpriteRenderer body))
            {
                singleBody = body;
            }
        }
    }
    private void OnEnable()
    {
        
    }



    void ResetParts()
    {
        if (singleBody != null)
        {
            singleBody.enabled = true;
        }


        // 각 파츠의 로컬 좌표를 원래대로 되돌림
        for (int i = 0; i < transform.childCount; i++)
        {
            partsObject[i].transform.localPosition = partsPosition[i]; // 로컬 좌표 복구
            partsObject[i].transform.localRotation = partsRotation[i];  // 로컬 회전 복구
            // 파츠가 물리적으로 움직였을 경우를 대비해 Rigidbody2D 초기화
            Rigidbody2D rb = partsObject[i].GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // 속도 초기화
                rb.angularVelocity = 0f;    // 회전 속도 초기화
            }
        }
        if(this.gameObject.activeSelf != false)
        {
            ReleaseObject();
        }
       
    }
    // 사망 연출 타이밍: 1.2초부터 0.6초 동안 디졸브 → 2.0초에 반환 (디졸브가 반환보다 먼저 끝나야 깜빡임이 없다)
    const float DissolveStart = 1.2f, DissolveDuration = 0.6f, ReleaseTime = 2f;
    // 단일 몸체 시체: Death 애니(약 0.5초)가 끝나고 잠깐 누운 뒤 부서진다
    const float ShatterTime = 0.75f;

    void Shatter()
    {
        if (singleBody != null && DeathShards.Shatter(singleBody, shatterDir))
        {
            singleBody.enabled = false;
            return;
        }
        StartDissolve(); // 조각을 못 만들면(타이트 아틀라스 등) 기존 디졸브
    }

    void StartDissolve()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform part = transform.GetChild(i);
            if (!part.TryGetComponent(out SpriteRenderer _))
            {
                continue;
            }
            if (!part.TryGetComponent(out SpriteHitFx fx))
            {
                fx = part.gameObject.AddComponent<SpriteHitFx>();
            }
            fx.Dissolve(DissolveDuration);
        }
    }

    public void Force2DeadBody(float knockbackForce)
    {
        CancelInvoke();
        if (singleBody != null)
        {
            singleBody.enabled = true; // 이전 사용이 반환 전에 끊겼어도 몸체를 다시 보인다
            Invoke(nameof(Shatter), ShatterTime);
        }
        else
        {
            Invoke(nameof(StartDissolve), DissolveStart);
        }
        Invoke("ResetParts", ReleaseTime);
        if (knockbackForce > maxNForce) // 최대 넉백 속도
        {
            knockbackForce = maxNForce;
        }
        if (player == null)
        {
            player = GameObject.FindWithTag("Player"); // 첫 사용 시 한 번만 찾는다
        }
        if (player.transform.position.x > this.transform.position.x)
        {
            knockbackDir = new Vector2(-1, 1);
        }
        else
        {
            knockbackDir = new Vector2(1, 1);
        }
        shatterDir = knockbackDir.x;

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
