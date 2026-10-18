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

    public Color RarityColor(string rarity)
    {
        return rarity == "U" ? unique : rarity == "R" ? rare : common;
    }
}
