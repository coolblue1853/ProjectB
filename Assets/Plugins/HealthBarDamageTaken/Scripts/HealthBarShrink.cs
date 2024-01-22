﻿/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using CodeMonkey;

public class HealthBarShrink : MonoBehaviour {

    private const float DAMAGED_HEALTH_SHRINK_TIMER_MAX = .6f;

    public Image barImage;
    public Image damagedBarImage;


    private float damagedHealthShrinkTimer;

    [HideInInspector]
     public HealthSystem healthSystem;
    public HealthSystem steminaSystem;
    private void Awake() {

    }
    public void ResetEquipHp(int HP)
    {
        healthSystem = new HealthSystem(HP);
        SetHealth(healthSystem.GetHealthNormalized());
        damagedBarImage.fillAmount = barImage.fillAmount;
        healthSystem.OnDamaged += HealthSystem_OnDamaged;
        healthSystem.OnHealed += HealthSystem_OnHealed;
    }
    public void ResetHp(int HP)
    {
       healthSystem = new HealthSystem(HP);
        SetHealth(healthSystem.GetHealthNormalized());
        damagedBarImage.fillAmount = barImage.fillAmount;
        healthSystem.OnDamaged += HealthSystem_OnDamaged;
        healthSystem.OnHealed += HealthSystem_OnHealed;
    }
    public void ResetStemina(int Stemina)
    {
        steminaSystem = new HealthSystem(Stemina);
        SetHealth(steminaSystem.GetHealthNormalized());
        damagedBarImage.fillAmount = barImage.fillAmount;
        steminaSystem.OnDamaged += HealthSystem_OnDamaged;
        steminaSystem.OnHealed += HealthSystem_OnHealed;
    }

    private void Start() {
       // healthSystem = new HealthSystem(100);
     //   SetHealth(healthSystem.GetHealthNormalized());
      //  damagedBarImage.fillAmount = barImage.fillAmount;



       // healthSystem.OnDamaged += HealthSystem_OnDamaged;
        //healthSystem.OnHealed += HealthSystem_OnHealed;
    }

    private void Update() {
        damagedHealthShrinkTimer -= Time.deltaTime;
        if (damagedHealthShrinkTimer < 0) {
            if (barImage.fillAmount < damagedBarImage.fillAmount) {
                float shrinkSpeed = 1f;
                damagedBarImage.fillAmount -= shrinkSpeed * Time.deltaTime;
            }
        }
    }

    private void HealthSystem_OnHealed(object sender, System.EventArgs e) {
        SetHealth(healthSystem.GetHealthNormalized());
        damagedBarImage.fillAmount = barImage.fillAmount;
    }

    private void HealthSystem_OnDamaged(object sender, System.EventArgs e) {
        damagedHealthShrinkTimer = DAMAGED_HEALTH_SHRINK_TIMER_MAX;
        SetHealth(healthSystem.GetHealthNormalized());
    }

    private void SetHealth(float healthNormalized) {
        barImage.fillAmount = healthNormalized;
    }

}
