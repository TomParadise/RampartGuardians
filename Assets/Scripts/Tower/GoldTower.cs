using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldTower : Tower
{
    private int pierceCount = 0;

    public override void Buff(float _buffAmount)
    {
        return;
    }

    public override void RemoveBuff()
    {
        return;
    }

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
        base.OnRoundEnd();
        if(piercingProjectiles)
        {
            if(++pierceCount == 3) 
            { 
                GameManager.instance.GivePlayerGold((int)(2 * damage));
                GameManager.instance.EarnGoldPopUp((int)(2 * damage), transform.position, 2.5f);
                pierceCount = 0;
                return;
            }
        }
        GameManager.instance.GivePlayerGold((int)damage);

        GameManager.instance.EarnGoldPopUp((int)damage, transform.position, 2.5f);
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
        AudioManager.instance.PlaySFX(infoPopUpSFX);
        string upgradeInfo = "Max level";
        int upgradeCost = 0;
        if (level - 1 < upgrades.Length)
        {
            upgradeInfo = upgrades[level - 1].upgradeText;
            upgradeCost = upgrades[level - 1].UpgradeCost;
        }
        string desc = "Earn +<color=#C58500>" + damage.ToString() + "</color> gold\nevery wave";
        if (piercingProjectiles)
        {
            int wavesUntilDouble = 3 - pierceCount;
            desc += "\n(Double in " + (wavesUntilDouble == 1 ? "1 wave" : wavesUntilDouble.ToString() + " waves") + ")";
        }
        levelUpPanel = GameManager.instance.GetUIManager().InitTowerUpgradeInfo(
            this, towerName, level, desc, upgradeInfo, upgradeCost, goldValue);
        if (levelUpPanel == null)
        {
            showingUpgradeInfo = false;
            radiusObject.gameObject.SetActive(false);
            hoverText.SetActive(false);
        }
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
            string desc = "Earn +<color=#C58500>" + damage.ToString() + "</color> gold\nevery wave";
            if (piercingProjectiles) 
            {
                int wavesUntilDouble = 3 - pierceCount;
                desc += "\n(Double in " + (wavesUntilDouble == 1 ? "1 wave" : wavesUntilDouble.ToString() + " waves") + ")";
            }
            levelUpPanel.Init(
            this, towerName, level, desc, upgradeInfo, upgradeCost, goldValue);
        }
    }

}
