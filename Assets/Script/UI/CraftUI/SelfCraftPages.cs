using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 자가 제작 UI 페이지 확장(Q-110·Q-436, BasicFeatures.md 10절): 씬의 자가 제작 상자는 1페이지 1줄 3칸이라 레시피를 더 못 넣었다.
/// Resources/SelfCraftRecipes.csv 의 추가 레시피로 1페이지를 복제한 페이지를 만들어 뒤에 붙인다(페이지 넘김 Q/E·LB/RB 는 CraftingManager 기존 기능).
/// 형식: 결과,재료*수;재료*수 (첫 줄 머리글). 시트에 없는 이름은 건너뛴다.
/// </summary>
public static class SelfCraftPages
{
    public const string ResourcePath = "SelfCraftRecipes";

    public struct Recipe
    {
        public string Result;
        public List<string> Ids;
        public List<int> Counts;
    }

    /// <summary>CSV 텍스트 → 레시피(순수, 테스트 대상). 깨진 줄·재료 없는 줄은 건너뛴다</summary>
    public static List<Recipe> Parse(string csv)
    {
        var list = new List<Recipe>();
        if (string.IsNullOrEmpty(csv))
        {
            return list;
        }
        string[] lines = csv.Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            int comma = line.IndexOf(',');
            if (line.Length == 0 || line.StartsWith("#") || comma <= 0)
            {
                continue;
            }
            var r = new Recipe { Result = line.Substring(0, comma).Trim(), Ids = new List<string>(), Counts = new List<int>() };
            foreach (string part in line.Substring(comma + 1).Split(';'))
            {
                int star = part.LastIndexOf('*');
                if (star <= 0 || !int.TryParse(part.Substring(star + 1).Trim(), out int n) || n <= 0)
                {
                    continue;
                }
                r.Ids.Add(part.Substring(0, star).Trim());
                r.Counts.Add(n);
            }
            if (r.Result.Length > 0 && r.Ids.Count > 0)
            {
                list.Add(r);
            }
        }
        return list;
    }

    /// <summary>결과·재료 이름이 시트에 없으면 경고 후 제외(제작 창이 없는 이름을 그리다 실패하지 않게)</summary>
    private static bool Invalid(Recipe r)
    {
        DatabaseManager db = DatabaseManager.instance;
        if (db == null)
        {
            return true;
        }
        bool bad = db.FindItemDataIndex(r.Result) < 0 || r.Ids.Exists(id => db.FindItemDataIndex(id) < 0);
        if (bad)
        {
            GameLog.Warn(GameLog.Cat.Item, "자가 제작 레시피 제외(시트에 없는 이름): " + r.Result);
        }
        return bad;
    }

    /// <summary>
    /// 씬 계층 가정: box/페이지/줄 0/칸(CraftItemCheck + NeedItem). 바뀌면 0페이지가 된다.
    /// box(페이지들의 부모)의 첫 페이지를 틀로 추가 페이지를 만든다. 만든 페이지 수</summary>
    public static int Build(Transform box)
    {
        if (box == null || box.childCount == 0)
        {
            return 0;
        }
        TextAsset csv = Resources.Load<TextAsset>(ResourcePath);
        List<Recipe> recipes = Parse(csv != null ? csv.text : null);
        recipes.RemoveAll(Invalid);
        Transform template = box.GetChild(0);
        if (recipes.Count == 0 || template.childCount == 0)
        {
            return 0;
        }
        int perPage = template.GetChild(0).childCount; // 1줄 칸 수(지금 3)
        int pages = 0;
        for (int start = 0; start < recipes.Count; start += perPage)
        {
            Transform page = Object.Instantiate(template.gameObject, box).transform;
            page.name = "Page" + (box.childCount);
            Transform line = page.GetChild(0);
            for (int l = page.childCount - 1; l >= 1; l--)
            {
                Remove(page.GetChild(l)); // 틀에 줄이 더 있으면 첫 줄만 쓴다
            }
            for (int c = line.childCount - 1; c >= 0; c--)
            {
                int index = start + c;
                Transform cell = line.GetChild(c);
                if (index >= recipes.Count)
                {
                    Remove(cell);
                    continue;
                }
                Fill(cell, recipes[index]);
            }
            page.gameObject.SetActive(false);
            pages++;
        }
        GameLog.Info(GameLog.Cat.Item, "자가 제작 추가 페이지 " + pages + " (레시피 " + recipes.Count + ")");
        return pages;
    }

    private static void Fill(Transform cell, Recipe r)
    {
        cell.name = r.Result;
        NeedItem need = cell.GetComponent<NeedItem>();
        if (need != null)
        {
            need.needItem = new List<string>(r.Ids);
            need.needItemAmount = new List<int>(r.Counts);
        }
        CraftItemCheck check = cell.GetComponent<CraftItemCheck>();
        if (check != null)
        {
            check.SetItem(r.Result); // 복제 때 Awake 는 틀 이름으로 돌았으니 새 이름으로 다시
        }
    }

    /// <summary>같은 프레임의 childCount 에 남지 않게 부모에서 떼고 파괴</summary>
    private static void Remove(Transform t)
    {
        t.SetParent(null, false);
        Object.Destroy(t.gameObject);
    }
}
