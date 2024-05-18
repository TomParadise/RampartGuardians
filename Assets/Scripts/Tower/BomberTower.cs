using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomberTower : AttackingTower
{
    private bool upgradedBombs = false;
    public override void UpgradeTower()
    {
        if (upgrades[level - 1].uniqueUpgrade != 0)
        {
            upgradedBombs = true;
        }

        base.UpgradeTower();
    }

    public override void ResetObject()
    {
        upgradedBombs = false;
        base.ResetObject();
    }

    public override void Shoot(Collider[] cols)
    {
        Enemy targetEnemy = GetTargetEnemy(cols);
        transform.rotation = Quaternion.LookRotation((targetEnemy.transform.position - transform.position).normalized);
        Projectile projectile = GameManager.instance.GetProjectile(projectileIndex).GetComponent<Projectile>();
        projectile.transform.position = projectileSpawnPoint.position;
        projectile.gameObject.SetActive(true);
        projectile.Init(damage, range, shotSpeed, piercingProjectiles, targetEnemy.transform, effectRange * (upgradedBombs ? 2 : 1f), this);
        animator.Play("Shoot");
    }
}
