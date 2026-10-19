using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;

public enum SfxId
{
    Hit, Crit, Kill, Pickup, Money, Craft, CraftFail, UiMove, UiOpen, Victory, RareDrop, PlayerDeath, Portal, BossSlam,
    // 2026-10-19 추가(직렬화가 정수라 반드시 끝에 덧붙인다): 강화·아티팩트·지면별 발소리
    EnhanceSuccess, EnhanceGreat, EnhanceFail, ArtifactHeal, ArtifactShell, ArtifactTime, ArtifactStar, FootGrass, FootDirt, FootStone,
}

/// <summary>
/// 게임 이벤트용 효과음 (보유 8-Bit Sfx / Frost Mage SFX). 클립은 Resources/Audio/SfxLibrary.asset에서 읽는다.
/// MasterAudio 전역 볼륨을 따르고, 같은 소리가 한 프레임에 여러 번 나도 1번만 재생한다(광역 타격 소음 방지).
/// </summary>
public static class Sfx
{
    const int Voices = 8;
    const float MinInterval = 0.04f;

    static SfxLibrary library;
    static AudioSource[] sources;
    static int next;
    static readonly Dictionary<SfxId, float> lastPlayed = new Dictionary<SfxId, float>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        library = null;
        sources = null;
        next = 0;
        lastPlayed.Clear();
    }

    public static void Play(SfxId id, float volume = 1f)
    {
        if (!EnsureInit())
        {
            return;
        }
        SfxLibrary.Entry entry = library.Get(id);
        AudioClip clip = entry != null ? entry.Pick() : null;
        if (clip == null)
        {
            return;
        }
        float now = Time.unscaledTime;
        if (lastPlayed.TryGetValue(id, out float t) && now - t < MinInterval)
        {
            return;
        }
        lastPlayed[id] = now;

        AudioSource src = sources[next];
        next = (next + 1) % sources.Length;
        src.pitch = 1f + Random.Range(-entry.pitchJitter, entry.pitchJitter);
        float master = MasterAudio.SafeInstance != null ? MasterAudio.MasterVolumeLevel : 1f;
        src.PlayOneShot(clip, entry.volume * volume * master);
        GameLog.Info(GameLog.Cat.Audio, "SFX " + id);
    }

    static bool EnsureInit()
    {
        if (sources != null && sources[0] != null)
        {
            return library != null;
        }
        library = Resources.Load<SfxLibrary>("Audio/SfxLibrary");
        var go = new GameObject("[Sfx]");
        Object.DontDestroyOnLoad(go);
        sources = new AudioSource[Voices];
        for (int i = 0; i < Voices; i++)
        {
            sources[i] = go.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            sources[i].spatialBlend = 0f;
        }
        return library != null;
    }
}
