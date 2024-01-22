using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class PlayerHealthManager : MonoBehaviour
{


    Sequence sequence;
    Sequence waitSequence;
    public int setHP;
    public int fullHP;
    public int nowHp;
    public int nomalizedHP;

    public HealthBarShrink healthBar;
    public HealthBarShrink steminaBar;
    public HealthBarShrink fullnesBar;

    public int setStemina;
    public int fullStemina;
    public int nowStemina;
    public int nomalizedStemina;
    public float waitTimeSteminaHeal;
    public int steminaHealPoint;
    bool isSteminaDown;
    public float intervalTimeSteminaHeal;

    public int fullFullness = 100;
    public int nowFullness;
    public int nomalizedFullness;
    void Start()
    {
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
        if(nowHp == 0)
        {
            Debug.Log("죽었습니다");
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
