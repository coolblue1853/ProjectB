using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사람 버릇 모델(<c>Resources/Bot/human_profile.json</c>, <c>Tools/playrec_fit.py</c> 산출물) 읽기·샘플링.
/// 표본이 모자란 칸(json null)은 <see cref="Dist.Valid"/> 가 false 이고, 쓰는 쪽이 넘긴 대체값을 쓴다.
/// 스키마는 PlayBotDesign.md 9절.
/// </summary>
[Serializable]
public sealed class BotProfile
{
    public const string ResourcePath = "Bot/human_profile";

    [Serializable]
    public sealed class Dist
    {
        public string key;
        public int n;
        public string unit;
        public float p10 = -1f;
        public float p25 = -1f;
        public float p50 = -1f;
        public float p75 = -1f;
        public float p90 = -1f;

        /// <summary>백분위가 다 있는가(표본 부족이면 json 이 null → -1)</summary>
        public bool Valid => p10 >= 0f && p25 >= p10 && p50 >= p25 && p75 >= p50 && p90 >= p75;
    }

    [Serializable]
    public sealed class Ratio
    {
        public string key;
        public int n;
        public int k;
        public float value = -1f;

        public bool Valid => value >= 0f;
    }

    [Serializable]
    public sealed class Rate
    {
        public string key;
        public int n;
        public float minutes;
        public float value = -1f;

        public bool Valid => value >= 0f;
    }

    public int version;
    public int minN;
    public float minutes;
    public int kills;
    public List<Dist> dists = new List<Dist>();
    public List<Ratio> ratios = new List<Ratio>();
    public List<Rate> rates = new List<Rate>();

    private Dictionary<string, Dist> _dist;
    private Dictionary<string, Ratio> _ratio;

    /// <summary>json 문자열 → 프로필. null 칸은 -1 로 읽는다(JsonUtility 가 null 숫자를 못 읽음)</summary>
    public static BotProfile Parse(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return new BotProfile();
        }
        BotProfile p = JsonUtility.FromJson<BotProfile>(json.Replace("null", "-1"));
        return p ?? new BotProfile();
    }

    /// <summary>Resources 에서 읽는다. 없으면 빈 프로필(모든 샘플이 대체값)</summary>
    public static BotProfile Load()
    {
        TextAsset ta = Resources.Load<TextAsset>(ResourcePath);
        return Parse(ta != null ? ta.text : null);
    }

    public Dist FindDist(string key)
    {
        if (_dist == null)
        {
            _dist = new Dictionary<string, Dist>(dists.Count);
            for (int i = 0; i < dists.Count; i++)
            {
                _dist[dists[i].key] = dists[i];
            }
        }
        Dist d;
        return _dist.TryGetValue(key, out d) ? d : null;
    }

    public Ratio FindRatio(string key)
    {
        if (_ratio == null)
        {
            _ratio = new Dictionary<string, Ratio>(ratios.Count);
            for (int i = 0; i < ratios.Count; i++)
            {
                _ratio[ratios[i].key] = ratios[i];
            }
        }
        Ratio r;
        return _ratio.TryGetValue(key, out r) ? r : null;
    }

    /// <summary>분포 key 에서 하나 뽑는다. 칸이 없거나 표본 부족이면 fallback</summary>
    public float Sample(string key, System.Random rng, float fallback)
    {
        Dist d = FindDist(key);
        if (d == null || !d.Valid)
        {
            return fallback;
        }
        return Quantile(d, (float)rng.NextDouble());
    }

    /// <summary>비율 key 의 값(0~1). 없거나 표본 부족이면 fallback</summary>
    public float RatioOr(string key, float fallback)
    {
        Ratio r = FindRatio(key);
        return r != null && r.Valid ? r.value : fallback;
    }

    /// <summary>
    /// 분위수 u(0~1) → 값. p10~p90 은 구간 선형 보간, 꼬리(0~0.1, 0.9~1)는 이웃 구간 기울기로 늘리되 0 아래로 내리지 않는다.
    /// </summary>
    public static float Quantile(Dist d, float u)
    {
        u = Mathf.Clamp01(u);
        float lo = Mathf.Max(0f, d.p10 - (d.p25 - d.p10) * (0.1f / 0.15f));
        float hi = d.p90 + (d.p90 - d.p75) * (0.1f / 0.15f);
        if (u < 0.1f)
        {
            return Mathf.Lerp(lo, d.p10, u / 0.1f);
        }
        if (u < 0.25f)
        {
            return Mathf.Lerp(d.p10, d.p25, (u - 0.1f) / 0.15f);
        }
        if (u < 0.5f)
        {
            return Mathf.Lerp(d.p25, d.p50, (u - 0.25f) / 0.25f);
        }
        if (u < 0.75f)
        {
            return Mathf.Lerp(d.p50, d.p75, (u - 0.5f) / 0.25f);
        }
        if (u < 0.9f)
        {
            return Mathf.Lerp(d.p75, d.p90, (u - 0.75f) / 0.15f);
        }
        return Mathf.Lerp(d.p90, hi, (u - 0.9f) / 0.1f);
    }
}
