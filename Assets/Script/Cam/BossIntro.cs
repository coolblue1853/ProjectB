using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 보스 등장 연출 (AssetIntegrationPlan W2 ProCamera): 카메라가 보스 쪽으로 끌려가며 살짝 줌아웃, 이름 배너, 1.6초 뒤 복귀.
/// 같은 보스는 리스폰마다 반복하지 않게 60초 쿨다운.
/// </summary>
public static class BossIntro
{
    const float Hold = 1.6f, Ease = 0.5f, Cooldown = 60f, ZoomOut = 1.5f;
    static readonly Dictionary<string, float> lastShown = new Dictionary<string, float>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => lastShown.Clear();

    public static void Play(EnemyHealth boss)
    {
        ProCamera2D cam = ProCamera2D.Instance;
        if (cam == null || boss == null)
        {
            return;
        }
        string key = boss.name.Replace("(Clone)", "").Trim();
        if (lastShown.TryGetValue(key, out float t) && Time.time - t < Cooldown)
        {
            return;
        }
        lastShown[key] = Time.time;
        Transform target = boss.transform;
        cam.AddCameraTarget(target, 1f, 1f, Ease);
        cam.Zoom(ZoomOut, Ease);
        HitFeedback.Shake(HitFeedback.ShakeTier.Heavy);
        Sfx.Play(SfxId.BossSlam, 0.6f);
        BossBanner.Show(MonsterTable.DisplayName(key), "A guardian of the Tower appears", true);
        GameLog.Info(GameLog.Cat.AI, "보스 등장 연출: " + key);
        DOVirtual.DelayedCall(Hold, () =>
        {
            if (ProCamera2D.Instance != null)
            {
                ProCamera2D.Instance.RemoveCameraTarget(target, Ease);
                ProCamera2D.Instance.Zoom(-ZoomOut, Ease);
            }
        }, false);
    }
}
