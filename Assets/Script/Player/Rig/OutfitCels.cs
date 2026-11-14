using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 의상 한 벌 통짜 셀 묶음(11-15, outfit_one_piece_decision "진행"). 장착 의상 → 셀 묶음 id → 같은 이름 그림으로 클립 교체, 없는 클립은 코트 셀 그대로(폴백).
/// 모션 소유 원칙: 의상이 기본 동작(대기·걷기·달리기·점프·사망·로프)을, 무기가 공격 동작을 정한다. 교체는 그림만 — 타이밍·타격 프레임·손·각도·주먹은 코트 클립 값을 그대로 쓴다.
/// </summary>
[Serializable]
public class OutfitCelSet
{
    public string id;
    /// <summary>그림 이름(스프라이트 이름) = 코트 셀 PNG 이름(ds_idle_0, cb_hslash_2 …). PixelRigBuilder 가 CelDS/Outfits/&lt;id&gt;/ 로 채운다</summary>
    public Sprite[] sprites = new Sprite[0];
}

public static class OutfitCels
{
    /// <summary>의상 아이템 이름(ItemSheet name, 장비 칸 5) → 셀 묶음 id. 표에 없으면 코트 셀</summary>
    private static readonly Dictionary<string, string> ByItem = new Dictionary<string, string>
    {
        { "ScareArmor", "scarecrow" },
        { "LeafArmor", "leaf" },
        { "WispArmor", "wisp" },
        { "WolfArmor", "wolf" },
        { "RatArmor", "rat" }, // 기본 4종만(공격은 코트 폴백)
    };

    /// <summary>장착 의상 아이템 이름 → 셀 묶음 id(없거나 빈 칸이면 null = 코트)</summary>
    public static string IdFor(string itemName)
    {
        return !string.IsNullOrEmpty(itemName) && ByItem.TryGetValue(itemName, out string id) ? id : null;
    }

    /// <summary>묶음의 그림을 이름 → 스프라이트로</summary>
    public static Dictionary<string, Sprite> Index(OutfitCelSet set)
    {
        var map = new Dictionary<string, Sprite>();
        if (set == null || set.sprites == null)
        {
            return map;
        }
        foreach (Sprite s in set.sprites)
        {
            if (s != null)
            {
                map[s.name] = s;
            }
        }
        return map;
    }

    /// <summary>
    /// 코트 클립의 프레임 그림이 묶음에 전부 있으면 그림만 바꾼 복사본(outfit = true, 부위 레이어 합성 안 함), 하나라도 없으면 null(코트 폴백).
    /// 프레임 일부만 바꾸면 코트·의상이 섞여 깜박이므로 클립 단위로 고른다.
    /// </summary>
    public static CelClip Swap(CelClip coat, IReadOnlyDictionary<string, Sprite> outfit)
    {
        if (coat == null || outfit == null || outfit.Count == 0 || coat.sprites.Length == 0)
        {
            return null;
        }
        var sprites = new Sprite[coat.sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            Sprite s = coat.sprites[i];
            if (s == null || !outfit.TryGetValue(s.name, out sprites[i]))
            {
                return null;
            }
        }
        return coat.CloneWith(sprites); // 손·각도·주먹(_fist 레이어 없음 → 코트 것)·타이밍은 코트 값 그대로
    }
}
