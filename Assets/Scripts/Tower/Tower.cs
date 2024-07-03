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
    [SerializeField] public GameObject hoverText;
    private TowerPositioner towerPositioner;

    public bool showingUpgradeInfo = false;
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

    [SerializeField] private AudioClip placeTowerSFX;
    [SerializeField] private AudioClip snapTowerSFX;
    [SerializeField] public AudioClip infoPopUpSFX;
    private PooledObject buffParticles = null;

    public int roundsActive = -1;
    public TowerButton towerButton;

    public void TogglePause(bool paused) 
    {
        if (animator != null)
        {
            animator.speed = paused ? 0 : 1;
        }
    }

    public virtual void OnRoundEnd() 
    {
        roundsActive++;
        if (levelUpPanel != null)
        {
            levelUpPanel.UpdateSellCost(goldValue, roundsActive);
        }
    }
    public virtual void OnRoundStart() 
    {
        if(roundsActive == -1) 
        {
            roundsActive = 0;

            GameManager.instance.TowerUpgraded(level - 1);
            GameManager.instance.AddTower();
        }
    }

    public enum TargetType
    {
        First,
        Last,
        HighestHP,
        LowestHP,
        Auto
    }

    public virtual void Buff(float _buffAmount)
    {
        if (!placed) { return; }
        buffAmount = _buffAmount;
        if(buffParticles == null)
        {
            buffParticles = GameManager.instance.GetEffect(6).GetComponent<PooledObject>();
            buffParticles.transform.SetParent(transform.GetChild(transform.childCount - 1));
            buffParticles.transform.localPosition = Vector3.zero;
            buffParticles.gameObject.SetActive(true);
        }
    }

    public virtual void RemoveBuff()
    {
        buffAmount = 0;
        if (buffParticles != null)
        {
            buffParticles.transform.SetParent(GameManager.instance.GetEffectHolder());
            buffParticles.Release();
            buffParticles = null;
        }
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

        if(roundsActive > -1) { GameManager.instance.TowerUpgraded(1); }

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
        AudioManager.instance.PlayButtonUpgradeSFX();

        if(animator != null) { animator.Play("Upgrade"); }
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
                this, towerName, level, damage, fireRate, range, upgradeInfo, upgradeCost, goldValue, initTargeting, (int)targetType, kills);
        }
    }

    public virtual void ForceSetNewUpgradeInfo(TowerLevelUpPanel towerLevelUpPanel)
    {
        string upgradeInfo = "Max level";
        int upgradeCost = 0;
        if (level - 1 < upgrades.Length)
        {
            upgradeInfo = upgrades[level - 1].upgradeText;
            upgradeCost = upgrades[level - 1].UpgradeCost;
        }
        towerLevelUpPanel.Init(this, towerName, level, damage, fireRate, range, upgradeInfo, upgradeCost, goldValue, initTargeting, (int)targetType, kills);
    }

    public virtual void InitUpgradeInfo()
    {
        AudioManager.instance.PlaySFX(infoPopUpSFX);
        string upgradeInfo = "Max level";
        int upgradeCost = 0;
        if(level - 1 < upgrades.Length) 
        {
            upgradeInfo = upgrades[level - 1].upgradeText;
            upgradeCost = upgrades[level - 1].UpgradeCost;
        }
        levelUpPanel = GameManager.instance.GetUIManager().InitTowerUpgradeInfo(
            this, towerName, level, damage, fireRate, range, upgradeInfo, upgradeCost, goldValue, initTargeting, (int)targetType, kills);
        if(levelUpPanel == null)
        {
            showingUpgradeInfo = false;
            radiusObject.gameObject.SetActive(false);
            hoverText.SetActive(false);
        }
    }

    public virtual void SellTower()
    {
        float value = (float)goldValue * (0.70f + (Mathf.Min(6, roundsActive) * 0.05f));
        if(roundsActive == -1) { value = goldValue; }
        GameManager.instance.GivePlayerGold(Mathf.FloorToInt(value));
        towerPositioner.OnTowerSold();
        GameManager.instance.towers.Remove(this);
        Release();
    }

    public override void Release()
    {
        if(buffParticles != null)
        {
            buffParticles.transform.SetParent(GameManager.instance.GetEffectHolder());
            buffParticles.Release();
            buffParticles = null;
        }
        base.Release();
    }

    public virtual void PlaceOnPositioner(TowerPositioner selectedPositioner, int cost)
    {
        goldValue = cost;
        towerPositioner = selectedPositioner;
        placed = true;
        GameManager.instance.towers.Add(this);
        if (!GetComponent<Collider>().bounds.Contains(Camera.main.ScreenToWorldPoint(Input.mousePosition))) { OnMouseExit(); }
        //transform.rotation = Quaternion.identity;
        float rot = Mathf.Round(transform.eulerAngles.y / 90f) * 90;
        transform.rotation = Quaternion.AngleAxis(rot, Vector3.up);

        AudioManager.instance.PlaySFX(placeTowerSFX);
        if (GameManager.instance.GetIsWaveStarted()) 
        {
            roundsActive = 0;
            GameManager.instance.AddTower();
        }
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
        roundsActive = -1;
        towerButton = null;
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
            if (towerButton.positioner == closestCol.GetComponent<TowerPositioner>())
            {
                transform.position = closestCol.transform.position;
                return;
            }
            AudioManager.instance.PlaySFX(snapTowerSFX);
            towerButton.positioner = closestCol.GetComponent<TowerPositioner>();
            transform.position = closestCol.transform.position;
            if (animator != null) { animator.Play("Idle"); }
            radiusObject.material = radiusMats[0];
            transform.rotation = Quaternion.identity;
        }
        else
        {
            pos.y = 0.5f;
            transform.position = pos;
            towerButton.positioner = null;
            if (animator != null) { animator.Play("Float"); }
            radiusObject.material = radiusMats[1];
        }
    }

    private void OnMouseUp()
    {
        MouseUp();
    }

    public virtual void MouseUp()
    {
        if (GameManager.instance.gameState == GameManager.GameState.Paused) { return; }
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
        MouseOver();
    }

    public virtual void MouseOver()
    {
        if (GameManager.instance.gameState == GameManager.GameState.Paused) { return; }
        if (showingUpgradeInfo) { return; }
        if (placed)
        {
            radiusObject.gameObject.SetActive(true);
            hoverText.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        MouseExit();
    }

    public virtual void MouseExit()
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
