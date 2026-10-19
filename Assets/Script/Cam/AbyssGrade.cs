using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 메이드 인 어비스 색감 (ArtStyle.md 머리말): 낮은 채도, 청록으로 들어 올린 그림자, 따뜻한 하이라이트, 옅은 비네트.
/// 기존 배경·몹·소품을 다시 칠하지 않고 한 번에 같은 분위기로 묶는다. 씬을 저장하지 않으려고 런타임에 전역 Volume 으로 붙인다.
/// 프로필은 Resources/PostFx/AbyssGrade.asset, 값은 Tools/probes/make_abyss_grade.cs 로 다시 만든다.
/// </summary>
public static class AbyssGrade
{
    public const bool Enabled = true;
    const string ProfilePath = "PostFx/AbyssGrade";
    const float Priority = 10f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Setup()
    {
        if (!Enabled)
        {
            return;
        }
        VolumeProfile profile = Resources.Load<VolumeProfile>(ProfilePath);
        if (profile == null)
        {
            Debug.LogWarning("[PB] AbyssGrade 프로필 없음: Resources/" + ProfilePath);
            return;
        }
        GameObject go = new GameObject("AbyssGrade");
        Object.DontDestroyOnLoad(go);
        Volume volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = Priority;
        volume.weight = 1f;
        volume.sharedProfile = profile;
    }
}
