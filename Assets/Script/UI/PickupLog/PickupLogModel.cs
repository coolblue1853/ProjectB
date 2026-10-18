using System.Collections.Generic;

/// <summary>
/// 획득 로그 규칙 (InventoryRefactorDesign 3.3): 최대 5줄, 같은 아이템을 1.5초 안에 다시 얻으면 기존 줄 수량 증가·수명 재시작,
/// 일반 수명 3초 / 강조(R·U) 5초, 마지막 0.5초 페이드. 시간을 주입받는 순수 클래스(EditMode 테스트 대상).
/// </summary>
public class PickupLogModel
{
    public const int MaxLines = 5;
    public const float MergeWindow = 1.5f, Lifetime = 3f, EmphasisLifetime = 5f, FadeTime = 0.5f;

    public class Line
    {
        public int id;
        public string itemName;
        public string rarity;
        public int count;
        public bool emphasis;
        public float lastAdded;
        public float expireAt;
        public int version; // 뷰 갱신 판단용 (수량이 바뀔 때마다 증가)

        public float Alpha(float now)
        {
            float left = expireAt - now;
            return left >= FadeTime ? 1f : left <= 0f ? 0f : left / FadeTime;
        }
    }

    readonly List<Line> lines = new List<Line>();
    int nextId = 1;

    /// <summary>새 줄이 위(마지막)에 쌓인다. 0번이 가장 오래된 줄</summary>
    public IReadOnlyList<Line> Lines => lines;

    public Line Add(string itemName, int count, string rarity, float now)
    {
        bool emphasis = rarity == "R" || rarity == "U";
        foreach (Line l in lines)
        {
            if (l.itemName == itemName && now - l.lastAdded <= MergeWindow && now < l.expireAt)
            {
                l.count += count;
                l.lastAdded = now;
                l.expireAt = now + (l.emphasis ? EmphasisLifetime : Lifetime);
                l.version++;
                return l;
            }
        }
        if (lines.Count >= MaxLines)
        {
            lines.RemoveAt(0); // 가장 오래된 줄을 밀어냄
        }
        var line = new Line
        {
            id = nextId++,
            itemName = itemName,
            rarity = rarity,
            count = count,
            emphasis = emphasis,
            lastAdded = now,
            expireAt = now + (emphasis ? EmphasisLifetime : Lifetime),
        };
        lines.Add(line);
        return line;
    }

    /// <returns>만료로 지운 줄 수</returns>
    public int Tick(float now)
    {
        return lines.RemoveAll(l => now >= l.expireAt);
    }

    public void Clear()
    {
        lines.Clear();
    }
}
