using System;
using UnityEngine;

/// <summary>펫 물약 자동 먹기 순수 규칙 (테스트 가능)</summary>
public static class PetPotionRules
{
    public static readonly int[] PercentSteps = { 20, 30, 40, 50, 60, 70, 80 };
    public const int DefaultHpPercent = 40;
    public const int DefaultMpPercent = 30;

    /// <summary>현재/최대가 기준 % 이하이면 마실 때. 죽었거나(현재 0 이하) 최대값이 없으면 false</summary>
    public static bool NeedsPotion(int current, int max, int percent)
    {
        return max > 0 && current > 0 && current * 100 <= percent * max;
    }

    /// <summary>다음 기준 %(목록 끝에서 처음으로 돈다). 목록에 없는 값은 가장 가까운 윗단계</summary>
    public static int NextPercent(int percent)
    {
        for (int i = 0; i < PercentSteps.Length; i++)
        {
            if (PercentSteps[i] > percent)
            {
                return PercentSteps[i];
            }
        }
        return PercentSteps[0];
    }

    /// <summary>효과 문자열이 정확히 "stat +" 한 줄짜리 소비 아이템인가(음식처럼 여러 효과가 섞인 것은 자동으로 먹지 않는다)</summary>
    public static bool IsSimpleRestore(string effectOb, string stat)
    {
        return effectOb != null && effectOb.Trim() == stat + " +";
    }
}

/// <summary>
/// 펫 (2층계 해금 · 장비 아님 · EQUIP-SLOT-REFORM v2): 물약 자동 먹기. HP·MP 각각 기준 %로 켜고, 기존 소비 경로(InventoryManager.TryConsume)와
/// 공용 쿨다운(ConsumeCooldown.Shared)을 그대로 쓴다. 기본 꺼짐. MP = 이 게임의 두 번째 자원인 스태미나(stemina) — 임시 가정(OpenQuestions).
/// 펫 그림은 플레이스홀더(아직 없음). 설정 UI 는 ExtraEquipPanel 의 펫 줄(마우스 클릭·키보드/패드 장비 커서).
/// ES3 키(추가만): feature.pet.auto(bool), feature.pet.hpPct(int), feature.pet.mpPct(int)
/// </summary>
public static class PetFeature
{
    public const string KeyAuto = "feature.pet.auto";
    public const string KeyHp = "feature.pet.hpPct";
    public const string KeyMp = "feature.pet.mpPct";

    static bool _loaded;
    static bool _auto;
    static int _hpPct = PetPotionRules.DefaultHpPercent;
    static int _mpPct = PetPotionRules.DefaultMpPercent;

    public static event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _loaded = false;
        Changed = null;
    }

    public static bool IsUnlocked => ContentFeatures.IsUnlocked(ContentFeatures.Pet);

    public static bool Enabled
    {
        get
        {
            EnsureLoaded();
            return _auto;
        }
    }

    public static int HpPercent
    {
        get
        {
            EnsureLoaded();
            return _hpPct;
        }
    }

    public static int MpPercent
    {
        get
        {
            EnsureLoaded();
            return _mpPct;
        }
    }

    static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }
        _loaded = true;
        _auto = false;
        _hpPct = PetPotionRules.DefaultHpPercent;
        _mpPct = PetPotionRules.DefaultMpPercent;
        try
        {
            _auto = SaveUtil.LoadOrThrow(KeyAuto, false);
            _hpPct = Mathf.Clamp(SaveUtil.LoadOrThrow(KeyHp, PetPotionRules.DefaultHpPercent), 1, 99);
            _mpPct = Mathf.Clamp(SaveUtil.LoadOrThrow(KeyMp, PetPotionRules.DefaultMpPercent), 1, 99);
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Item, "펫 설정 읽기 실패, 기본값(꺼짐)으로 시작: " + e.Message);
        }
    }

    public static void ReloadForTests()
    {
        _loaded = false;
    }

    /// <summary>자동 먹기 켜기/끄기. 잠겨 있으면 false</summary>
    public static bool Toggle()
    {
        if (!IsUnlocked)
        {
            return false;
        }
        EnsureLoaded();
        _auto = !_auto;
        Save();
        return true;
    }

    /// <summary>HP 기준 %를 다음 단계로(잠금이면 false)</summary>
    public static bool CycleHp()
    {
        if (!IsUnlocked)
        {
            return false;
        }
        EnsureLoaded();
        _hpPct = PetPotionRules.NextPercent(_hpPct);
        Save();
        return true;
    }

    public static bool CycleMp()
    {
        if (!IsUnlocked)
        {
            return false;
        }
        EnsureLoaded();
        _mpPct = PetPotionRules.NextPercent(_mpPct);
        Save();
        return true;
    }

    static void Save()
    {
        try
        {
            ES3.Save(KeyAuto, _auto);
            ES3.Save(KeyHp, _hpPct);
            ES3.Save(KeyMp, _mpPct);
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Item, "펫 설정 저장 실패: " + e.Message);
        }
        Changed?.Invoke();
    }

    /// <summary>주기적으로 부른다(PetAutoPotion). 기준 % 이하이면 HP·MP 물약을 하나씩 먹는다. 쿨다운 중이면 TryConsume 이 거절한다</summary>
    public static void Tick()
    {
        if (!IsUnlocked || !Enabled || Time.timeScale <= 0f)
        {
            return;
        }
        PlayerHealthManager phm = PlayerHealthManager.Instance;
        InventoryManager inv = InventoryManager.instance;
        if (phm == null || inv == null)
        {
            return;
        }
        if (PetPotionRules.NeedsPotion(phm.nowHp, phm.fullHP, _hpPct) && inv.ConsumeFirstByEffect("hp"))
        {
            return; // 같은 틱에 두 번 마시지 않는다(공용 쿨다운이 거절하기도 한다)
        }
        if (PetPotionRules.NeedsPotion(phm.nowStemina, phm.fullStemina, _mpPct))
        {
            inv.ConsumeFirstByEffect("stemina");
        }
    }
}

/// <summary>펫 자동 먹기 구동기: 씬 로드 후 한 번 만들어 0.4초마다 PetFeature.Tick 을 부른다</summary>
public sealed class PetAutoPotion : MonoBehaviour
{
    const float Interval = 0.4f;
    float _next;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindFirstObjectByType<PetAutoPotion>() != null)
        {
            return;
        }
        var go = new GameObject("PetAutoPotion");
        DontDestroyOnLoad(go);
        go.AddComponent<PetAutoPotion>();
    }

    void Update()
    {
        if (Time.unscaledTime < _next)
        {
            return;
        }
        _next = Time.unscaledTime + Interval;
        PetFeature.Tick();
    }
}
