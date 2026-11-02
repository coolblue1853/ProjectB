using TMPro;
using UnityEngine;

/// <summary>HUD 막대 숫자 문자열 (순수): 현재 / 최대, 최대가 0 이하면 빈 문자열</summary>
public static class HudBarText
{
    public static string Format(int now, int max)
    {
        if (max <= 0)
        {
            return "";
        }
        return Mathf.Clamp(now, 0, max) + " / " + max;
    }
}

/// <summary>
/// HP·스태미너 막대 위에 메이플식 "현재 / 최대" 숫자를 얹는다 (11-02). 씬의 막대(PlayerHealthManager.healthBar·steminaBar)를 건드리지 않고
/// 막대 위에 막대를 가득 채우는 글자 하나씩만 만들며, 값이 바뀐 프레임에만 문자열을 새로 만든다. 경험치 막대 숫자·퍼센트는 LevelHud 가 이미 보여 준다.
/// </summary>
public sealed class HudBarNumbers : MonoBehaviour
{
    private const float FontSize = 14f;

    private PlayerHealthManager _health;
    private TextMeshProUGUI _hpText, _staText;
    private int _hp = -1, _hpMax = -1, _sta = -1, _staMax = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (FindFirstObjectByType<HudBarNumbers>() != null)
        {
            return;
        }
        var go = new GameObject("[HudBarNumbers]");
        DontDestroyOnLoad(go);
        go.AddComponent<HudBarNumbers>();
    }

    private void Update()
    {
        if (_health == null)
        {
            _health = PlayerHealthManager.Instance;
            if (_health == null || _health.healthBar == null || _health.steminaBar == null)
            {
                _health = null;
                return;
            }
            _hpText = CreateLabel(_health.healthBar.transform);
            _staText = CreateLabel(_health.steminaBar.transform);
            _hp = _hpMax = _sta = _staMax = -1; // 새 라벨은 값을 다시 채운다
        }
        if (_hpText == null || _staText == null)
        {
            return;
        }
        if (_health.nowHp != _hp || _health.fullHP != _hpMax)
        {
            _hp = _health.nowHp;
            _hpMax = _health.fullHP;
            _hpText.text = HudBarText.Format(_hp, _hpMax);
        }
        if (_health.nowStemina != _sta || _health.fullStemina != _staMax)
        {
            _sta = _health.nowStemina;
            _staMax = _health.fullStemina;
            _staText.text = HudBarText.Format(_sta, _staMax);
        }
    }

    private static TextMeshProUGUI CreateLabel(Transform bar)
    {
        UiTheme theme = UiTheme.Get();
        var go = new GameObject("Number", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(bar, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (theme.pixelFont != null)
        {
            t.font = theme.pixelFont;
        }
        t.fontSize = FontSize;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = false;
        t.raycastTarget = false;
        t.outlineWidth = 0.3f;
        t.outlineColor = new Color32(0, 0, 0, 230);
        RectTransform r = t.rectTransform;
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
        return t;
    }
}
