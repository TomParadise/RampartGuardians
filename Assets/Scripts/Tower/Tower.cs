using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : PooledObject
{
    [System.Serializable]
    public struct UpgradeInfo
    {
        public string upgradeText;
        public float damageUpgrade;
        public float fireRateUpgrade;
        public float rangeUpgrade;
        public bool piercing;
        public int UpgradeCost;
        public float uniqueUpgrade;
        public bool switchWeaponModel;
    }

    [SerializeField] public UpgradeInfo[] upgrades;

    public float startDamage = 1;
    public float startFireRate = 1;
    public float startRange = 5;
    public float damage = 1;
    public float fireRate = 1;
    public float shotSpeed = 1;
    public float range = 5;
    public int level = 1;
    public bool basePiercingProjectiles = false;
    public bool piercingProjectiles = false;
    public bool placed = false;
    [SerializeField] public MeshRenderer radiusObject;
    [SerializeField] private Material[] radiusMats;
    [SerializeField] private GameObject hoverText;
    private TowerPositioner towerPositioner;

    private bool showingUpgradeInfo = false;
    public int goldValue = 300;

    public float buffAmount = 0;

    public Animator animator;

    public bool utilityTile = false;

    [SerializeField] private bool targetingTower = true;
    [SerializeField] private FaceTowardsCamera levelIcon;
    public TargetType targetType;
    public int kills = 0;
    public float radiusScale = 4;

    public string towerName;
    public string damageType;
    [TextArea] public string descriptionInfo;
    [SerializeField] private GameObject[] weaponModels;
    public int towerCost = 200;
    public int baseProjectileCount = 3;

    public TowerLevelUpPanel levelUpPanel;

    public int initTargeting = 1;

    public virtual void OnRoundEnd() { }

    public enum TargetType
    {
        First,
        Last,
        HighestHP,
        LowestHP,
        Auto
    }

    public void GiveKill()
    {
        kills++;
        if(levelUpPanel != null)
        {
            levelUpPanel.UpdateKillsText(kills);
        }
    }

    public virtual void UpgradeTower()
    {
        if(level - 1 == upgrades.Length) { return; }

        goldValue += upgrades[level - 1].UpgradeCost;
        damage += upgrades[level - 1].damageUpgrade;
        fireRate += upgrades[level - 1].fireRateUpgrade;
        range += upgrades[level - 1].rangeUpgrade;
        if (radiusObject != null) { radiusObject.transform.localScale = new Vector3(radiusScale * range, 0.01f, radiusScale * range); }
        if (upgrades[level - 1].piercing) { piercingProjectiles = true; }
        if (upgrades[level - 1].switchWeaponModel) 
        { 
            weaponModels[0].SetActive(false); 
            weaponModels[1].SetActive(true); 
        }

        level++;
        levelIcon.SetSprite(level - 2);
        UpdateUpgradeInfo();
    }

    public virtual void UpdateUpgradeInfo()
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
                this, towerName, level, damage, fireRate, range, upgradeInfo, upgradeCost, Mathf.FloorToInt((float)goldValue * 0.9f), initTargeting, (int)targetType, kills);
        }
    }

    public void ForceSetNewUpgradeInfo(TowerLevelUpPanel towerLevelUpPanel)
    {
        string upgradeInfo = "Max level";
        int upgradeCost = 0;
        if (level - 1 < upgrades.Length)
        {
            upgradeInfo = upgrades[level - 1].upgradeText;
            upgradeCost = upgrades[level - 1].UpgradeCost;
        }
        towerLevelUpPanel.Init(this, towerName, level, damage, fireRate, range, upgradeInfo, upgradeCost, Mathf.FloorToInt((float)goldValue * 0.8f), initTargeting, (int)targetType, kills);
    }

    public virtual void InitUpgradeInfo()
    {
        string upgradeInfo = "Max level";
        int upgradeCost = 0;
        if(level - 1 < upgrades.Length) 
        {
            upgradeInfo = upgrades[level - 1].upgradeText;
            upgradeCost = upgrades[level - 1].UpgradeCost;
        }
        levelUpPanel = GameManager.instance.GetUIManager().InitTowerUpgradeInfo(
            this, towerName, level, damage, fireRate, range, upgradeInfo, upgradeCost, Mathf.FloorToInt((float)goldValue * 0.9f), initTargeting, (int)targetType, kills);
    }

    public virtual void SellTower()
    {
        GameManager.instance.GivePlayerGold(Mathf.FloorToInt((float)goldValue * 0.9f));
        towerPositioner.OnTowerSold();
        GameManager.instance.towers.Remove(this);
        Release();
    }

    public virtual void PlaceOnPositioner(TowerPositioner selectedPositioner, int cost)
    {
        goldValue = cost;
        towerPositioner = selectedPositioner;
        placed = true;
        GameManager.instance.towers.Add(this);
        if (!GetComponent<Collider>().bounds.Contains(Camera.main.ScreenToWorldPoint(Input.mousePosition))) { OnMouseExit(); }
        transform.rotation = Quaternion.identity;
    }

    public override void ResetObject()
    {
        if(weaponModels.Length == 2)
        {
            weaponModels[0].SetActive(true);
            weaponModels[1].SetActive(false);
        }
        damage = startDamage;
        fireRate = startFireRate;
        range = startRange;
        level = 1;
        placed = false;
        kills = 0;
        targetType = TargetType.First;
        if (radiusObject != null) 
        { 
            radiusObject.gameObject.SetActive(true);
            radiusObject.transform.localScale = new Vector3(radiusScale * range, 0.01f, radiusScale * range);
            radiusObject.material = radiusMats[0];
        }
        hoverText.SetActive(false);
        showingUpgradeInfo = false;
        levelIcon.SetSprite(-1);
        buffAmount = 0;
        piercingProjectiles = basePiercingProjectiles;
        base.ResetObject();
    }

    public void CheckForPositioners(Vector3 pos)
    {
        Collider[] cols = Physics.OverlapSphere(pos, 0.1f, 1 << 6);
        Collider closestCol = null;
        float closestDist = 0;
        foreach (Collider col in cols)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (closestCol == null || dist < closestDist)
            {
                closestCol = col;
                closestDist = dist;
            }
        }
        if (closestCol != null)
        {
            transform.parent.GetComponent<TowerButton>().positioner = closestCol.GetComponent<TowerPositioner>();
            transform.position = closestCol.transform.position;
            if (animator != null) { animator.Play("Idle"); }
            radiusObject.material = radiusMats[0];
            transform.rotation = Quaternion.identity;
        }
        else
        {
            pos.y = 0.5f;
            transform.position = pos;
            transform.parent.GetComponent<TowerButton>().positioner = null;
            if (animator != null) { animator.Play("Float"); }
            radiusObject.material = radiusMats[1];
        }
    }

    private void OnMouseUp()
    {
        if (showingUpgradeInfo) { return; }
        if (placed)
        {
            showingUpgradeInfo = true;
            hoverText.SetActive(false);
            InitUpgradeInfo();
        }
    }

    private void OnMouseOver()
    {
        if (showingUpgradeInfo) { return; }
        if (placed)
        {
            radiusObject.gameObject.SetActive(true);
            hoverText.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        if (showingUpgradeInfo) { return; }
        if (placed)
        {
            radiusObject.gameObject.SetActive(false);
            hoverText.SetActive(false);
        }
    }

    public void HideUpgradeInfo()
    {
        radiusObject.gameObject.SetActive(false);
        hoverText.SetActive(false);
        showingUpgradeInfo = false;
    }    
}
