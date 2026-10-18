using DG.Tweening;
using UnityEngine;

/// <summary>
/// DOTween 동시 트윈 용량을 시작 시 한 번 정한다. 기본(200/50)은 몹이 많은 맵(샛길 19마리 등)에서 시퀀스가 넘쳐
/// "Max Tweens reached: capacity has automatically been increased" 경고와 함께 실행 중 재할당이 일어난다(2026-10-19 회귀 소크).
/// </summary>
public static class TweenCapacityBoot
{
    private const int Tweeners = 500;
    private const int Sequences = 200;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
        DOTween.SetTweensCapacity(Tweeners, Sequences);
    }
}
