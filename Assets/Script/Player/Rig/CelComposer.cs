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
