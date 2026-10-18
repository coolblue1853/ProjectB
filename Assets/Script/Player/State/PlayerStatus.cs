using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public enum StatusFlag { Invincible, SuperArmor, MoveLock, LadderLock, InputLock, DashLock }

/// <summary>
/// 플레이어 상태 플래그를 ref-count 토큰으로 관리한다 (설계: .claude-rules/Game/PlayerStateDesign.md 2.2).
/// 여러 출처가 같은 플래그를 켜도 각자 자기 토큰만 해제하므로, 한 출처가 다른 출처의 무적·슈퍼아머·이동정지를 꺼버리는 버그(B-114)가 구조적으로 사라진다.
/// 기존 코드의 static bool 읽기/쓰기는 DatabaseManager의 프로퍼티(facade) → SetLegacy로 연결되어 그대로 동작한다.
/// </summary>
public static class PlayerStatus
{
    public static Func<float> Clock = () => Time.time;

    /// <summary>플래그가 꺼짐↔켜짐으로 바뀔 때만 발생</summary>
    public static event Action<StatusFlag, bool> Changed;

    sealed class StatusHandle : IDisposable
    {
        public readonly StatusFlag Flag;
        public readonly string Owner;
        public readonly string Where;
        public readonly float ExpireAt; // <0 = 만료 없음
        public bool Disposed;

        public StatusHandle(StatusFlag flag, string owner, string where, float expireAt)
        {
            Flag = flag;
            Owner = owner;
            Where = where;
            ExpireAt = expireAt;
        }

        public void Dispose()
        {
            Release(this);
        }
    }

    static readonly int FlagCount = Enum.GetValues(typeof(StatusFlag)).Length;
    static List<StatusHandle>[] handles = CreateLists();
    static StatusHandle[] legacy = new StatusHandle[FlagCount];

    static List<StatusHandle>[] CreateLists()
    {
        var arr = new List<StatusHandle>[Enum.GetValues(typeof(StatusFlag)).Length];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = new List<StatusHandle>(4);
        }
        return arr;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetStatics()
    {
        handles = CreateLists();
        legacy = new StatusHandle[FlagCount];
        Changed = null;
        Clock = () => Time.time;
    }

    public static bool Has(StatusFlag flag) => handles[(int)flag].Count > 0;

    public static int Count(StatusFlag flag) => handles[(int)flag].Count;

    /// <summary>플래그를 켠다. 돌려받은 핸들을 Dispose하면 이 출처의 몫만 해제된다(멱등).</summary>
    public static IDisposable Acquire(StatusFlag flag, string owner,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        return Add(new StatusHandle(flag, owner, Where(file, line), -1f));
    }

    /// <summary>seconds 뒤 자동 해제되는 토큰. 트윈·Invoke가 아니라 Tick에서 만료를 검사하므로 오브젝트가 파괴돼도 반드시 풀린다.</summary>
    public static IDisposable AcquireFor(StatusFlag flag, float seconds, string owner,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        return Add(new StatusHandle(flag, owner, Where(file, line), Clock() + Mathf.Max(0f, seconds)));
    }

    /// <summary>매 프레임 1회 (PlayerController.Update 첫 줄): 만료된 토큰 해제</summary>
    public static void Tick()
    {
        float now = Clock();
        for (int f = 0; f < FlagCount; f++)
        {
            List<StatusHandle> list = handles[f];
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].ExpireAt >= 0f && now >= list[i].ExpireAt)
                {
                    Release(list[i]);
                }
            }
        }
    }

    /// <summary>기존 static bool 쓰기용(facade). 플래그마다 레거시 토큰 1개만 켜고 끈다 → 다른 출처의 토큰은 끄지 못한다.</summary>
    public static void SetLegacy(StatusFlag flag, bool on)
    {
        int f = (int)flag;
        if (on && legacy[f] == null)
        {
            legacy[f] = (StatusHandle)Add(new StatusHandle(flag, "Legacy", "DatabaseManager", -1f));
        }
        else if (!on && legacy[f] != null)
        {
            StatusHandle h = legacy[f];
            legacy[f] = null;
            Release(h);
        }
    }

    /// <summary>사망·씬 전환·디버그용 전체 해제</summary>
    public static void ForceClear(string reason)
    {
        for (int f = 0; f < FlagCount; f++)
        {
            List<StatusHandle> list = handles[f];
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Release(list[i]);
            }
            legacy[f] = null;
        }
    }

    public static string Dump()
    {
        var sb = new StringBuilder();
        float now = Clock();
        for (int f = 0; f < FlagCount; f++)
        {
            List<StatusHandle> list = handles[f];
            if (list.Count == 0)
            {
                continue;
            }
            sb.Append((StatusFlag)f).Append(": ");
            foreach (StatusHandle h in list)
            {
                sb.Append(h.Owner).Append('(').Append(h.Where).Append(')');
                if (h.ExpireAt >= 0f)
                {
                    sb.Append(' ').Append((h.ExpireAt - now).ToString("F2")).Append("s");
                }
                sb.Append("  ");
            }
            sb.AppendLine();
        }
        return sb.Length == 0 ? "(none)" : sb.ToString();
    }

    static IDisposable Add(StatusHandle h)
    {
        List<StatusHandle> list = handles[(int)h.Flag];
        list.Add(h);
        if (list.Count == 1)
        {
            Changed?.Invoke(h.Flag, true);
        }
        return h;
    }

    static void Release(StatusHandle h)
    {
        if (h == null || h.Disposed)
        {
            return;
        }
        h.Disposed = true;
        List<StatusHandle> list = handles[(int)h.Flag];
        if (list.Remove(h) && list.Count == 0)
        {
            Changed?.Invoke(h.Flag, false);
        }
        if (legacy[(int)h.Flag] == h)
        {
            legacy[(int)h.Flag] = null;
        }
    }

    static string Where(string file, int line)
    {
        int slash = Math.Max(file.LastIndexOf('/'), file.LastIndexOf('\\'));
        return (slash >= 0 ? file.Substring(slash + 1) : file) + ":" + line;
    }
}
