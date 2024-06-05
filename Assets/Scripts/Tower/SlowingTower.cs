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
        if (level - 1 == upgrades.Length) { return; }
        if (upgrades[level - 1].uniqueUpgrade != 0)
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

        AudioManager.instance.PlaySFX(attackSFX);
    }
    public override void UpdateUpgradeInfo()
    {
        if (levelUpPanel != null)
        {
            string upgradeInfo = "Max level";
            int upgradeCost = 0;
            if (level - 1 < upgrades.Length)
            {
                upgradeInfo = upgrades[level - 1].upgradeText;
                upgradeCost = upgrades[level - 1].UpgradeCost;
            }
            levelUpPanel.InitSorceror(
                this, towerName, level, slowDownTime, fireRate, range, upgradeInfo, upgradeCost, goldValue, initTargeting, (int)targetType, kills);
        }
    }

    public override void ForceSetNewUpgradeInfo(TowerLevelUpPanel towerLevelUpPanel)
    {
        string upgradeInfo = "Max level";
        int upgradeCost = 0;
        if (level - 1 < upgrades.Length)
        {
            upgradeInfo = upgrades[level - 1].upgradeText;
            upgradeCost = upgrades[level - 1].UpgradeCost;
        }
        towerLevelUpPanel.InitSorceror(this, towerName, level, slowDownTime, fireRate, range, upgradeInfo, upgradeCost, goldValue, initTargeting, (int)targetType, kills);
    }

    public override void InitUpgradeInfo()
    {
        AudioManager.instance.PlaySFX(infoPopUpSFX);
        string upgradeInfo = "Max level";
        int upgradeCost = 0;
        if (level - 1 < upgrades.Length)
        {
            upgradeInfo = upgrades[level - 1].upgradeText;
            upgradeCost = upgrades[level - 1].UpgradeCost;
        }
        levelUpPanel = GameManager.instance.GetUIManager().InitSorcerorUpgradeInfo(
            this, towerName, level, slowDownTime, fireRate, range, upgradeInfo, upgradeCost, goldValue, initTargeting, (int)targetType, kills);
        if (levelUpPanel == null)
        {
            showingUpgradeInfo = false;
            radiusObject.gameObject.SetActive(false);
            hoverText.SetActive(false);
        }
    }
}
