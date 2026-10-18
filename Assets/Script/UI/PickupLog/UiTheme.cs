using TMPro;
using UnityEngine;

/// <summary>코드로 만드는 UI(획득 로그 등)가 공유하는 폰트·등급색 (Resources/UI/UiTheme.asset)</summary>
[CreateAssetMenu(menuName = "ProjectB/UI Theme")]
public class UiTheme : ScriptableObject
{
    public TMP_FontAsset pixelFont;
    public Color common = Color.white;
    public Color rare = new Color(0.35f, 0.66f, 1f);
    public Color unique = new Color(1f, 0.78f, 0.25f);
    public Color warning = new Color(1f, 0.35f, 0.3f);

    [Header("Pixel UI frames (Art/UI, 9-slice)")]
    public Sprite frame;       // 어두운 패널 + 금테
    public Sprite frameLight;  // 밝은 목재 (선택 행·강조)
    public Sprite divider;

    static UiTheme cached;

    public static UiTheme Get()
    {
        if (cached == null)
        {
            cached = Resources.Load<UiTheme>("UI/UiTheme");
            if (cached == null)
            {
                cached = CreateInstance<UiTheme>(); // 에셋이 없어도 기본값으로 동작
            }
        }
        return cached;
    }

    /// <summary>9-슬라이스 프레임 패널 (코드 생성 UI 공용). 프레임이 없으면 단색</summary>
    public UnityEngine.UI.Image NewPanel(string name, Transform parent, bool light = false, float alpha = 1f)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<UnityEngine.UI.Image>();
        Sprite sp = light ? frameLight : frame;
        if (sp != null)
        {
            img.sprite = sp;
            img.type = UnityEngine.UI.Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 0.5f; // 24px 프레임을 48px 두께로 (1080p 기준)
            img.color = new Color(1f, 1f, 1f, alpha);
        }
        else
        {
            img.color = light ? new Color(0.3f, 0.24f, 0.16f, alpha) : new Color(0.08f, 0.07f, 0.11f, alpha);
        }
        img.raycastTarget = false;
        return img;
    }

    public Color RarityColor(string rarity)
    {
        return rarity == "U" ? unique : rarity == "R" ? rare : common;
    }
}
