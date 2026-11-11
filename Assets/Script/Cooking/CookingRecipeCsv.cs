using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 요리 레시피 CSV 로더 (REWORK-11 후속). Resources/CookingRecipes.csv — 컬럼: id,method,rarity,ingredients.
/// id = 결과 요리 ItemSheet 이름, method = Grill|Boil, rarity = C|R|U, ingredients = "재료*개수;재료*개수".
/// 잘못된 줄은 건너뛰고 경고만 남긴다. 파일이 없거나 유효한 줄이 0 이면 호출자가 코드 기본값을 쓴다(null 반환).
/// </summary>
public static class CookingRecipeCsv
{
    public const string ResourceName = "CookingRecipes";

    public static CookRecipe[] Load()
    {
        TextAsset asset = Resources.Load<TextAsset>(ResourceName);
        return asset == null ? null : Parse(asset.text);
    }

    /// <summary>CSV 본문 → 레시피 배열. 유효한 줄이 없으면 null</summary>
    public static CookRecipe[] Parse(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        var list = new List<CookRecipe>();
        string[] lines = text.Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++) // 0 = 머리글
        {
            if (lines[i].Trim().Length == 0)
            {
                continue;
            }
            CookRecipe r = ParseLine(lines[i]);
            if (r == null)
            {
                GameLog.Warn(GameLog.Cat.Item, "CookingRecipes.csv " + (i + 1) + "번째 줄 무시: " + lines[i]);
                continue;
            }
            list.Add(r);
        }
        return list.Count > 0 ? list.ToArray() : null;
    }

    private static CookRecipe ParseLine(string line)
    {
        string[] c = line.Split(',');
        if (c.Length < 4 || c[0].Trim().Length == 0)
        {
            return null;
        }
        if (!Enum.TryParse(c[1].Trim(), true, out CookMethod method) || !Enum.TryParse(c[2].Trim(), true, out ItemRarity rarity))
        {
            return null;
        }
        var ings = new List<CookIngredient>();
        foreach (string part in c[3].Split(';'))
        {
            string[] kv = part.Split('*');
            if (kv.Length != 2 || kv[0].Trim().Length == 0 || !int.TryParse(kv[1].Trim(), out int n) || n < 1)
            {
                return null;
            }
            ings.Add(new CookIngredient(kv[0].Trim(), n));
        }
        return ings.Count == 0 ? null : new CookRecipe(c[0].Trim(), method, rarity, ings.ToArray());
    }
}
