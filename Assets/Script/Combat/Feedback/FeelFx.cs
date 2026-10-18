using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

/// <summary>
/// Feel(MMFeedbacks) 연출 프리셋 (AssetIntegrationPlan W2). HitFeedback 파사드 옆에서 "큰 순간"만 담당한다.
/// DOTween 은 UI·짧은 트윈 담당으로 그대로 두고, 여기서는 MMF_Player 를 코드로 만들어 재사용한다.
/// 설정의 화면 흔들림 배율(HitFeedback.ShakeScale)이 0 이면 크기 연출도 끈다.
/// </summary>
public static class FeelFx
{
    static readonly Dictionary<Transform, MMF_Player> pops = new Dictionary<Transform, MMF_Player>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => pops.Clear();

    /// <summary>대상이 한 번 크게 부풀었다 돌아온다 (강화 성공·승급·보스 처치 등). amount 0.15 = 기준 크기의 +15%</summary>
    public static void Pop(Transform target, float amount = 0.15f, float duration = 0.35f)
    {
        if (target == null || HitFeedback.ShakeScale <= 0f)
        {
            return;
        }
        if (!pops.TryGetValue(target, out MMF_Player player) || player == null)
        {
            player = Build(target);
            pops[target] = player;
        }
        MMF_Scale scale = player.GetFeedbackOfType<MMF_Scale>();
        // Additive: 현재 크기에 더한다 → 기준 크기에 비례하게
        scale.RemapCurveZero = 0f;
        scale.RemapCurveOne = Mathf.Abs(target.localScale.y) * amount * HitFeedback.ShakeScale;
        scale.AnimateScaleDuration = duration;
        player.StopFeedbacks();
        player.PlayFeedbacks();
    }

    /// <summary>플레이어 몸(메시) 팝 — 루트 스케일은 방향에 쓰이므로 건드리지 않는다</summary>
    public static void PlayerPop(float amount = 0.15f)
    {
        PlayerController pc = PlayerController.instance;
        if (pc != null && pc.mainCharacter != null && pc.mainCharacter.transform != pc.transform)
        {
            Pop(pc.mainCharacter.transform, amount);
        }
    }

    static MMF_Player Build(Transform target)
    {
        var go = new GameObject("[FeelFx] " + target.name);
        go.transform.SetParent(target.parent != null ? target.parent : target, false);
        var player = go.AddComponent<MMF_Player>();
        player.InitializationMode = MMFeedbacks.InitializationModes.Script;
        var scale = new MMF_Scale
        {
            AnimateScaleTarget = target,
            Mode = MMF_Scale.Modes.Additive,
            UniformScaling = true,
            AnimateScaleTweenX = new MMTweenType(new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.3f, 1f), new Keyframe(1f, 0f))),
        };
        player.AddFeedback(scale);
        player.Initialization();
        return player;
    }
}
