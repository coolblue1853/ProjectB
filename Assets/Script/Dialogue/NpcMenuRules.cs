using System.Collections.Generic;

/// <summary>NPC 대화 선택지 규칙 (순수, REWORK-8). NPC 가 가진 기능만 순서대로 뽑고, 마지막은 항상 닫기.</summary>
public static class NpcMenuRules
{
    public enum Kind { Quest, Shop, Craft, Enhance, Trial, Talk, Close }

    public static List<Kind> Build(bool quest, bool shop, bool craft, bool enhance, bool trial, bool talk)
    {
        var list = new List<Kind>();
        if (quest) { list.Add(Kind.Quest); }
        if (shop) { list.Add(Kind.Shop); }
        if (craft) { list.Add(Kind.Craft); }
        if (enhance) { list.Add(Kind.Enhance); }
        if (trial) { list.Add(Kind.Trial); }
        if (talk) { list.Add(Kind.Talk); }
        list.Add(Kind.Close);
        return list;
    }

    /// <summary>닫기를 뺀 항목이 둘 이상일 때만 메뉴를 띄운다 (하나뿐이면 바로 실행)</summary>
    public static bool NeedsMenu(IList<Kind> kinds)
    {
        return kinds != null && kinds.Count > 2;
    }

    public static string Label(Kind kind)
    {
        switch (kind)
        {
            case Kind.Quest: return "퀘스트 이야기";
            case Kind.Shop: return "상점";
            case Kind.Craft: return "제작";
            case Kind.Enhance: return "강화";
            case Kind.Trial: return "무기 시험";
            case Kind.Talk: return "그냥 대화";
            default: return "닫기";
        }
    }

    /// <summary>커서를 delta 만큼 옮긴다 (순환)</summary>
    public static int Step(int cursor, int count, int delta)
    {
        return count <= 0 ? 0 : ((cursor + delta) % count + count) % count;
    }
}
