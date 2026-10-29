using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using AnyPortrait;
using System;
public class PlayerHealthManager : MonoBehaviour
{

    public apPortrait mainCharacter;
    /// <summary>활성 캐릭터 리그(픽셀 리그 또는 AnyPortrait 래퍼)</summary>
    CharacterRig Rig => playerController != null ? playerController.Rig : null;
   // bool isSuperArmor = false;
    Sequence sequence; Sequence hpSequence;
    Sequence waitSequence; Sequence waitHpSequence;
    public int fullHP;
    public int nowHp;
    public int armorAtp = 0;
    public HealthBarShrink healthBar;
    public HealthBarShrink steminaBar;
    public HealthBarShrink fullnesBar;

    Sequence noDamgeSycle;
    public int fullStemina;
    public int nowStemina;
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

    public float noDamgeTime;
    bool isNoDamge = false;
    public int Def;
    public int Critical;
    public int Drop;

    public event EventHandler OnHpDamaged;
    public event EventHandler OnHpHealed;
    public event EventHandler OnSteminaDamaged;
    public event EventHandler OnSteminaHealed;
    public event EventHandler OnFullnessDamaged;
    public event EventHandler OnFullnessHealed;

    public PlayerHealthManager(int healthAmount)
    {
        fullHP = healthAmount;
        this.nowHp = healthAmount;
    }

    public void HpDamage(int amount)
    {
        nowHp -= amount;
        if (nowHp < 0)
        {
            nowHp = 0;
        }
        if (OnHpDamaged != null) OnHpDamaged(this, EventArgs.Empty);
    }
    [Tooltip("플레이어 회복·피격 숫자 프리팹 (적과 같은 EnemyDamge)")]
    [SerializeField] private DamageNumbersPro.DamageNumber _numberPrefab;

    Vector3 NumberAnchor()
    {
        Transform t = PlayerController.instance != null ? PlayerController.instance.transform : transform;
        return t.position + Vector3.up * 1.2f;
    }

    public void HpHeal(int amount)
    {
        int before = nowHp;
        nowHp += amount;
        if (nowHp > fullHP)
        {
            nowHp = fullHP;
        }
        if (nowHp > before)
        {
            HitFeedback.SpawnNumber(_numberPrefab, NumberAnchor(), nowHp - before, HitFeedback.NumberKind.Heal);
        }
        if (OnHpHealed != null) OnHpHealed(this, EventArgs.Empty);
    }
    public void FullnessDamage(int amount)
    {
        nowFullness -= amount;
        if (nowFullness < 0)
        {
            nowFullness = 0;
        }
        if (OnFullnessDamaged != null) OnFullnessDamaged(this, EventArgs.Empty);
    }
    public void FullnessHeal(int amount)
    {
        nowFullness += amount;
        if (nowFullness > fullFullness)
        {
            nowFullness = fullFullness;
        }
        if (OnFullnessHealed != null) OnFullnessHealed(this, EventArgs.Empty);
    }
    public void SteminaDamage(int amount)
    {
        nowStemina -= amount;
        if (nowStemina < 0)
        {
            nowStemina = 0;
        }
        if (OnSteminaDamaged != null) OnSteminaDamaged(this, EventArgs.Empty);
    }
    public void SteminaHeal(int amount)
    {
        nowStemina += amount;
        if (nowStemina > fullStemina)
        {
            nowStemina = fullStemina;
        }
        if (OnSteminaHealed != null) OnSteminaHealed(this, EventArgs.Empty);
    }
    public float GetHpNormalized()
    {
        return (float)nowHp / fullHP;
    }
    public float GetFullnessNormalized()
    {
        return (float)nowFullness / fullFullness;
    }
    public float GetSteminaNormalized()
    {
        return (float)nowStemina / fullStemina;
    }





    void Start()
    {
        rb = player.GetComponent<Rigidbody2D>();
        playerController = player.GetComponent<PlayerController>();
        ResetMHp();
        healthBar.ResetHealthStat(this);
        steminaBar.ResetSteminaStat(this);
        fullnesBar.ResetFullnessStat(this);
    }

    public void EquipmentActiveTrue(int hp)
    {
        // 방어도 적용은 나중에 시켜주어야 함.
        fullHP += hp;

        healthBar.ResetHealthStat(this);
        //HpDamage(minus); 
        waitHpSequence.Kill();
        HpUp(hp);
    }
    public void EquipmentActiveFalse(int hp)
    {

        // 방어도 적용은 나중에 시켜주어야 함.
        fullHP -= hp;
        healthBar.ResetHealthStat(this);
        // 최대 HP가 줄어 현재 HP가 넘치면 최대치로 깎는다 (HP가 차 있지 않으면 그대로)
        if (nowHp > fullHP)
        {
            HpDamage(nowHp - fullHP);
        }
    }
    //체력바 관련
    // ── 사망 · 부활 (메이플식: 마을에서 HP·스태미나 가득 찬 채로 부활) ──
    public static event System.Action PlayerDied;
    public static event System.Action PlayerRespawned;
    public bool IsDead => isDead;
    bool isDead;
    System.IDisposable deathInputLock, deathInvincible;
    const float RespawnDelay = 2.5f;

    void Die()
    {
        isDead = true;
        GameLog.Info(GameLog.Cat.Player, "플레이어 사망 → " + RespawnDelay + "초 뒤 마을 부활");
        Sfx.Play(SfxId.PlayerDeath);
        deathInputLock = PlayerStatus.Acquire(StatusFlag.InputLock, "Death");
        deathInvincible = PlayerStatus.Acquire(StatusFlag.Invincible, "Death");
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        if (Rig != null)
        {
            Rig.SetMeshAlphaAll(0.4f);
        }
        PlayerDied?.Invoke();
        DOVirtual.DelayedCall(RespawnDelay, Respawn, false);
    }

    void Respawn()
    {
        // 사냥터에서 죽었으면 마을로 가는 포털을 이용해 맵을 전환한다
        Portal toTown = null;
        foreach (Portal p in FindObjectsByType<Portal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (p.openMap != null && p.openMap.name == "Town" && p.isActiveAndEnabled)
            {
                toTown = p;
                break;
            }
        }
        if (toTown != null)
        {
            toTown.TravelNow(PlayerController.instance.gameObject);
        }
        else
        {
            MapInfo.GoToTown(PlayerController.instance.gameObject); // 마을 직행 포털이 없는 맵(Lv2 등)
        }
        nowHp = fullHP;
        nowStemina = fullStemina;
        HpUp(0);
        if (Rig != null)
        {
            Rig.SetMeshAlphaAll(1f);
        }
        deathInputLock?.Dispose();
        deathInputLock = null;
        // 부활 직후 2초 무적
        deathInvincible?.Dispose();
        deathInvincible = PlayerStatus.AcquireFor(StatusFlag.Invincible, 2f, "Respawn");
        isDead = false;
        if (healthBar != null)
        {
            healthBar.ResetHealthStat(this);
        }
        PlayerRespawned?.Invoke();
        GameLog.Info(GameLog.Cat.Player, "부활 완료 (마을 이동=" + (toTown != null) + ")");
    }

    private void ResetMHp()
    {
        nowHp = fullHP;

        nowStemina = fullStemina;

        nowFullness = fullFullness;
    }

    // Update is called once per frame
    void Update()
    {
        Def = DatabaseManager.playerDef;
        Critical = DatabaseManager.playerCritRate;
        Drop = DatabaseManager.playerDropRate;
        if (nowHp <= 0 && !isDead)
        {
            Die();
        }

        if(isAlphaChange == false && alphaSequence.IsActive() == true)
        {

            alphaSequence.Kill();
            Rig.SetMeshAlphaAll(1f);
        }
    }
    PlayerController playerController;
    Sequence mysequence;
    Sequence alphaSequence;
    const float InvincibleBlinkAlpha = 0.45f;
    private void RepeatAnimation()
    {
        alphaSequence = DOTween.Sequence()
        // 애니메이션 함수 호출 및 반복 설정
        // 불투명 먼저: 맞는 순간의 흰 번쩍임(PixelRig 0.07초)이 반투명으로 묻히지 않게. 0.85 는 거의 안 보였다 → 0.45(10-26)
        .AppendCallback(() => Rig.SetMeshAlphaAll(1f))
        .AppendInterval(0.1f)
        .AppendCallback(() => Rig.SetMeshAlphaAll(InvincibleBlinkAlpha))
        .AppendInterval(0.1f)
        .OnComplete(() => RepeatAnimation());
    }
    bool isAlphaChange = false;
    public void damage2Player(int damage, float stiffTime, float force, Vector2 knockbackDir, float x, bool isDirChange)
    {

        if (isNoDamge == false && DatabaseManager.isInvincibility == true)
        {
            HitFeedback.SpawnText(_numberPrefab, NumberAnchor(), "IMMUNE", HitFeedback.TextKind.Immune); // 대시·스킬 무적으로 흘린 공격
        }
        if(isNoDamge == false && DatabaseManager.isInvincibility == false)
        {
            isAlphaChange = true;
            RepeatAnimation();
            noDamgeSycle = DOTween.Sequence()      
            .AppendCallback(() => isNoDamge = true)
            .AppendCallback(() => Rig.SetControlParamInt("Eye Param", 4))
            .AppendInterval(noDamgeTime)
            .AppendCallback(() => Rig.SetControlParamInt("Eye Param", 0))
            .AppendCallback(() => isNoDamge = false)
            .AppendCallback(() => alphaSequence.Kill())
            .AppendCallback(() => isAlphaChange = false);

            int dmgAfDef = DamageCalculator.IncomingToPlayer(damage, Def, StatMath.IncomingDamageMul * AscentLoad.IncomingMul()); // 상승 부하: 받는 피해 증가
            HitFeedback.OnPlayerHit();
            if (dmgAfDef <= 0)
            {
                HitFeedback.SpawnText(_numberPrefab, NumberAnchor(), "BLOCK", HitFeedback.TextKind.Block); // 방어력이 전부 흡수
            }
            HpDown(dmgAfDef);
            rb.linearVelocity = Vector2.zero;
            if (DatabaseManager.isSuperArmor == false && stiffTime > 0)
            {
       
                mysequence.Kill(); // 재공격시 경직 시간 초기화.
                playerController.isAttacked = true;
                playerController.isAttackedUp = true;
                playerController.CancelSkillHold(); // 경직되면 홀드 스킬도 끊긴다 (B-113)

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
            FullnessHeal(healed);
        }
        else
        {
            FullnessHeal(healed);
        }
    }
    public void FullnessDown(int damage)
    {
        FullnessDamage(damage);
    }


    public void HpDown(int damage)
    {
        HitFeedback.SpawnNumber(_numberPrefab, NumberAnchor(), damage, HitFeedback.NumberKind.Hurt);
        HpDamage(damage);

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
        SteminaDamage(damage);
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
            HpHeal(healed);
        }
        else
        {
            HpHeal(healed);
        }
    }
    public void SteminaUp(int damage)
    {
        if (damage > fullStemina - nowStemina)
        {
            damage = fullStemina - nowStemina;
            SteminaHeal(damage);
        }
        else
        {
            SteminaHeal(damage);
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
