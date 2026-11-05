using System;
using UnityEngine;

/// <summary>
/// 플레이어 수면 상태(F2-SLEEP). 자는 동안 PlayerStatus 의 InputLock·MoveLock 토큰을 잡는다(경직과 별개, 토큰 충돌 없음).
/// 규칙은 SleepRules. 호출: PlayerSleep.Apply(초) / 피격 시 PlayerHealthManager 가 PlayerSleep.OnHit().
/// </summary>
public sealed class PlayerSleep : MonoBehaviour
{
    private static PlayerSleep s_instance;

    private readonly SleepRules _rules = new SleepRules();
    private IDisposable _inputLock;
    private IDisposable _moveLock;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_instance = null;
    }

    public static bool IsSleeping => s_instance != null && s_instance._rules.Sleeping;

    public static bool Apply(float seconds)
    {
        PlayerController pc = PlayerController.instance;
        if (pc == null)
        {
            return false;
        }
        if (s_instance == null && !pc.TryGetComponent(out s_instance))
        {
            s_instance = pc.gameObject.AddComponent<PlayerSleep>();
        }
        return s_instance.Begin(seconds);
    }

    /// <summary>피격 알림 — 깨운다</summary>
    public static void OnHit()
    {
        if (s_instance != null && s_instance._rules.Hit())
        {
            s_instance.Wake();
        }
    }

    private bool Begin(float seconds)
    {
        if (!_rules.TryApply(seconds))
        {
            return false;
        }
        _inputLock = PlayerStatus.Acquire(StatusFlag.InputLock, "Sleep");
        _moveLock = PlayerStatus.Acquire(StatusFlag.MoveLock, "Sleep");
        return true;
    }

    private void Wake()
    {
        _rules.Clear();
        _inputLock?.Dispose();
        _moveLock?.Dispose();
        _inputLock = null;
        _moveLock = null;
    }

    private void Update()
    {
        if (_rules.Tick(Time.deltaTime))
        {
            Wake();
        }
    }

    private void OnDisable()
    {
        Wake();
    }
}
