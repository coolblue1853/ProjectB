using System.Collections.Generic;
using UnityEngine;

/// <summary>효과음 ID → 클립·볼륨 매핑 (Resources/Audio/SfxLibrary.asset)</summary>
[CreateAssetMenu(menuName = "ProjectB/Sfx Library")]
public class SfxLibrary : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public SfxId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.7f;
        [Range(0f, 0.3f)] public float pitchJitter = 0.05f;
        [Tooltip("있으면 재생마다 무작위로 고른다(발소리 등). clip 은 대표/미리보기")]
        public AudioClip[] variants;

        public AudioClip Pick()
        {
            if (variants != null && variants.Length > 0)
            {
                AudioClip v = variants[Random.Range(0, variants.Length)];
                if (v != null)
                {
                    return v;
                }
            }
            return clip;
        }
    }

    public List<Entry> entries = new List<Entry>();

    Dictionary<SfxId, Entry> map;

    public Entry Get(SfxId id)
    {
        if (map == null)
        {
            map = new Dictionary<SfxId, Entry>();
            foreach (Entry e in entries)
            {
                map[e.id] = e;
            }
        }
        map.TryGetValue(id, out Entry found);
        return found;
    }
}
