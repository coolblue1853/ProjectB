using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Feel(MMFeedbacks) 연출 프리셋 (AssetIntegrationPlan W2). HitFeedback 파사드 옆에서 "큰 순간"만 담당한다.
/// DOTween 은 UI·짧은 트윈 담당으로 그대로 두고, 여기서는 MMF_Player 를 코드로 만들어 재사용한다.
/// 설정의 화면 흔들림 배율(HitFeedback.ShakeScale)이 0 이면 크기 연출도 끈다.
/// </summary>
public static class FeelFx
{
    static readonly Dictionary<Transform, MMF_Player> pops = new Dictionary<Transform, MMF_Player>();

    static MMF_Player flashPlayer;
    static Image flashImage;
    static Tween slowRestore;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        pops.Clear();
        flashPlayer = null;
        flashImage = null;
        slowRestore = null;
    }

    // ── 순간별 프리셋 (AssetIntegrationPlan 6절 2) ──
    static readonly Color RankColor = new Color(0.45f, 0.95f, 0.9f);
    static readonly Color EnhanceColor = new Color(1f, 0.78f, 0.35f);
    static readonly Color UniqueColor = new Color(1f, 0.85f, 0.45f);
    static readonly Color PortalColor = new Color(0.55f, 0.5f, 0.78f);

    /// <summary>탐험가 랭크업: 청록 플래시 + 몸 팝</summary>
    public static void RankUp()
    {
        Flash(RankColor, 0.35f, 0.45f);
        PlayerPop(0.25f);
    }

    /// <summary>강화 성공: +5 이상이면 호박색 플래시</summary>
    public static void EnhanceSuccess(int newLevel)
    {
        if (newLevel >= 5)
        {
            Flash(EnhanceColor, 0.3f, 0.35f);
        }
        PlayerPop(newLevel >= 5 ? 0.2f : 0.1f);
    }

    /// <summary>희귀 드롭: 드롭 오브젝트 팝, 유니크(U)는 금빛 플래시까지</summary>
    public static void RareDrop(Transform drop, string rarity)
    {
        if (rarity != "R" && rarity != "U")
        {
            return;
        }
        Pop(drop, rarity == "U" ? 0.35f : 0.2f, 0.4f);
        if (rarity == "U")
        {
            Flash(UniqueColor, 0.25f, 0.4f);
        }
    }

    /// <summary>보스 처치: 0.25배 슬로모션 0.5초(실시간) + 흰 플래시 + 몸 팝</summary>
    public static void BossKill()
    {
        SlowMo(0.25f, 0.5f);
        Flash(Color.white, 0.45f, 0.6f);
        PlayerPop(0.2f);
    }

    /// <summary>포털 진입: 순간이동 직후 라벤더에서 걷히는 페이드로 맵 전환을 가린다</summary>
    public static void PortalEnter()
    {
        Flash(PortalColor, 0.85f, 0.5f, true);
    }

    /// <summary>
    /// 화면 플래시: 오버레이 이미지 알파를 Feel MMF_ImageAlpha 로 0→peak→0 (fromPeak 면 peak 에서 시작해 걷힘).
    /// 흔들림 배율 0(접근성 설정)이면 끈다.
    /// </summary>
    public static void Flash(Color color, float peak, float duration, bool fromPeak = false)
    {
        if (HitFeedback.ShakeScale <= 0f)
        {
            return;
        }
        EnsureFlash();
        flashImage.color = new Color(color.r, color.g, color.b, 0f);
        MMF_ImageAlpha alpha = flashPlayer.GetFeedbackOfType<MMF_ImageAlpha>();
        alpha.Duration = duration;
        alpha.CurveRemapOne = peak;
        alpha.Curve = new MMTweenType(fromPeak
            ? new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.35f, 0.8f), new Keyframe(1f, 0f))
            : new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.2f, 1f), new Keyframe(1f, 0f)));
        flashPlayer.StopFeedbacks();
        flashPlayer.PlayFeedbacks();
    }

    /// <summary>
    /// 슬로모션. Feel 의 MMF_TimescaleModifier 는 MMTimeManager 가 매 프레임 timeScale 을 덮어써 일시정지(0)와 충돌하므로 쓰지 않는다.
    /// 정상 속도(1)일 때만 걸고, 끝날 때 아직 우리가 건 값이면 되돌린다.
    /// </summary>
    public static void SlowMo(float scale, float realSeconds)
    {
        if (!Mathf.Approximately(Time.timeScale, 1f))
        {
            return;
        }
        Time.timeScale = scale;
        slowRestore?.Kill();
        slowRestore = DOVirtual.DelayedCall(realSeconds, () =>
        {
            if (Mathf.Approximately(Time.timeScale, scale))
            {
                Time.timeScale = 1f;
            }
        }, true).SetUpdate(true);
    }

    static void EnsureFlash()
    {
        if (flashPlayer != null && flashImage != null)
        {
            return;
        }
        var root = new GameObject("[FeelFx] Flash");
        Object.DontDestroyOnLoad(root);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900; // 게임 UI 위, 대화창보다 아래 상관없음(짧게 지나감)
        var imgGo = new GameObject("Image");
        imgGo.transform.SetParent(root.transform, false);
        flashImage = imgGo.AddComponent<Image>();
        flashImage.raycastTarget = false;
        var rt = flashImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        flashImage.color = new Color(1f, 1f, 1f, 0f);
        flashPlayer = root.AddComponent<MMF_Player>();
        flashPlayer.InitializationMode = MMFeedbacks.InitializationModes.Script;
        flashPlayer.AddFeedback(new MMF_ImageAlpha { BoundImage = flashImage, Mode = MMF_ImageAlpha.Modes.OverTime, CurveRemapZero = 0f });
        flashPlayer.Initialization();
    }

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
        if (pc != null && pc.Rig != null && pc.Rig.transform != pc.transform)
        {
            Pop(pc.Rig.transform, amount);
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
