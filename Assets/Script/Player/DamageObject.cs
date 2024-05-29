using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DamageObject : MonoBehaviour
{

    Transform playerTf;
    public float dmgRatio = 1.0f;
    public float ShakeTime;
    public  bool isSkill = false;
    public bool isDestroyByTime = true;
    [ConditionalHide("isDestroyByTime")]
    public float holdingTime = 0;
    public int hitCount = 1 ; //다단히트수.
    public int[] damageArr;
    public float stiffnessTime = 0;   // 경직 시간.
    public float knockForce = 0;
    public Vector2 knockbackDir;
    public bool isNockBackChangeDir;
    public bool isPlayerAttack;
    public GameObject player;
    // Start is called before the first frame update
    public bool isLaunch;
    [ConditionalHide("isLaunch")]
    public float launchForce = 0;
    [ConditionalHide("isLaunch")]
    public Vector2 launchDir;

    public bool isDeletByGround = false;

    public bool isTrackingEnemy = false;
    [ConditionalHide("isTrackingEnemy")]
    public float desiredSpeed;
    private Sequence sequence; // 시퀀스를 저장하기 위한 변수 추가
    Rigidbody2D rigidbody2D;
    [ConditionalHide("isTrackingEnemy")]
    public Vector2 boxSize; // 확인할 직사각형 영역의 크기
    [ConditionalHide("isTrackingEnemy")]
    public float trackingPivot = 0f;

    // 여기부터는 스킬 사용시 버프/디버프 관련 스크립트.
    public bool isHoldingBuffObject = false;
    [ConditionalHide("isHoldingBuffObject")]
    public string holdingBuffEffect;
    [ConditionalHide("isHoldingBuffObject")]
    public int holdingBuffPower;

    public bool addSuperArmor =false;

    public bool isDashAttack = false;
    [ConditionalHide("isDashAttack")]
    public float dashSpeed = 10;
    [ConditionalHide("isDashAttack")]
    public bool isDashInvins = false;
    [ConditionalHide("isDashAttack")]
    public bool isBackDash = false;
    BoxCollider2D boxCol;
    [ConditionalHide("isHoldingBuffObject")]
    public GameObject buffObject;
    private void Update()
    {

        if (damagedEnemies.Count == maxDamagedEnemies && isLaunch == true)
        {
            Destroy(this.gameObject);
        }
        if (isLaunch && isTrackingEnemy)
        {
            float currentSpeed = rigidbody2D.velocity.magnitude;
            if (currentSpeed != desiredSpeed)
            {
                Vector2 newVelocity = rigidbody2D.velocity.normalized * desiredSpeed;
                rigidbody2D.velocity = newVelocity;
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (player != null)
        {
            // 현재 오브젝트의 위치에서 boxSize를 기준으로 직사각형 그리기
            if (player.transform.localScale.x > 0)
            {
                Gizmos.DrawWireCube(new Vector2(transform.position.x + (trackingPivot), transform.position.y), new Vector3(boxSize.x, boxSize.y, 1));
            }
            else
            {
                Gizmos.DrawWireCube(new Vector2(transform.position.x - (trackingPivot), transform.position.y), new Vector3(boxSize.x, boxSize.y, 1));
            }
        }


    }
    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        playerTf = player.GetComponent<Transform>();
    }

    void BuffOn()
    {
        if (addSuperArmor == true)
        {
            DatabaseManager.isSuperArmor = true;
        }
        if (holdingBuffEffect == "Def")
        {
            DatabaseManager.playerDef += holdingBuffPower;

        }
    }
    void BuffOff()
    {
        if (DatabaseManager.weaponStopMove == true)
        {
            DatabaseManager.weaponStopMove = false;
        }
        if (addSuperArmor == true)
        {
            DatabaseManager.isSuperArmor = false;
        }
        if (holdingBuffEffect == "Def")
        {
            
            DatabaseManager.playerDef -= holdingBuffPower;

        }
    }


    void Start()
    {
        if(buffObject != null)
        {
            AttackManager att = player.GetComponent<AttackManager>();
            GameObject buff = Instantiate(buffObject, Vector2.zero, Quaternion.identity, att.BuffSlot.transform);
        }
        boxCol = this.GetComponent<BoxCollider2D>();
        if (hitCount != 1)
        {
            ResetDamagedEnemies();
        }
        if (isDashAttack)
        {
            PlayerController.instance.SkillDash(holdingTime, dashSpeed, isDashInvins, isBackDash);
        }
        if (isHoldingBuffObject)
        {
            BuffOn();
        }
        rigidbody2D = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");
        if (isDestroyByTime && isHoldingBuffObject == false)
        {
            DestroyObject();
        }

        if (isTrackingEnemy)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
            transform.parent = null;

            GameObject enemy = CheckEnemy();
            Vector3 direction;
            if (enemy != null)
            {
                 direction = (enemy.transform.position - this.transform.position).normalized;

            }
            else
            {
                if (player.transform.position.x > transform.position.x)
                {
                    launchDir.x = -launchDir.x;
                }
                direction = launchDir;
            }
           
            Rigidbody2D rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
            rigidbody2D.AddForce(direction * launchForce, ForceMode2D.Impulse);

            // 오브젝트가 날아가는 방향을 기준으로 회전하기 위해 각도를 계산합니다.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // Z축을 기준으로 회전하기 때문에, Z축을 기준으로 회전되도록 쿼터니언(quaternion)을 생성합니다.
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            // 회전을 적용합니다.
            transform.rotation = rotation;


        }
        else if (isLaunch)
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

    public void DestroyObject()
    {
        if (isHoldingBuffObject)
        {
            BuffOff();
        }
        sequence = DOTween.Sequence()
            .AppendInterval(holdingTime)
            .AppendCallback(() => Destroy(this.gameObject));
    }

    public int maxDamagedEnemies = 1; // 최대 데미지를 입힐 적의 수
    private List<Collider2D> damagedEnemies = new List<Collider2D>(); // 데미지를 입힌 적들의 리스트

    void ResetDamagedEnemies()
    {
        if(this.gameObject != null)
        {

            float resetTime = holdingTime / hitCount + 0.02f;

            Sequence seq = DOTween.Sequence()
           .AppendInterval(resetTime)
            .AppendCallback(() => damagedEnemies.Clear())
            .AppendCallback(() => boxCol.enabled =false)
            .AppendInterval(0.01f)
            .AppendCallback(() => boxCol.enabled = true)
           .AppendCallback(() => ResetDamagedEnemies());
        }


    }

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


    public LayerMask layerMask; // 검출할 레이어
    public string enemyTag = "Enemy"; // 플레이어 태그

    public GameObject CheckEnemy()
    {

        // 주변에 있는 모든 Collider를 가져옴
        Collider2D[] colliders;

        if (player.transform.localScale.x > 0)
        {
             colliders = Physics2D.OverlapBoxAll(new Vector2(transform.position.x + (trackingPivot), transform.position.y), boxSize, 0f, layerMask);
        }
        else
        {
              colliders = Physics2D.OverlapBoxAll(new Vector2(transform.position.x - (trackingPivot), transform.position.y), boxSize, 0f, layerMask);
        }
        // 가져온 Collider를 순회하면서 Player 태그를 가진 오브젝트가 있는지 확인
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(enemyTag))
            {
                // Player 태그를 가진 오브젝트가 있으면 Success 반환
                return  col.gameObject;
            }
        }

        // 주변에 Player 태그를 가진 오브젝트가 없으면 Failure 반환
        return null;
    }



}
