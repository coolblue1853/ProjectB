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
        healthSystem = health;
        SetHealth(healthSystem.GetHealthNormalized());

        healthSystem.OnDamaged += OnDamaged;
        healthSystem.OnHealed += OnHealed;
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
