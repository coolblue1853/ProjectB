/// <summary>
/// 지면별 발소리 (AssetIntegrationPlan 6절 12). 현재 맵으로 지면을 정한다: 숲·늪 = 풀, 마을·산적 성채 = 흙, 탑 근교·2층계 = 돌.
/// 걷기 0.36초·달리기 0.27초 간격으로 한 번씩, 클립은 SfxLibrary 의 변형 4종 중 무작위.
/// </summary>
public static class Footsteps
{
    const float WalkInterval = 0.36f, RunInterval = 0.27f;
    static float last = -10f;

    public static void Step(bool running)
    {
        float now = UnityEngine.Time.time;
        if (now - last < (running ? RunInterval : WalkInterval))
        {
            return;
        }
        last = now;
        Sfx.Play(SurfaceFor(BgmDirector.CurrentMap), running ? 0.8f : 0.6f);
    }

    /// <summary>발이 땅에 닿는 포즈에서 바로 한 번(픽셀 리그가 부른다 — 타이머 간격은 다리 동작과 어긋났다, 10-26)</summary>
    public static void Contact(bool running)
    {
        last = UnityEngine.Time.time;
        Sfx.Play(SurfaceFor(BgmDirector.CurrentMap), running ? 0.8f : 0.6f);
    }

    /// <summary>착지: 지면 발소리를 세기(0~1)만큼 크게</summary>
    public static void Land(float strength)
    {
        last = UnityEngine.Time.time;
        Sfx.Play(SurfaceFor(BgmDirector.CurrentMap), 0.6f + 0.4f * UnityEngine.Mathf.Clamp01(strength));
    }

    public static SfxId SurfaceFor(string map)
    {
        switch (map)
        {
            case "Town":
            case "Lv1_BanditFortress":
                return SfxId.FootDirt;
            case "Lv1_TowerOutskirts":
            case "Lv2_BrokenRampart":
            case "Lv2_MagicCliffs":
                return SfxId.FootStone;
            default:
                return SfxId.FootGrass;
        }
    }
}
