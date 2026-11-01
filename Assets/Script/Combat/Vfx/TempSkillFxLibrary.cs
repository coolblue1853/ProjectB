using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>임시 PNG를 기존 Vfx/PixelFxClip 재생 경로에 연결한다.</summary>
public class TempSkillFxLibrary : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string skillId;
        public Texture2D texture;
        public int frameCount;
    }

    public List<Entry> entries = new List<Entry>();
    private readonly Dictionary<string, PixelFxClip> _clips = new Dictionary<string, PixelFxClip>();

    public static string SkillId(string skillName)
    {
        return string.IsNullOrEmpty(skillName) ? string.Empty : skillName.Replace(' ', '_');
    }

    public static bool TryParseFile(string path, out string skillId, out int frames)
    {
        skillId = Path.GetFileNameWithoutExtension(path);
        frames = 1;
        if (!string.Equals(Path.GetExtension(path), ".png", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        int suffix = skillId.LastIndexOf("_f", StringComparison.Ordinal);
        if (suffix >= 0)
        {
            if (!int.TryParse(skillId.Substring(suffix + 2), out frames) || frames < 1 || frames > 64)
            {
                return false;
            }
            skillId = skillId.Substring(0, suffix);
        }
        return !string.IsNullOrEmpty(skillId);
    }

    public PixelFxClip GetClip(string skillId)
    {
        if (string.IsNullOrEmpty(skillId) || entries == null)
        {
            return null;
        }
        if (_clips.TryGetValue(skillId, out PixelFxClip cached))
        {
            return cached;
        }
        Entry entry = null;
        foreach (Entry candidate in entries)
        {
            if (candidate == null || candidate.skillId != skillId)
            {
                continue;
            }
            if (entry != null)
            {
                return null; // 잘못 작성된 카탈로그도 중복 ID는 폴백한다.
            }
            entry = candidate;
        }
        if (entry == null || entry.texture == null || entry.frameCount < 1 || entry.frameCount > 64
            || entry.texture.width % entry.frameCount != 0)
        {
            return null;
        }
        var clip = CreateInstance<PixelFxClip>();
        clip.hideFlags = HideFlags.HideAndDontSave;
        clip.fps = entry.frameCount == 1 ? 5f : 20f;
        clip.source = "Assets/Art/Temp/FX/" + skillId;
        clip.frames = new Sprite[entry.frameCount];
        int width = entry.texture.width / entry.frameCount;
        for (int i = 0; i < entry.frameCount; i++)
        {
            clip.frames[i] = Sprite.Create(entry.texture,
                new Rect(i * width, 0, width, entry.texture.height), new Vector2(0.5f, 0.5f), 36f);
            clip.frames[i].hideFlags = HideFlags.HideAndDontSave;
        }
        _clips.Add(skillId, clip);
        return clip;
    }

    private void OnDisable()
    {
        ClearClips();
    }

    public void ClearClips()
    {
        foreach (PixelFxClip clip in _clips.Values)
        {
            foreach (Sprite sprite in clip.frames)
            {
                if (Application.isPlaying) { Destroy(sprite); }
                else { DestroyImmediate(sprite); }
            }
            if (Application.isPlaying) { Destroy(clip); }
            else { DestroyImmediate(clip); }
        }
        _clips.Clear();
    }
}
