using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthBarShrink : MonoBehaviour 
{

    private const float shrinkTime = .6f;

    public Image barImage;
    public Image damagedBarImage;

    private float damagedHealthShrinkTimer;

    [HideInInspector]
    public EnemyHealthSystem healthManager;

    public void ResetStat(int point)
    {
       healthManager = new EnemyHealthSystem(point);
        SetHealth(healthManager.GetHealthNormalized());
        damagedBarImage.fillAmount = barImage.fillAmount;
        healthManager.OnDamaged += HealthSystem_OnDamaged;
        healthManager.OnHealed += HealthSystem_OnHealed;
    }

    private void Update() 
    {
        damagedHealthShrinkTimer -= Time.deltaTime;
        if (damagedHealthShrinkTimer < 0) 
        {
            if (barImage.fillAmount < damagedBarImage.fillAmount) 
            {
                float shrinkSpeed = 1f;
                damagedBarImage.fillAmount -= shrinkSpeed * Time.deltaTime;
            }
        }
    }

    private void HealthSystem_OnHealed(object sender, System.EventArgs e) {
        SetHealth(healthManager.GetHealthNormalized());
        damagedBarImage.fillAmount = barImage.fillAmount;
    }

    private void HealthSystem_OnDamaged(object sender, System.EventArgs e) {
        damagedHealthShrinkTimer = shrinkTime;
        SetHealth(healthManager.GetHealthNormalized());
    }   

    private void SetHealth(float healthNormalized) 
    {
        barImage.fillAmount = healthNormalized;
    }

}
