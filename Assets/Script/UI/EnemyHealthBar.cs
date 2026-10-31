using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class EnemyHealthBar : MonoBehaviour
{
    public List<Transform> damagedBars = new List<Transform>();
    public float BAR_WIDTH = 0;
    public RectTransform  hpBarRect;
    public Image barImage;
    public Transform damagedBarTemplate;
    [HideInInspector]
    public EnemyHealth healthSystem;

    private void Awake()
    {
        BAR_WIDTH = hpBarRect.sizeDelta.x;
    }
    public void setHpBar(EnemyHealth health)
    {
        // 풀 재사용 시 OnEnable마다 호출되므로 이전 구독을 먼저 해제 (중복 구독 → 데미지바 여러 개, B-109)
        if (healthSystem != null)
        {
            healthSystem.OnDamaged -= OnDamaged;
            healthSystem.OnHealed -= OnHealed;
        }
        healthSystem = health;
        SetHealth(healthSystem.GetHealthNormalized());

        healthSystem.OnDamaged += OnDamaged;
        healthSystem.OnHealed += OnHealed;
        RefreshLevelLabel();
    }

    private TMPro.TextMeshProUGUI _lvLabel;

    /// <summary>플레이어 레벨 대비 몹 레벨 색: +4 이상 붉게, -4 이하 회색, 나머지 흰색 (메이플식)</summary>
    public static Color LevelColor(int monsterLevel, int playerLevel)
    {
        int gap = monsterLevel - playerLevel;
        if (gap >= DamageCalculator.LevelGapStart)
        {
            return new Color(1f, 0.32f, 0.28f);
        }
        return gap <= -DamageCalculator.LevelGapStart ? new Color(0.65f, 0.65f, 0.65f) : Color.white;
    }

    /// <summary>HP 바 위쪽에 "Lv.N" 을 보인다. 레벨 미지정 몹·보스는 숨긴다. 라벨은 처음 필요할 때 한 번 만든다</summary>
    private void RefreshLevelLabel()
    {
        bool show = healthSystem != null && healthSystem.Level > 0 && !healthSystem.isBossMob;
        if (!show)
        {
            if (_lvLabel != null)
            {
                _lvLabel.gameObject.SetActive(false);
            }
            return;
        }
        if (_lvLabel == null)
        {
            var go = new GameObject("LvLabel", typeof(RectTransform));
            go.transform.SetParent(hpBarRect.parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = hpBarRect.anchorMin;
            rt.anchorMax = hpBarRect.anchorMax;
            rt.pivot = new Vector2(hpBarRect.pivot.x, 0f);
            rt.anchoredPosition = hpBarRect.anchoredPosition + new Vector2(0f, hpBarRect.sizeDelta.y * (1f - hpBarRect.pivot.y) + 2f);
            rt.sizeDelta = new Vector2(Mathf.Max(60f, hpBarRect.sizeDelta.x), 18f);
            _lvLabel = go.AddComponent<TMPro.TextMeshProUGUI>();
            _lvLabel.fontSize = 14f;
            _lvLabel.alignment = TMPro.TextAlignmentOptions.BottomLeft;
            _lvLabel.raycastTarget = false;
            _lvLabel.enableWordWrapping = false;
        }
        _lvLabel.gameObject.SetActive(true);
        _lvLabel.text = "Lv." + healthSystem.Level;
        _lvLabel.color = LevelColor(healthSystem.Level, LevelSystem.Level);
    }
    private void OnHealed(object sender, System.EventArgs e)
    {
        SetHealth(healthSystem.GetHealthNormalized());
    }

    private void OnDamaged(object sender, System.EventArgs e)
    {
        float beforeDamagedBarFillAmount = barImage.fillAmount;
        SetHealth(healthSystem.GetHealthNormalized());

        Transform damagedBar = Instantiate(damagedBarTemplate, hpBarRect.transform);
        damagedBar.gameObject.SetActive(true);
        damagedBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(barImage.fillAmount * BAR_WIDTH,
        damagedBar.GetComponent<RectTransform>().anchoredPosition.y);
        damagedBar.GetComponent<Image>().fillAmount = beforeDamagedBarFillAmount - barImage.fillAmount;
        damagedBar.gameObject.AddComponent<HealthBarCutFallDown>();

        // damagedBar를 리스트에 추가
        damagedBars.Add(damagedBar);
    }

    private void SetHealth(float healthNormalized)
    {
        barImage.fillAmount = healthNormalized;
    }
}
