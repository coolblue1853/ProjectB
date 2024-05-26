using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using AnyPortrait;
public class PlayerHealthManager : MonoBehaviour
{
    public apPortrait mainCharacter;
   // bool isSuperArmor = false;
    Sequence sequence; Sequence hpSequence;
    Sequence waitSequence; Sequence waitHpSequence;
    public int setHP;
    public int fullHP;
    public int nowHp;
    public int nomalizedHP;
    public int armorAtp = 0;
    public HealthBarShrink healthBar;
    public HealthBarShrink steminaBar;
    public HealthBarShrink fullnesBar;

    public int setStemina;
    public int fullStemina;
    public int nowStemina;
    public int nomalizedStemina;
    public float waitTimeSteminaHeal;
    public float waitTimeHpHeal;
    public int steminaHealPoint;
    public int HpHealPoint;
    bool isSteminaDown; bool isHpDown;
    public float intervalTimeSteminaHeal;
    public float intervalTimeHpHeal;
    public int fullFullness = 100;
    public int nowFullness;
    public int nomalizedFullness;

    public GameObject player;
    Rigidbody2D rb;

    Sequence noDamgeSycle;
    public float noDamgeTime;
    bool isNoDamge = false;
    public int Def;
    void Start()
    {
        rb = player.GetComponent<Rigidbody2D>();
        playerController = player.GetComponent<PlayerController>();
        ResetMHp();
        healthBar.ResetHp(fullHP);
        steminaBar.ResetHp(fullStemina);
        fullnesBar.ResetHp(fullFullness);
    }

    public void EquipmentActiveTrue(int hp, int armor)
    {
        int checkHP =  nowHp;
        // 방어도 적용은 나중에 시켜주어야 함.
        fullHP += hp;

        setHP = 1;
        nomalizedHP = 1 / fullHP;
        int minus = fullHP - checkHP;
        healthBar.ResetHp(fullHP);
        healthBar.healthSystem.Damage(minus);

    }
    public void EquipmentActiveFalse(int hp, int armor)
    {

        // 방어도 적용은 나중에 시켜주어야 함.
        fullHP -= hp;

        setHP = 1;
        nomalizedHP = 1 / fullHP;
        int minus = fullHP - nowHp;
        healthBar.ResetHp(fullHP);
        healthBar.healthSystem.Damage(minus);
        if (nowHp > fullHP)
        {
            nowHp = fullHP;
        }
    }
    //체력바 관련
    private void ResetMHp()
    {
        nowHp = fullHP;
        setHP = 1;
        nomalizedHP = 1 / fullHP;

        nowStemina = fullStemina;
        setStemina = 1;
        nomalizedStemina = 1 / fullStemina;

        nowFullness = fullFullness;
    }

    // Update is called once per frame
    void Update()
    {
        Def = DatabaseManager.playerDef;
        if (nowHp == 0)
        {
            Debug.Log("죽었습니다");
        }

        if(isAlphaChange == false && alphaSequence.IsActive() == true)
        {

            alphaSequence.Kill();
            mainCharacter.SetMeshAlphaAll(1f);
        }
    }
    PlayerController playerController;
    Sequence mysequence;
    Sequence alphaSequence;
    private void RepeatAnimation()
    {
        alphaSequence = DOTween.Sequence()
        // 애니메이션 함수 호출 및 반복 설정
        .AppendCallback(() => mainCharacter.SetMeshAlphaAll(0.85f))
        .AppendInterval(0.1f)
        .AppendCallback(() => mainCharacter.SetMeshAlphaAll(1f))
        .AppendInterval(0.1f)
        .OnComplete(() => RepeatAnimation());
    }
    bool isAlphaChange = false;
    public void damage2Player(int damage, float stiffTime, float force, Vector2 knockbackDir, float x, bool isDirChange)
    {

        if(isNoDamge == false && DatabaseManager.isInvincibility == false)
        {
            isAlphaChange = true;
            RepeatAnimation();
            noDamgeSycle = DOTween.Sequence()      
            .AppendCallback(() => isNoDamge = true)
            .AppendCallback(() => mainCharacter.SetControlParamInt("Eye Param", 4))
            .AppendInterval(noDamgeTime)
            .AppendCallback(() => mainCharacter.SetControlParamInt("Eye Param", 0))
            .AppendCallback(() => isNoDamge = false)
            .AppendCallback(() => alphaSequence.Kill())
            .AppendCallback(() => isAlphaChange = false);



            int dmgAfDef = damage * (damage / (damage + DatabaseManager.playerDef)); // 방무 미적용
            HpDown(dmgAfDef);
            rb.velocity = Vector2.zero;
            if (DatabaseManager.isSuperArmor == false && stiffTime > 0)
            {
       
                mysequence.Kill(); // 재공격시 경직 시간 초기화.
                playerController.isAttacked = true;
                playerController.isAttackedUp = true;

            mysequence = DOTween.Sequence()
            .AppendCallback(() => KnockbackActive(force, knockbackDir, x, isDirChange))
            .AppendInterval(stiffTime)
            .OnComplete(() => EndStiffness());
            }
            else if (DatabaseManager.isSuperArmor == true && stiffTime > 0)
            {
                KnockbackActive(force*0.1f, knockbackDir, x, isDirChange);
            }

        }


    }
    private void KnockbackActive(float knockbackForce, Vector2 knockbackDir, float x, bool isDirChange)
    {

        if (rb != null)
        {
            if (x < transform.position.x && isDirChange == true)
            {
                knockbackDir.x = -knockbackDir.x;
            }
            // 힘을 가해 넉백을 적용
            rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);
        }

    }
    private void EndStiffness()
    {
        if (this != null)
        {
            playerController.isAttacked = false;


        }

    }
    private static PlayerHealthManager instance = null;

    public void FullnessUp(int healed)
    {
        if (healed > fullFullness - nowFullness)
        {
            healed = fullFullness - nowFullness;

            fullnesBar.healthSystem.Heal(healed);
        }
        else
        {
            nowFullness += healed;

            fullnesBar.healthSystem.Heal(healed);
        }
    }
    public void FullnessDown(int damage)
    {
        nowFullness -= damage;
        fullnesBar.healthSystem.Damage(damage);
    }


    public void HpDown(int damage)
    {
        //DamageNumber damageNumber = numberPrefab.Spawn(player.transform.position, damage);
        nowHp -= damage;
        setHP = (setHP - nomalizedHP * damage);
        healthBar.healthSystem.Damage(damage);

        isHpDown = true;
        hpSequence.Kill();
        waitHpSequence.Kill();
        hpSequence = DOTween.Sequence()
        .AppendInterval(waitTimeHpHeal) // 대기 시간 사용
        .OnComplete(() => OnHpSequenceComplete());
    }


    public void SteminaDown(int damage)
    {
        isSteminaDown = true;
        sequence.Kill();
        waitSequence.Kill();
        nowStemina -= damage;
        setStemina = setStemina - nomalizedStemina * damage;
        steminaBar.healthSystem.Damage(damage);
        sequence = DOTween.Sequence()
        .AppendInterval(waitTimeSteminaHeal) // 대기 시간 사용
        .OnComplete(() => OnSequenceComplete());
    }

    private void OnSequenceComplete()
    {
        if(nowFullness >= 1)
        {
            FullnessDown(1);

            isSteminaDown = false;
            if (nowStemina < fullStemina)
            {
                if (fullStemina - nowStemina < steminaHealPoint)
                {
                    SteminaUp(fullStemina - nowStemina);
                }
                else
                {
                    SteminaUp(steminaHealPoint);
                }

                if (isSteminaDown == false)
                {
                    waitSequence = DOTween.Sequence()
                        .AppendInterval(intervalTimeSteminaHeal)
                        .OnComplete(() => OnSequenceComplete());
                }

            }
        }
       
    }
    private void OnHpSequenceComplete()
    {
        if (nowFullness >= 3)
        {
            FullnessDown(3);

            isHpDown = false;
            if (nowHp < fullHP)
            {
                if (fullHP - nowHp < HpHealPoint)
                {
                    HpUp(fullHP - nowHp);
                }
                else
                {
                    HpUp(HpHealPoint);
                }

                if (isHpDown == false)
                {
                    waitHpSequence = DOTween.Sequence()
                        .AppendInterval(intervalTimeHpHeal)
                        .OnComplete(() => OnHpSequenceComplete());
                }

            }
        }

    }
    public void HpUp(int healed)
    {
        if (healed > fullHP - nowHp)
        {
            healed = fullHP - nowHp;
            nowHp += healed;
            setHP = (setHP - nomalizedHP * healed);
            healthBar.healthSystem.Heal(healed);
        }
        else
        {
            nowHp += healed;
            setHP = (setHP - nomalizedHP * healed);
            healthBar.healthSystem.Heal(healed);
        }
    }
    public void SteminaUp(int damage)
    {
        if (damage > fullStemina - nowStemina)
        {
            damage = fullStemina - nowStemina;
            nowStemina += damage;
            setStemina = setStemina - nomalizedStemina * damage;
            steminaBar.healthSystem.Heal(damage);
        }
        else
        {
            nowStemina += damage;
            setStemina = setStemina - nomalizedStemina * damage;
            steminaBar.healthSystem.Heal(damage);
        }
    }
    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    //게임 매니저 인스턴스에 접근할 수 있는 프로퍼티. static이므로 다른 클래스에서 맘껏 호출할 수 있다.
    public static PlayerHealthManager Instance
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
}
