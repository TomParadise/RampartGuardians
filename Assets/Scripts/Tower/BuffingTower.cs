using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffingTower : Tower
{
    [SerializeField] private SphereCollider sphereCol;
    [SerializeField] private BuffingAoE buffingAoE;

    public override void ResetObject()
    {
        base.ResetObject();
        if (radiusObject != null) { radiusObject.transform.localScale = new Vector3(radiusScale * range, 0.01f, radiusScale * range); }
        sphereCol.radius = range;
        buffingAoE.enabled = false;
    }

    public override void UpgradeTower()
    {
        base.UpgradeTower();
        if (radiusObject != null) { radiusObject.transform.localScale = new Vector3(radiusScale * range, 0.01f, radiusScale * range); }
        sphereCol.radius = range;
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
            this, towerName, level, "Grant +" + (100 * damage).ToString() + "% fire rate to towers in range", upgradeInfo, upgradeCost, Mathf.FloorToInt((float)goldValue * 0.8f));
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
            this, towerName, level, "Grant +" + (100 * damage).ToString() + "% fire rate to towers in range", upgradeInfo, upgradeCost, Mathf.FloorToInt((float)goldValue * 0.8f));
        }
    }

    public override void PlaceOnPositioner(TowerPositioner selectedPositioner, int cost)
    {
        base.PlaceOnPositioner(selectedPositioner, cost);
        GameManager.instance.buffingTowers.Add(this);
        buffingAoE.enabled = true;
    }

    public override void SellTower()
    {
        base.SellTower();
        GameManager.instance.buffingTowers.Remove(this);

        Collider[] cols = Physics.OverlapSphere(transform.position, range, 1 << 10);
        foreach(Collider col in cols)
        {
            TowerTriggerExit(col.GetComponent<Tower>());
        }
    }

    public bool CollisionCheck(Tower towerToCheck)
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, range, 1 << 10);
        foreach(Collider col in cols)
        {
            if(col.gameObject == towerToCheck.gameObject) { return true; }
        }
        return false;
    }

    public void TowerTriggerEnter(Tower tower)
    {
        if (tower.buffAmount < damage) { tower.buffAmount = damage; }
    }
    public void TowerTriggerExit(Tower towerToCheck)
    {
        List<BuffingTower> buffingTowers = GameManager.instance.buffingTowers;
        List<BuffingTower> collidingTowers = new List<BuffingTower>();

        foreach (BuffingTower tower in buffingTowers)
        {
            if (tower.CollisionCheck(towerToCheck))
            {
                collidingTowers.Add(tower);
            }
        }
        towerToCheck.buffAmount = 0;
        if (collidingTowers.Count > 0)
        {
            BuffingTower highestTower = collidingTowers[0];
            foreach (BuffingTower tower in collidingTowers)
            {
                if (tower.damage > highestTower.damage)
                {
                    highestTower = tower;
                }
            }
            towerToCheck.buffAmount = highestTower.damage;
        }
    }
}
