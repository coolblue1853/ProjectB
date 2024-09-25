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
    public PlayerHealthManager healthManager;

    public void ResetHealthStat(PlayerHealthManager health)
    {
       healthManager = health;
        SetHealth(healthManager.GetHpNormalized());
        damagedBarImage.fillAmount = barImage.fillAmount;

       healthManager.OnHpDamaged += HpOnDamaged;
      healthManager.OnHpHealed += HpOnHealed;
    }
    public void ResetFullnessStat(PlayerHealthManager fullness)
    {
        healthManager = fullness;
        SetHealth(healthManager.GetFullnessNormalized());
        damagedBarImage.fillAmount = barImage.fillAmount;

        healthManager.OnFullnessDamaged += FullnessOnDamaged;
        healthManager.OnFullnessHealed += FullnessOnHealed;
    }

    public void ResetSteminaStat(PlayerHealthManager stemina)
    {
        healthManager = stemina;
        SetHealth(healthManager.GetSteminaNormalized());
        damagedBarImage.fillAmount = barImage.fillAmount;

        healthManager.OnSteminaDamaged += SteminaOnDamaged;
        healthManager.OnSteminaHealed += SteminaOnHealed;
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

    private void SteminaOnHealed(object sender, System.EventArgs e) {
        SetHealth(healthManager.GetSteminaNormalized());
        damagedBarImage.fillAmount = barImage.fillAmount;
    }

    private void SteminaOnDamaged(object sender, System.EventArgs e) {
        damagedHealthShrinkTimer = shrinkTime;
        SetHealth(healthManager.GetSteminaNormalized());
    }

    private void HpOnHealed(object sender, System.EventArgs e)
    {
        SetHealth(healthManager.GetHpNormalized());
        damagedBarImage.fillAmount = barImage.fillAmount;
    }

    private void HpOnDamaged(object sender, System.EventArgs e)
    {
        damagedHealthShrinkTimer = shrinkTime;
        SetHealth(healthManager.GetHpNormalized());
    }
    private void FullnessOnHealed(object sender, System.EventArgs e)
    {
        SetHealth(healthManager.GetFullnessNormalized());
        damagedBarImage.fillAmount = barImage.fillAmount;
    }

    private void FullnessOnDamaged(object sender, System.EventArgs e)
    {
        damagedHealthShrinkTimer = shrinkTime;
        SetHealth(healthManager.GetFullnessNormalized());
    }


    private void SetHealth(float healthNormalized) 
    {
        barImage.fillAmount = healthNormalized;
    }

}
