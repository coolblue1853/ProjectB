using System;
using System.Text;
using UnityEngine;

/// <summary>
/// 성능 기록: 1초마다 k:"pf"(평균·최악 프레임 시간, 끊김 수, GC 횟수, 관리 힙, 할당 추정), 50ms 넘는 프레임마다 k:"hitch".
/// 할당 추정 = 1초 동안 GC.GetTotalMemory 증가분(그 사이 GC 가 돌면 그 구간은 0 으로 본다). 에디터에서는 에디터 몫이 섞인다.
/// </summary>
public sealed class PlayRecPerf
{
    public const float HitchMs = 50f;
    public const long AllocSpikeBytes = 1024 * 1024;

    private int _frames;
    private float _sumMs;
    private float _maxMs;
    private int _hitches;
    private int _gc0;
    private long _heap0;
    private float _next;

    public float LastAvgMs { get; private set; }

    public void Reset(float real)
    {
        _frames = 0;
        _sumMs = 0f;
        _maxMs = 0f;
        _hitches = 0;
        _gc0 = GC.CollectionCount(0);
        _heap0 = GC.GetTotalMemory(false);
        _next = real + 1f;
    }

    /// <summary>매 프레임. 끊김이면 hitch 한 줄, 1초가 차면 pf 한 줄.</summary>
    public void Feed(float real, float dtReal)
    {
        float ms = dtReal * 1000f;
        _frames++;
        _sumMs += ms;
        if (ms > _maxMs)
        {
            _maxMs = ms;
        }
        if (ms > HitchMs)
        {
            _hitches++;
            WriteHitch(real, ms);
        }
        if (real < _next)
        {
            return;
        }
        _next = real + 1f;
        int gc = GC.CollectionCount(0);
        long heap = GC.GetTotalMemory(false);
        int gcDelta = gc - _gc0;
        long alloc = gcDelta == 0 ? Math.Max(0L, heap - _heap0) : 0L;
        LastAvgMs = _frames > 0 ? _sumMs / _frames : 0f;
        WritePf(real, gcDelta, heap, alloc);
        _gc0 = gc;
        _heap0 = heap;
        _frames = 0;
        _sumMs = 0f;
        _maxMs = 0f;
        _hitches = 0;
    }

    private void WritePf(float real, int gcDelta, long heap, long alloc)
    {
        lock (PlayRecWriter.Gate)
        {
            try
            {
                StringBuilder sb = PlayRecWriter.Begin("pf");
                PlayRecJson.KeyNum(sb, "u", real, 3);
                PlayRecJson.KeyInt(sb, "n", _frames);
                PlayRecJson.KeyNum(sb, "avg", LastAvgMs, 2);
                PlayRecJson.KeyNum(sb, "max", _maxMs, 1);
                PlayRecJson.KeyInt(sb, "hitch", _hitches);
                PlayRecJson.KeyInt(sb, "gc", gcDelta);
                PlayRecJson.KeyNum(sb, "heap", heap / 1048576.0, 1);
                PlayRecJson.KeyInt(sb, "alloc", alloc / 1024);
                if (alloc >= AllocSpikeBytes)
                {
                    PlayRecJson.KeyInt(sb, "spike", 1);
                }
                PlayRecWriter.Commit();
            }
            catch (Exception)
            {
                PlayRecWriter.Discard();
            }
        }
    }

    private static void WriteHitch(float real, float ms)
    {
        lock (PlayRecWriter.Gate)
        {
            try
            {
                StringBuilder sb = PlayRecWriter.Begin("hitch");
                PlayRecJson.KeyNum(sb, "u", real, 3);
                PlayRecJson.KeyNum(sb, "ms", ms, 1);
                PlayRecWriter.Commit();
            }
            catch (Exception)
            {
                PlayRecWriter.Discard();
            }
        }
    }
}
