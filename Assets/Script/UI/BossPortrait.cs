using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 보스 화면 HP바(보스 프리팹 안 Canvas/HealthBarCut)에 초상 프레임과 이름을 붙인다 (G10).
/// 데이터는 Resources/Boss/&lt;보스&gt;.asset(BossReward.portrait/displayName). 보스마다 한 번만 만든다.
/// </summary>
public static class BossPortrait
{
    public static void Ensure(EnemyHealth boss)
    {
        Transform bar = FindDeep(boss.transform, "HealthBarCut");
        if (bar == null || bar.Find("BossPortrait") != null)
        {
            return;
        }
        string bossName = boss.name.Replace("(Clone)", "").Trim();
        var reward = Resources.Load<BossReward>("Boss/" + bossName);
        if (reward == null || reward.portrait == null)
        {
            return;
        }
        var barRect = (RectTransform)bar;
        float h = barRect.sizeDelta.y;
        float size = h * 2.2f;

        // 초상 프레임: 어두운 테두리 + 금색 안쪽선 + 아이콘
        var frame = NewImage("BossPortrait", bar, new Color(0.32f, 0.28f, 0.36f, 0.95f)); // 어두운 몬스터 아이콘과 대비
        // 바 오른쪽 끝 (왼쪽은 플레이어 HP·스태미나 HUD와 겹친다)
        frame.rectTransform.anchorMin = frame.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        frame.rectTransform.pivot = new Vector2(0f, 0.5f);
        frame.rectTransform.anchoredPosition = new Vector2(10f, 0f);
        frame.rectTransform.sizeDelta = new Vector2(size, size);
        var outline = frame.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.78f, 0.3f, 1f);
        outline.effectDistance = new Vector2(3f, -3f);

        var icon = NewImage("Icon", frame.transform, Color.white);
        icon.sprite = reward.portrait;
        icon.preserveAspect = true;
        icon.rectTransform.anchorMin = Vector2.zero;
        icon.rectTransform.anchorMax = Vector2.one;
        icon.rectTransform.offsetMin = new Vector2(6f, 6f);
        icon.rectTransform.offsetMax = new Vector2(-6f, -6f);

        // 이름 (바 아래 왼쪽 — 바가 화면 맨 위라 위에 두면 잘린다)
        var nameGo = new GameObject("BossName", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameGo.transform.SetParent(bar, false);
        var text = nameGo.GetComponent<TextMeshProUGUI>();
        UiTheme theme = UiTheme.Get();
        if (theme.pixelFont != null)
        {
            text.font = theme.pixelFont;
        }
        text.text = Loc.T(string.IsNullOrEmpty(reward.displayName) ? bossName : reward.displayName);
        text.fontSize = h * 1.3f;
        text.color = new Color(1f, 0.85f, 0.5f);
        text.outlineWidth = 0.25f;
        text.outlineColor = new Color32(30, 10, 0, 255);
        text.raycastTarget = false;
        text.enableWordWrapping = false;
        var tr = text.rectTransform;
        tr.anchorMin = tr.anchorMax = new Vector2(0f, 0f);
        tr.pivot = new Vector2(0f, 1f);
        tr.anchoredPosition = new Vector2(4f, -4f);
        tr.sizeDelta = new Vector2(barRect.sizeDelta.x, h * 1.6f);
        GameLog.Info(GameLog.Cat.AI, "보스 초상 부착: " + text.text);
    }

    static Image NewImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == name)
            {
                return t;
            }
        }
        return null;
    }
}
