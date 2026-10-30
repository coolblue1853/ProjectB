using DarkTonic.MasterAudio;
using UnityEngine;

/// <summary>게임 설정 (ES3 "settings.*"): 마스터 볼륨 · 화면 흔들림 배율 · 피격 플래시 · 줍기 필터. 시작 시 로드해 MasterAudio/HitFeedback 에 적용</summary>
public static class GameSettings
{
    public static float Volume { get; private set; } = 1f;   // 0~1
    public static float Shake { get; private set; } = 1f;    // 0~1
    public static bool Flash { get; private set; } = true;
    public static PickupFilter Pickup { get; private set; } = PickupFilter.All; // BasicFeatures.md 4절

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        Volume = ES3.KeyExists("settings.volume", SaveSlots.SharedFile) ? ES3.Load<float>("settings.volume", SaveSlots.SharedFile) : 1f;
        Shake = ES3.KeyExists("settings.shake", SaveSlots.SharedFile) ? ES3.Load<float>("settings.shake", SaveSlots.SharedFile) : 1f;
        Flash = ES3.KeyExists("settings.flash", SaveSlots.SharedFile) ? ES3.Load<bool>("settings.flash", SaveSlots.SharedFile) : true;
        int pickup = ES3.KeyExists("settings.pickup", SaveSlots.SharedFile) ? ES3.Load<int>("settings.pickup", SaveSlots.SharedFile) : 0;
        Pickup = System.Enum.IsDefined(typeof(PickupFilter), pickup) ? (PickupFilter)pickup : PickupFilter.All;
        Apply();
    }

    public static void SetVolume(float v) { Volume = Mathf.Clamp01(v); ES3.Save("settings.volume", Volume, SaveSlots.SharedFile); Apply(); }
    public static void SetShake(float v) { Shake = Mathf.Clamp01(v); ES3.Save("settings.shake", Shake, SaveSlots.SharedFile); Apply(); }
    public static void SetFlash(bool v) { Flash = v; ES3.Save("settings.flash", Flash, SaveSlots.SharedFile); Apply(); }
    public static void SetPickup(PickupFilter v) { Pickup = v; ES3.Save("settings.pickup", (int)Pickup, SaveSlots.SharedFile); }

    static void Apply()
    {
        if (MasterAudio.SafeInstance != null)
        {
            MasterAudio.MasterVolumeLevel = Volume;
        }
        HitFeedback.ShakeScale = Shake;
        HitFeedback.FlashEnabled = Flash;
    }
}
