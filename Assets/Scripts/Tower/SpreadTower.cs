using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadTower : AttackingTower
{
    public int projectileCount = 3;

    public override void Shoot(Collider[] cols)
    {
        Enemy targetEnemy = GetTargetEnemy(cols);
        transform.rotation = Quaternion.LookRotation((targetEnemy.transform.position - transform.position).normalized);
        float angle = 60 / (projectileCount - 1);
        int projectilesDivided = Mathf.FloorToInt(projectileCount / 2);
        for (int i = -projectilesDivided; i <= projectilesDivided; i++)
        {
            Projectile projectile = GameManager.instance.GetProjectile(projectileIndex).GetComponent<Projectile>();
            projectile.transform.position = projectileSpawnPoint.position;
            projectile.gameObject.SetActive(true);
            projectile.transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + angle * i, 0);
            projectile.Init(damage, range, shotSpeed, piercingProjectiles, null, effectRange, this);
        }
        animator.Play("Shoot");

        AudioManager.instance.PlaySFX(attackSFX);
    }

    public override void ResetObject()
    {
        base.ResetObject();
        projectileCount = baseProjectileCount;
    }

    public override void UpgradeTower()
    {
        if (level - 1 == upgrades.Length) { return; }
        projectileCount += (int)upgrades[level - 1].uniqueUpgrade;
        base.UpgradeTower();
    }

    public override void UpdateUpgradeInfo()
    {
        base.UpdateUpgradeInfo();
        if (levelUpPanel != null)
        {
            levelUpPanel.InitSpreadDamageInfo(this, projectileCount, damage, fireRate, range);
        }
    }

    public override void ForceSetNewUpgradeInfo(TowerLevelUpPanel towerLevelUpPanel)
    {
        base.ForceSetNewUpgradeInfo(towerLevelUpPanel);
        if (levelUpPanel != null)
        {
            levelUpPanel.InitSpreadDamageInfo(this, projectileCount, damage, fireRate, range);
        }
    }

    public override void InitUpgradeInfo()
    {
        base.InitUpgradeInfo();
        if (levelUpPanel != null)
        {
            levelUpPanel.InitSpreadDamageInfo(this, projectileCount, damage, fireRate, range);
        }
    }
}
