using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldTower : Tower
{
    private int pierceCount = 0;

    public override void PlaceOnPositioner(TowerPositioner selectedPositioner, int cost)
    {
        base.PlaceOnPositioner(selectedPositioner, cost);
    }

    public override void SellTower()
    {
        base.SellTower();
    }

    public override void OnRoundEnd()
    {
        if(piercingProjectiles)
        {
            if(++pierceCount == 3) 
            { 
                GameManager.instance.GivePlayerGold((int)(2 * damage));
                GameManager.instance.EarnGoldPopUp((int)(2 * damage), transform.position);
                return;
            }
        }
        GameManager.instance.GivePlayerGold((int)damage);

        GameManager.instance.EarnGoldPopUp((int)damage, transform.position);
    }

    public override void ResetObject()
    {
        base.ResetObject();
        if (radiusObject != null) { radiusObject.transform.localScale = new Vector3(1.75f, 0.01f, 1.6f); }
        pierceCount = 0;
    }

    public override void UpgradeTower()
    {
        base.UpgradeTower();

        if (radiusObject != null) { radiusObject.transform.localScale = new Vector3(1.75f, 0.01f, 1.6f); }
    }
    public override void InitUpgradeInfo()
    {
        string upgradeInfo = "Max level";
        int upgradeCost = 0;
        if (level - 1 < upgrades.Length)
        {
            upgradeInfo = upgrades[level - 1].upgradeText;
            upgradeCost = upgrades[level - 1].UpgradeCost;
        }
        levelUpPanel = GameManager.instance.GetUIManager().InitTowerUpgradeInfo(
            this, towerName, level, "Earn +" + damage.ToString() + " gold after every wave", upgradeInfo, upgradeCost, Mathf.FloorToInt((float)goldValue * 0.8f));
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
            levelUpPanel.Init(
            this, towerName, level, "Earn +" + damage.ToString() + " gold after every wave", upgradeInfo, upgradeCost, Mathf.FloorToInt((float)goldValue * 0.8f));
        }
    }

}
