using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Golem : Enemy
{
    private bool halfHP = false;
    public override void TakeDamage(float damageTaken, Tower attackingTower)
    {
        base.TakeDamage(damageTaken, attackingTower);
        if (halfHP) { return; }
        if (currentHP <= maxHP * 0.5f)
        {
            animator.Play("Run");
            speed = baseSpeed * 2f;
            halfHP = true;
        }
    }

    public override void SetBuffedSpeed(bool buffing)
    {
        if (buffing)
        {
            speed = baseSpeed * (slowed ? 0.75f : 1.25f) * (halfHP ? 2 : 1);
        }
        else
        {
            speed = baseSpeed * (slowed ? 0.6f : 1) * (halfHP ? 2 : 1);
        }
    }

    public override void SetSlowedSpeed(bool slowing)
    {
        if (slowing)
        {
            speed = baseSpeed * (buffed ? 0.75f : 0.6f) * (halfHP ? 2 : 1);
        }
        else
        {
            speed = baseSpeed * (buffed ? 1.25f : 1f) * (halfHP ? 2 : 1);
        }
    }
}
