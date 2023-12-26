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

public class HealthBarCut : MonoBehaviour
{

    public float BAR_WIDTH = 0;
    public RectTransform  hpBarRect;
    public Image barImage;
    public Transform damagedBarTemplate;
    [HideInInspector]
    public HealthSystem healthSystem;

    private void Awake()
    {

        BAR_WIDTH = hpBarRect.sizeDelta.x;
    }
    public void setHpBar(int hp)
    {
        healthSystem = new HealthSystem(hp);
        SetHealth(healthSystem.GetHealthNormalized());

        healthSystem.OnDamaged += HealthSystem_OnDamaged;
        healthSystem.OnHealed += HealthSystem_OnHealed;
    }


    private void Update()
    {
    }

    private void HealthSystem_OnHealed(object sender, System.EventArgs e)
    {
        SetHealth(healthSystem.GetHealthNormalized());
    }

    private void HealthSystem_OnDamaged(object sender, System.EventArgs e)
    {
        float beforeDamagedBarFillAmount = barImage.fillAmount;
        SetHealth(healthSystem.GetHealthNormalized());
        Transform damagedBar = Instantiate(damagedBarTemplate, hpBarRect.transform);
        damagedBar.gameObject.SetActive(true);
        damagedBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(barImage.fillAmount * BAR_WIDTH, damagedBar.GetComponent<RectTransform>().anchoredPosition.y);
        damagedBar.GetComponent<Image>().fillAmount = beforeDamagedBarFillAmount - barImage.fillAmount;
        damagedBar.gameObject.AddComponent<HealthBarCutFallDown>();
    }

    private void SetHealth(float healthNormalized)
    {
        barImage.fillAmount = healthNormalized;
    }
}
