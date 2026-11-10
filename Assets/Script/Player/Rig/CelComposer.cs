using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 맨몸 셀 프레임 + 장비 레이어들 → 합성 스프라이트(CelEquipmentPlan 「확정」 던파식). 장비가 바뀔 때만 부른다 — 그리는 비용은 맨몸 셀과 같다.
/// 순서: 슬롯 낮은 것부터, 각 레이어의 지울 마스크로 지금까지의 그림에서 픽셀을 지운 뒤 덮어쓴다. 원본 텍스처는 읽기 가능(빌더가 켬)이어야 한다.
/// </summary>
public static class CelComposer
{
    public static Sprite[] Compose(CelClip clip, IList<CelEquipLayer> layers)
    {
        var result = new Sprite[clip.sprites.Length];
        for (int i = 0; i < clip.sprites.Length; i++)
        {
            result[i] = ComposeFrame(clip.name, i, clip.sprites[i], layers);
        }
        return result;
    }

    private static Sprite ComposeFrame(string clipName, int index, Sprite baseSprite, IList<CelEquipLayer> layers)
    {
        if (baseSprite == null)
        {
            return null;
        }
        Texture2D src = baseSprite.texture;
        Rect r = baseSprite.rect;
        int w = (int)r.width, h = (int)r.height;
        Color32[] px = src.GetPixels32();
        bool changed = false;
        Color32[] bodyPx = null;
        string bodySet = null;
        foreach (CelEquipLayer layer in layers)
        {
            CelEquipLayer.Frame bf = layer != null && layer.slot == CelEquipLayer.Slot.Body ? layer.Find(clipName, index) : null;
            if (bf != null && bf.layer != null)
            {
                bodyPx = bf.layer.GetPixels32();
                bodySet = SetOf(layer.id);
            }
        }
        foreach (CelEquipLayer layer in layers)
        {
            CelEquipLayer.Frame f = layer != null ? layer.Find(clipName, index) : null;
            if (f == null)
            {
                continue;
            }
            if (f.erase != null)
            {
                Color32[] m = f.erase.GetPixels32();
                for (int p = 0; p < px.Length && p < m.Length; p++)
                {
                    if (m[p].r > 127)
                    {
                        px[p] = new Color32(0, 0, 0, 0);
                    }
                }
            }
            if (f.layer != null)
            {
                Color32[] l = f.layer.GetPixels32();
                if (layer.slot == CelEquipLayer.Slot.Shoes && bodyPx != null && SetOf(layer.id) != bodySet)
                {
                    l = AlignBoots(l, bodyPx, w, h); // 다른 세트의 신발: 옷의 다리 끝에 맞춘다
                }
                for (int p = 0; p < px.Length && p < l.Length; p++)
                {
                    if (l[p].a > 0)
                    {
                        px[p] = l[p];
                    }
                }
            }
            changed = true;
        }
        if (!changed)
        {
            return baseSprite;
        }
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = baseSprite.name + "+equip" };
        tex.SetPixels32(px);
        tex.Apply(false, true);
        Vector2 pivot = new Vector2(baseSprite.pivot.x / w, baseSprite.pivot.y / h);
        Sprite s = Sprite.Create(tex, new Rect(0, 0, w, h), pivot, baseSprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);
        s.name = tex.name;
        return s;
    }

    private static string SetOf(string id)
    {
        int i = id != null ? id.LastIndexOf('_') : -1;
        return i > 0 ? id.Substring(0, i) : id;
    }

    /// <summary>
    /// 신발 조각을 옷 조각의 다리 끝(가장 아래 픽셀) 바로 아래로 옮긴다(11-10 이음새 기준점). 다리마다 따로(열 묶음), 틈이 있을 때만 위로 12칸·좌우 5칸까지. 옷에 다리가 없으면 그대로 둔다.
    /// 텍스처 좌표는 아래가 0이다.
    /// </summary>
    private static Color32[] AlignBoots(Color32[] shoe, Color32[] body, int w, int h)
    {
        var colHas = new bool[w];
        for (int p = 0; p < shoe.Length; p++)
        {
            if (shoe[p].a > 0)
            {
                colHas[p % w] = true;
            }
        }
        var result = (Color32[])shoe.Clone();
        int x = 0;
        var moves = new List<int[]>();
        while (x < w)
        {
            if (!colHas[x])
            {
                x++;
                continue;
            }
            int g0 = x;
            while (x + 1 < w && (colHas[x + 1] || (x + 2 < w && colHas[x + 2])))
            {
                x++;
            }
            int g1 = x;
            x++;
            int shoeTop = -1;
            for (int yy = h - 1; yy >= 0 && shoeTop < 0; yy--)
            {
                for (int xx = g0; xx <= g1; xx++)
                {
                    if (shoe[yy * w + xx].a > 0)
                    {
                        shoeTop = yy;
                        break;
                    }
                }
            }
            int c0 = Mathf.Max(0, g0 - 3), c1 = Mathf.Min(w - 1, g1 + 3);
            int bodyBottom = -1;
            for (int yy = 0; yy < h && bodyBottom < 0; yy++)
            {
                for (int xx = c0; xx <= c1; xx++)
                {
                    if (body[yy * w + xx].a > 0)
                    {
                        bodyBottom = yy;
                        break;
                    }
                }
            }
            if (shoeTop < 0 || bodyBottom < 0)
            {
                continue;
            }
            int dy = bodyBottom - 1 - shoeTop;
            float bx = 0f, sx = 0f;
            int bn = 0, sn = 0;
            for (int xx = c0; xx <= c1; xx++)
            {
                for (int yy = bodyBottom; yy <= bodyBottom + 1 && yy < h; yy++)
                {
                    if (body[yy * w + xx].a > 0)
                    {
                        bx += xx;
                        bn++;
                    }
                }
            }
            for (int xx = g0; xx <= g1; xx++)
            {
                for (int yy = Mathf.Max(0, shoeTop - 1); yy <= shoeTop; yy++)
                {
                    if (shoe[yy * w + xx].a > 0)
                    {
                        sx += xx;
                        sn++;
                    }
                }
            }
            int dx = bn > 0 && sn > 0 ? Mathf.RoundToInt(bx / bn - sx / sn) : 0;
            if (dy <= 0 || dy > 12 || Mathf.Abs(dx) > 5) // 겹쳐 있거나 맞닿은 신발(dy<=0)은 그대로 — 아래로 떠 있을 때만 위로 붙인다
            {
                continue;
            }
            moves.Add(new[] { g0, g1, dx, dy });
        }
        if (moves.Count == 0)
        {
            return shoe;
        }
        foreach (int[] m in moves)
        {
            for (int yy = 0; yy < h; yy++)
            {
                for (int xx = m[0]; xx <= m[1]; xx++)
                {
                    result[yy * w + xx] = new Color32(0, 0, 0, 0);
                }
            }
        }
        foreach (int[] m in moves)
        {
            for (int yy = 0; yy < h; yy++)
            {
                for (int xx = m[0]; xx <= m[1]; xx++)
                {
                    Color32 c = shoe[yy * w + xx];
                    int ny = yy + m[3], nx = xx + m[2];
                    if (c.a > 0 && ny >= 0 && ny < h && nx >= 0 && nx < w)
                    {
                        result[ny * w + nx] = c;
                    }
                }
            }
        }
        return result;
    }

    /// <summary>슬롯 순서로 정렬(낮은 슬롯이 아래)</summary>
    public static List<CelEquipLayer> Ordered(IEnumerable<CelEquipLayer> layers)
    {
        var list = new List<CelEquipLayer>();
        foreach (CelEquipLayer l in layers)
        {
            if (l != null)
            {
                list.Add(l);
            }
        }
        list.Sort((a, b) => ((int)a.slot).CompareTo((int)b.slot));
        return list;
    }
}
