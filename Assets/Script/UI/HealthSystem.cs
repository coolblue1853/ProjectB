﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{

    public event EventHandler OnDamaged;
    public event EventHandler OnHealed;

    private int healthAmount;
    private int healthAmountMax;
    public HealthSystem(int healthAmount)
    {
        healthAmountMax = healthAmount;
        this.healthAmount = healthAmount; 
    }

    public void Damage(int amount)
    {
        healthAmount -= amount;
        if (healthAmount < 0)
        {
            healthAmount = 0;
        }
        if (OnDamaged != null) OnDamaged(this, EventArgs.Empty);
    }

    public void Heal(int amount)
    {
        healthAmount += amount;
        if (healthAmount > healthAmountMax)
        {
            healthAmount = healthAmountMax;
        }
        if (OnHealed != null) OnHealed(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return (float)healthAmount / healthAmountMax;
    }
    public void SetMaxHealth(int maxHealth)
    {
        healthAmountMax = maxHealth;
        if (healthAmount > healthAmountMax)
        {
            healthAmount = healthAmountMax;
        }
    }

}
    