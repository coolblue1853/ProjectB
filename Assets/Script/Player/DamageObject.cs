using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DamageObject : MonoBehaviour
{
    public string skillName;
    Transform playerTf;
    public float dmgRatio = 1.0f;
    public float ShakeTime;
    public  bool isSkill = false;
    public bool isDestroyByTime = true;
    [ConditionalHide("isDestroyByTime")]
    public float holdingTime = 0;
    public int hitCount = 1 ; //�ٴ���Ʈ��.
    public float waitInterval = 0; // ���� �������� ���� �ٴ���Ʈ������ ���ð�
    public bool isDeletByMaxHit = true;//�ִ� ��Ʈ�ο��� ü��� �ڵ��ı�
    public bool isHitIntervelEven = true;
    public float hitIntervalTime = 0; //�ٴ���Ʈ��.
    public int[] damageArr;
    public float stiffnessTime = 0;   // ���� �ð�.
    public float knockForce = 0;
    public Vector2 knockbackDir;
    public bool isNockBackChangeDir;
    public bool nonChangeNockDir = false;
    public bool isPlayerAttack;
    public GameObject player;
    // Start is called before the first frame update
    public bool isLaunch;
    [ConditionalHide("isLaunch")]
    public float launchForce = 0;
    [ConditionalHide("isLaunch")]
    public Vector2 launchDir;
    [ConditionalHide("isLaunch")]
    public bool isRoundLunch;

    public bool isDeletByGround = false;

    public bool isTrackingEnemy = false;
    [ConditionalHide("isTrackingEnemy")]
    public float desiredSpeed;
    private Sequence sequence; // �������� �����ϱ� ���� ���� �߰�
    Rigidbody2D rigidbody2D;
    [ConditionalHide("isTrackingEnemy")]
    public Vector2 boxSize; // Ȯ���� ���簢�� ������ ũ��
    [ConditionalHide("isTrackingEnemy")]
    public float trackingPivot = 0f;

    // ������ʹ� ��ų ���� ����/����� ���� ��ũ��Ʈ.
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
    public float dashYSpeed = 0;
    [ConditionalHide("isDashAttack")]
    public bool isDashInvins = false;
    [ConditionalHide("isDashAttack")]
    public bool isBackDash = false;
    [ConditionalHide("isDashAttack")]
    public bool isEffectGrvity = false;
    [ConditionalHide("isDashAttack")]
    public bool isXbaseDash = true; //  �̰Ϳ� ���� �����ɰ��� y������ x������ ������
    BoxCollider2D boxCol;
    public GameObject buffObject;

    public bool isHealSkill = false;
    [ConditionalHide("isHealSkill")]
    public float waitNextHeal = 0;
    [ConditionalHide("isHealSkill")]
    public int healPower = 0;
    [ConditionalHide("isHealSkill")]
    public int healCount = 0;

    public bool isAbsorberSkill = false; // Ư�� ��ġ�� ���� ������ ���
    [ConditionalHide("isAbsorberSkill")]
    public GameObject absorbPivot;// ���Ƶ鿩���� ��ġ
    [ConditionalHide("isAbsorberSkill")]
    public float absorbPower = 0;
    [ConditionalHide("isAbsorberSkill")]
    public Vector2 absorbDir;

    public bool isShortTime = false; // ����� �ð����� ������Ʈ ����Ʈ�� ����� ������ �ƴ���./
    public bool disEffectbyTime = false; // �ƿ� ������ ���� ����������
    
    private void Update()
    {

        if (isDeletByMaxHit == true && damagedEnemy.Count == maxDamagedEnemies && isLaunch == true)
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
            // ���� ������Ʈ�� ��ġ���� boxSize�� �������� ���簢�� �׸���
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

        foreach(var value in DatabaseManager.bleedingEquipment)
        {
            if(Random.value <= (float)value.Value[0] / 100)
            {
                isBleedingAttack = true;
                bleedingDamage = value.Value[1];
                bleedingDamageCount = value.Value[2];
                bleedingDamageInterval = DatabaseManager.bleedingEquipmentInterval[value.Key];
            }

        }
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

        if (isHealSkill == true)
        {
            holdingTime = healCount * waitNextHeal;
            ResetHealGround();
        }
        if (buffObject != null)
        {
            AttackManager att = player.GetComponent<AttackManager>();
            GameObject buff = Instantiate(buffObject, Vector2.zero, Quaternion.identity, att.BuffSlot.transform);
        }
        boxCol = this.GetComponent<BoxCollider2D>();
        if ((hitCount+DatabaseManager.hitCount) != 1)
        {
           Sequence waitSeq = DOTween.Sequence()
       .AppendInterval(waitInterval)
       .AppendCallback(() => ResetDamagedEnemies());

        }

        if (isDashAttack)
        {
            if (isXbaseDash)
            {
                PlayerController.instance.SkillDash(holdingTime / (1 + (DatabaseManager.attackSpeedBuff / 100)), dashSpeed * (1 + (DatabaseManager.attackSpeedBuff / 100)), dashYSpeed / (1 + (DatabaseManager.attackSpeedBuff / 100)), isDashInvins, isBackDash, isEffectGrvity);
            }
            else
            {
                PlayerController.instance.SkillDash(holdingTime / (1 + (DatabaseManager.attackSpeedBuff / 100)), dashSpeed / (1 + (DatabaseManager.attackSpeedBuff / 100)), dashYSpeed * (1 + (DatabaseManager.attackSpeedBuff / 100)), isDashInvins, isBackDash, isEffectGrvity);
            }
         
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

            // ������Ʈ�� ���ư��� ������ �������� ȸ���ϱ� ���� ������ ����մϴ�.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // Z���� �������� ȸ���ϱ� ������, Z���� �������� ȸ���ǵ��� ���ʹϾ�(quaternion)�� �����մϴ�.
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            // ȸ���� �����մϴ�.
            transform.rotation = rotation;


        }
        else if (isLaunch)
        {
            transform.parent = null;

            if(isRoundLunch == false)
            {
                if (player.transform.position.x > transform.position.x)
                {
                    launchDir.x = -launchDir.x;
                }
                Rigidbody2D rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
                rigidbody2D.AddForce(launchDir * launchForce, ForceMode2D.Impulse);
            }
            else
            {
                Vector2 launchDir = transform.up;
                Rigidbody2D rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
                rigidbody2D.AddForce(launchDir * launchForce, ForceMode2D.Impulse);
            }

        }

    }

    private void OnDestroy()
    {
        // Stop any active DOTween sequences when the object is destroyed
        if (sequence != null)
        {
            sequence.Kill(); // �������� ����
        }
    }

    public void DestroyObject()
    {
        if (isHoldingBuffObject)
        {
            BuffOff();
        }

        float resetTime = 0;

        // 



         if (isHitIntervelEven == true)
        {
            resetTime = (holdingTime) / (1 + (DatabaseManager.attackSpeedBuff / 100));
            Sequence sequence = DOTween.Sequence()
    .AppendInterval(resetTime)
    .AppendCallback(() => isEndBoxCollider = true)
    .AppendCallback(() => boxCol.enabled = false);

        }
        else
        {
         //   resetTime = holdingTime / (1 + (DatabaseManager.attackSpeedBuff / 100));
        }


        float destroyTime = 0;
        if (isShortTime)
        {
            destroyTime = resetTime;
        }
        else
        {
            destroyTime = holdingTime;
        }

        sequence = DOTween.Sequence()
            .AppendInterval(destroyTime)
            .AppendCallback(() => Destroy(this.gameObject));
    }

    public int maxDamagedEnemies = 1; // �ִ� �������� ���� ���� ��
   // private List<Collider2D> damagedEnemies = new List<Collider2D>(); // �������� ���� ������ ����Ʈ
    private Dictionary<Collider2D, int> damagedEnemy = new Dictionary<Collider2D, int>();
    void ResetDamagedEnemies()
    {
        if(this.gameObject != null)
        {
            float resetTime;

            // 
            if (isHitIntervelEven == true)
            {
                    resetTime =( holdingTime / (hitCount + DatabaseManager.hitCount) + 0.01f) / (1 + (DatabaseManager.attackSpeedBuff / 100));
            }
            else
            {
                 resetTime = hitIntervalTime / (1 + (DatabaseManager.attackSpeedBuff / 100));
            }

            Sequence seq = DOTween.Sequence()
           .AppendInterval(resetTime)
                       // .AppendCallback(() => damagedEnemies.Clear())
             .AppendCallback(() => ResetDict())
            .AppendCallback(() => CollOnOff(false))
            .AppendInterval(0.01f)
            .AppendCallback(() => CollOnOff(true))
           .AppendCallback(() => ResetDamagedEnemies());
        } 
    }
    bool isEndBoxCollider = false;
    public void CollOnOff(bool boolen)
    {
        if (isEndBoxCollider == false)
        {
            boxCol.enabled = boolen;
        }

    }
    void ResetDict()
    {
        if(isHitIntervelEven)
             damagedEnemy.Clear();
    }
    void ResetHealGround()
    {
        if (this.gameObject != null && healCount != 0)
        {
            healCount -= 1;
            float resetTime = waitNextHeal;

            Sequence seq = DOTween.Sequence()
           .AppendInterval(resetTime)
            .AppendCallback(() => boxCol.enabled = false)
            .AppendInterval(0.01f)
            .AppendCallback(() => boxCol.enabled = true)
           .AppendCallback(() => ResetHealGround());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy" && isHealSkill == false)
        {
            // �������� ���� ���� ���� �ִ�ġ�� �ʰ����� ���� ��쿡�� ����
     
                // �̹� �������� ���� ������ Ȯ���ϰ�, �������� ������ ���� ��쿡�� ����
                if ((!damagedEnemy.ContainsKey(collision)&& damagedEnemy.Count < maxDamagedEnemies) ||(isHitIntervelEven == false &&damagedEnemy.ContainsKey(collision) && damagedEnemy[collision] < hitCount))
                {

                    EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();

                    if (isAbsorberSkill == false)
                    {

                        if (isPlayerAttack == true)
                        {
                            if (player.transform.position.x > transform.position.x)
                            {
                                knockbackDir.x = -knockbackDir.x;
                            }
                        }

                        if(nonChangeNockDir)
                    {
                        if(player.transform.localScale.x >0)
                        {
                            knockbackDir.x = Mathf.Abs(knockbackDir.x);
                        }
                        else
                        {
                            knockbackDir.x = -Mathf.Abs(knockbackDir.x);
                        }
                    }
  
                    // ������ �������� ������ �������� ���� �� ����Ʈ�� �߰�
                    enemyHealth.damage2Enemy(damageArr, stiffnessTime, knockForce, knockbackDir, this.transform.position.x, isNockBackChangeDir, isSkill, ShakeTime, dmgRatio);

                    }
                    else
                    {
                        if (isPlayerAttack == true)
                        {
                            if (collision.transform.position.x > absorbPivot.transform.position.x)
                            {
                                absorbDir.x = -Mathf.Abs(absorbDir.x);
                            }
                            else
                            {
                                absorbDir.x = Mathf.Abs(absorbDir.x);
                            }
                        }
                        // ������ �������� ������ �������� ���� �� ����Ʈ�� �߰�
                        enemyHealth.damage2Enemy(damageArr, stiffnessTime, absorbPower, absorbDir, this.transform.position.x, isNockBackChangeDir, isSkill, ShakeTime, dmgRatio);
                    }


                if (isPosionAttack)
                {
                    enemyHealth.CreatPoisonPrefab(poisonDamage, damageInterval, damageCount);
                }
                if (isBleedingAttack)
                {
                    enemyHealth.CreatBleedingPrefab(bleedingDamage, bleedingDamageInterval, bleedingDamageCount);
                }

                if (!damagedEnemy.ContainsKey(collision))
                    {
                        damagedEnemy.Add(collision,1);
                    }
                    else
                    {
                        damagedEnemy[collision] += 1;
                    }
                    // �������� ���� �� ����Ʈ�� �߰�
                
                
            }
        }
        if (collision.tag == "Player" && isHealSkill == true)
        {
           PlayerHealthManager.Instance.HpUp(healPower);
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
    [ConditionalHide("isPosionAttack")]
    public int poisonDamage;
    [ConditionalHide("isPosionAttack")]
    public float damageInterval;
    [ConditionalHide("isPosionAttack")]
    public int damageCount;

    public bool isBleedingAttack;
    [ConditionalHide("isBleedingAttack")]
    public int bleedingDamage;
    [ConditionalHide("isBleedingAttack")]
    public float bleedingDamageInterval;
    [ConditionalHide("isBleedingAttack")]
    public int bleedingDamageCount;
    public LayerMask layerMask; // ������ ���̾�
    public string enemyTag = "Enemy"; // �÷��̾� �±�

    public GameObject CheckEnemy()
    {

        // �ֺ��� �ִ� ��� Collider�� ������
        Collider2D[] colliders;

        if (player.transform.localScale.x > 0)
        {
             colliders = Physics2D.OverlapBoxAll(new Vector2(transform.position.x + (trackingPivot), transform.position.y), boxSize, 0f, layerMask);
        }
        else
        {
              colliders = Physics2D.OverlapBoxAll(new Vector2(transform.position.x - (trackingPivot), transform.position.y), boxSize, 0f, layerMask);
        }
        // ������ Collider�� ��ȸ�ϸ鼭 Player �±׸� ���� ������Ʈ�� �ִ��� Ȯ��
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(enemyTag))
            {
                // Player �±׸� ���� ������Ʈ�� ������ Success ��ȯ
                return  col.gameObject;
            }
        }

        // �ֺ��� Player �±׸� ���� ������Ʈ�� ������ Failure ��ȯ
        return null;
    }



}
