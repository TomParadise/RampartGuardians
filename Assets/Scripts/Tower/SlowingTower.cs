using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowingTower : AttackingTower
{
    private float slowDownTime = 0;

    public override void ResetObject()
    {
        slowDownTime = shotSpeed;
        base.ResetObject();
    }

    public override void UpgradeTower()
    {
        if(upgrades[level - 1].uniqueUpgrade != 0)
        {
            slowDownTime += upgrades[level - 1].uniqueUpgrade;
        }

        base.UpgradeTower();
    }

    public override void Shoot(Collider[] cols)
    {
        animator.Play("Shoot");
        foreach (Collider col in cols)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            enemy.TakeDamage(damage, this);
            enemy.SlowDown(slowDownTime);
        }
        GameObject effect = GameManager.instance.GetEffect(4);
        effect.transform.position = transform.position + Vector3.up * 0.5f;
        effect.SetActive(true);
    }
}
