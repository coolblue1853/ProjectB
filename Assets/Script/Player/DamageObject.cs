using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class DamageObject : MonoBehaviour
{
    public float dmgRatio = 1.0f;
    public float ShakeTime;
    public  bool isSkill = false;
    public bool isDestroyByTime = true;
    public float holdingTime = 0;
    public int[] damageArr;
    public float stiffnessTime = 0;   // 경직 시간.
    public float knockForce = 0;
    public Vector2 knockbackDir;
    public bool isNockBackChangeDir;
    public bool isPlayerAttack;
    public GameObject player;
    // Start is called before the first frame update
    public bool isLaunch;
    public float launchForce = 0;
    public Vector2 launchDir;

    public bool isDeletByGround = false;

    private Sequence sequence; // 시퀀스를 저장하기 위한 변수 추가
    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
    }
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (isDestroyByTime)
        {
            DestroyObject();
        }

        if (isLaunch)
        {
            transform.parent = null;
            if (player.transform.position.x > transform.position.x)
            {
                launchDir.x = -launchDir.x;
            }
            Rigidbody2D rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
            rigidbody2D.AddForce(launchDir * launchForce, ForceMode2D.Impulse);
        }
    }

    private void OnDestroy()
    {
        // Stop any active DOTween sequences when the object is destroyed
        if (sequence != null)
        {
            sequence.Kill(); // 시퀀스를 중지
        }
    }

    private void DestroyObject()
    {
        sequence = DOTween.Sequence()
            .AppendInterval(holdingTime)
            .AppendCallback(() => Destroy(this.gameObject));
    }

    public int maxDamagedEnemies = 1; // 최대 데미지를 입힐 적의 수
    private List<Collider2D> damagedEnemies = new List<Collider2D>(); // 데미지를 입힌 적들의 리스트

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            // 데미지를 입힌 적의 수가 최대치를 초과하지 않은 경우에만 실행
            if (damagedEnemies.Count < maxDamagedEnemies)
            {
                // 이미 데미지를 입힌 적인지 확인하고, 데미지를 입히지 않은 경우에만 실행
                if (!damagedEnemies.Contains(collision))
                {
                    EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();

                    if (isPlayerAttack == true)
                    {
                        if (player.transform.position.x > transform.position.x)
                        {
                            knockbackDir.x = -knockbackDir.x;
                        }
                    }


                    // 적에게 데미지를 입히고 데미지를 입힌 적 리스트에 추가
                    enemyHealth.damage2Enemy(damageArr, stiffnessTime, knockForce, knockbackDir, this.transform.position.x, isNockBackChangeDir, isSkill, ShakeTime, dmgRatio);
                    if (isPosionAttack)
                    {
                        enemyHealth.CreatPoisonPrefab(poisonDamage, damageInterval, damageCount);
                    }

                    // 데미지를 입힌 적 리스트에 추가
                    damagedEnemies.Add(collision);
                }
            }
        }
        else if (collision.tag == "Ground")
        {
            if (isDeletByGround)
            {
                Destroy(this.gameObject);
            }
        }
    }




    public void SetDamge(int[] damgeArray)
    {
        damageArr = damgeArray;
        //this.gameObject.SetActive(true);
    }

    public bool isPosionAttack;
    public int poisonDamage;
    public float damageInterval;
    public int damageCount;



}
