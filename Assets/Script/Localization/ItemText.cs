/// <summary>
/// 아이템 표시 문구 도우미(현지화). ItemSheet.csv 의 description 은 대부분 임시 값("equipment", "World", "eat!" 등)이라
/// 임시 값이면 종류·부위·세트로 설명을 만들고, 실제 설명이 있으면 번역한다.
/// </summary>
public static class ItemText
{
    static readonly System.Collections.Generic.HashSet<string> Placeholders = new System.Collections.Generic.HashSet<string>
    {
        "", "-", "String", "String2", "World", "eat!", "equipment", "ToxicPickAxe",
    };

    /// <summary>아이템 표시 이름(시트 itemNameT 번역본). DB 가 없거나 시트에 없으면 키를 번역</summary>
    public static string DisplayName(string id)
    {
        DatabaseManager db = DatabaseManager.instance;
        if (db == null)
        {
            return Loc.T(id);
        }
        ItemData d = db.LoadItemData(db.FindItemDataIndex(id));
        return d != null && !string.IsNullOrEmpty(d.itemNameT) ? d.itemNameT : Loc.T(id);
    }

    public static string Description(ItemData d)
    {
        if (d == null)
        {
            return "";
        }
        if (!Placeholders.Contains(d.description ?? ""))
        {
            return Loc.T(d.description);
        }
        switch (d.type)
        {
            case "Equip":
                string area = Loc.T(d.equipArea);
                return string.IsNullOrEmpty(d.setName) ? Loc.T("Equipment") + " · " + area : area + " · " + Loc.T(d.setName) + " " + Loc.T("set");
            case "Consum":
                return Loc.T("Consumable");
            default:
                return Loc.T("Crafting material");
        }
    }

    /// <summary>종류 표시: "장비 : 무기" / "재료"</summary>
    public static string TypeLine(ItemData d, bool withArea)
    {
        return withArea ? Loc.T(d.type) + " : " + Loc.T(d.equipArea) : Loc.T(d.type);
    }

    /// <summary>획득 경로: 의미 있는 값만(Buy·Craft·Boss·cook), 내부 코드 값은 숨긴다</summary>
    public static string AcqPath(ItemData d)
    {
        switch (d.acqPath)
        {
            case "Buy":
            case "Craft":
            case "cook":
            case "fishing":
            case "gathering":
            case "mining":
            case "alchemy":
            case "smelt":
                return Loc.T(d.acqPath);
            case "Boss":
                return Loc.T("BOSS");
            default:
                return "";
        }
    }
}
