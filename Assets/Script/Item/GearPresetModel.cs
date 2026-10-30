using System.Collections.Generic;
using System.Text;

/// <summary>
/// 장비 프리셋(던파식 세트 스왑, 순수 모델, BasicFeatures.md 11절). 프리셋마다 부위 → (이름, 티어, 강화).
/// 세이브는 프리셋당 문자열 "부위=이름:티어:강화;…"(빈 프리셋은 "").
/// </summary>
public sealed class GearPresetModel
{
    public const int PresetCount = 3;
    public static readonly string[] Areas = { "Weapon", "Head", "Chest", "Hand", "Leg", "Shoes", "Ring", "Necklace" };

    public struct Entry
    {
        public string Name;
        public int Tier;
        public int Upgrade;
    }

    private readonly Dictionary<string, Entry>[] _presets = new Dictionary<string, Entry>[PresetCount];

    public GearPresetModel()
    {
        for (int i = 0; i < PresetCount; i++)
        {
            _presets[i] = new Dictionary<string, Entry>();
        }
    }

    public IReadOnlyDictionary<string, Entry> Get(int preset)
    {
        return preset >= 0 && preset < PresetCount ? _presets[preset] : null;
    }

    public bool IsEmpty(int preset)
    {
        return preset < 0 || preset >= PresetCount || _presets[preset].Count == 0;
    }

    /// <summary>preset 을 통째로 바꾼다(빈 이름은 무시)</summary>
    public void Set(int preset, IDictionary<string, Entry> entries)
    {
        if (preset < 0 || preset >= PresetCount)
        {
            return;
        }
        _presets[preset].Clear();
        if (entries == null)
        {
            return;
        }
        foreach (KeyValuePair<string, Entry> kv in entries)
        {
            if (!string.IsNullOrEmpty(kv.Key) && !string.IsNullOrEmpty(kv.Value.Name))
            {
                _presets[preset][kv.Key] = kv.Value;
            }
        }
    }

    public string[] ToSaveArray()
    {
        var a = new string[PresetCount];
        var sb = new StringBuilder();
        for (int i = 0; i < PresetCount; i++)
        {
            sb.Clear();
            foreach (KeyValuePair<string, Entry> kv in _presets[i])
            {
                if (sb.Length > 0)
                {
                    sb.Append(';');
                }
                sb.Append(kv.Key).Append('=').Append(kv.Value.Name).Append(':').Append(kv.Value.Tier).Append(':').Append(kv.Value.Upgrade);
            }
            a[i] = sb.ToString();
        }
        return a;
    }

    /// <summary>세이브 배열을 읽는다. 깨진 항목은 건너뛴다</summary>
    public void LoadFrom(string[] saved)
    {
        for (int i = 0; i < PresetCount; i++)
        {
            _presets[i].Clear();
            if (saved == null || i >= saved.Length || string.IsNullOrEmpty(saved[i]))
            {
                continue;
            }
            foreach (string part in saved[i].Split(';'))
            {
                int eq = part.IndexOf('=');
                if (eq <= 0)
                {
                    continue;
                }
                string[] f = part.Substring(eq + 1).Split(':');
                if (f.Length < 3 || f[0].Length == 0 || !int.TryParse(f[f.Length - 2], out int tier) || !int.TryParse(f[f.Length - 1], out int up))
                {
                    continue;
                }
                string name = string.Join(":", f, 0, f.Length - 2); // 이름 안의 콜론도 견딘다
                _presets[i][part.Substring(0, eq)] = new Entry { Name = name, Tier = tier, Upgrade = up };
            }
        }
    }
}
