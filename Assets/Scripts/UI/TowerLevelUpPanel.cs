using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerLevelUpPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI preText;
    [SerializeField] private TextMeshProUGUI utilityText;
    [SerializeField] private TextMeshProUGUI upgradeText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI sellCostText;
    [SerializeField] private RectTransform[] clickRects;
    [SerializeField] private ButtonSidesChange selectedTargetingButton;
    [SerializeField] private ButtonSidesChange[] targetingButtons;
    [SerializeField] private GameObject targetingPanel;
    private int upgradeCost;
    public Tower selectedTower;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Image fireRateChargeFill;
    [SerializeField] private Color[] chargeCols;
    [SerializeField] private RectTransform targetingRect;
    [SerializeField] private GameObject targetingButtonHolder;
    [SerializeField] private RectTransform sellValueRect;
    [SerializeField] private TextMeshProUGUI sellValueInfo;

    private Camera mainCam;
    private CapsuleCollider towerCol;

    private void Start()
    {
        transform.SetSiblingIndex(transform.parent.childCount - 5);
    }

    public void SetPosition()
    {
        Vector2 screenPos = mainCam.WorldToScreenPoint(towerCol.bounds.center + Vector3.up * towerCol.height * 0.55f * selectedTower.transform.lossyScale.y);
        if (screenPos.y > Screen.height  * 0.6f) { screenPos = mainCam.WorldToScreenPoint(selectedTower.transform.position) - new Vector3(0, 0.16f * Screen.height); }
        else { screenPos.y += 0.215f * Screen.height; }
        rect.position = screenPos;
    }

    public void SetTargeting(int buttonIndex)
    {
        selectedTargetingButton.UnClick();
        selectedTargetingButton = targetingButtons[buttonIndex];
        selectedTargetingButton.OnSelect();
        selectedTower.GetComponent<AttackingTower>().SetTargeting((AttackingTower.TargetType)buttonIndex);
        AudioManager.instance.PlayButtonHoverSFX();
    }

    public void InitSpreadDamageInfo(Tower tower, int projectiles, float damage, float fireRate, float range)
    {
        statsText.text = damage.ToString() + "x" + projectiles.ToString() + "\n";
        if (tower.buffAmount > 0)
        {
            fireRate = (1 - tower.buffAmount) * (1 / fireRate);
            statsText.text += "<color=#79E4FD>" + fireRate.ToString("F1") + "</color>s\n";
        }
        else { statsText.text += (1 / fireRate).ToString("F1") + "s\n"; }
        statsText.text += range.ToString() + "m";
    }

    public void InitSorceror(Tower tower, string name, int level, float damage, float fireRate, float range, string upgradeInfo, int _upgradeCost, int sellCost, int initTargeting, int targetType = 1, int killCount = 0)
    {
        preText.text = "Duration:\nFire rate:\nRange:";
        Init(tower, name, level, damage, fireRate, range, upgradeInfo, _upgradeCost, sellCost, initTargeting, targetType, killCount);
        statsText.text = damage.ToString("F1") + "s\n";
        if (tower.buffAmount > 0)
        {
            fireRate = (1 - tower.buffAmount) * (1 / fireRate);
            statsText.text += "<color=#79E4FD>" + fireRate.ToString("F1") + "</color>s\n";
        }
        else { statsText.text += (1 / fireRate).ToString("F1") + "s\n"; }
        statsText.text += range.ToString() + "m";

        fireRateChargeFill.gameObject.SetActive(true);
    }

    public void Init(Tower tower, string name, int level, float damage, float fireRate, float range, string upgradeInfo, int _upgradeCost, int sellCost, int initTargeting, int targetType = 1, int killCount = 0)
    {
        selectedTower = tower;
        titleText.text = name;
        if (level == 4) { levelText.text = "MAX"; }
        else { levelText.text = level.ToString(); }

        statsText.text = damage.ToString() + "\n";
        if (tower.buffAmount > 0)
        {
            fireRate = (1 - tower.buffAmount) * (1 / fireRate);
            statsText.text += "<color=#79E4FD>" + fireRate.ToString("F1") + "</color>s\n"; 
        }
        else { statsText.text += (1 / fireRate).ToString("F1") + "s\n"; }
        statsText.text += range.ToString() + "m";

        upgradeText.text = upgradeInfo;
        upgradeCost = _upgradeCost;
        upgradeCostText.text = upgradeCost.ToString();
        int towerWavesActive = Mathf.Min(6, tower.roundsActive);
        if(tower.roundsActive == -1)
        {
            towerWavesActive = 6;
            sellValueRect.gameObject.SetActive(false);
        }
        else
        {
            sellValueInfo.text = "Sell value = <color=#C58500><b>" + (70 + towerWavesActive * 5) + "</color></b>%";
            if (towerWavesActive < 6) { sellValueInfo.text += "\nFull value in <color=#C58500><b>" + (6 - towerWavesActive) + "</color></b> wave" + ((6 - towerWavesActive) > 1 ? "s" : ""); }
        }
        sellCostText.text = Mathf.Floor(((float)sellCost * (0.70f + (towerWavesActive * 0.05f)))).ToString();

        if (initTargeting == -1)
        {
            targetingPanel.SetActive(false);
            fireRateChargeFill.gameObject.SetActive(false);
        }
        else if(initTargeting == 1)
        {
            selectedTargetingButton.UnClick();
            selectedTargetingButton = targetingButtons[targetType];
            selectedTargetingButton.OnSelect();
            killsText.text = "Kills:" + killCount.ToString();
        }
        else if(initTargeting == 0)
        {
            targetingButtonHolder.SetActive(false);
            targetingRect.sizeDelta = new Vector2(targetingRect.sizeDelta.x, 0);
            killsText.text = "Kills:" + killCount.ToString();
        }

        towerCol = tower.GetComponent<CapsuleCollider>();
        mainCam = Camera.main;
        fireRateChargeFill.color = chargeCols[1];
    }
    //init for utility towers
    public void Init(Tower tower, string name, int level, string utilityDescription, string upgradeInfo, int _upgradeCost, int sellCost)
    {
        selectedTower = tower;
        titleText.text = name;
        if (level == 4) { levelText.text = "MAX"; }
        else { levelText.text = level.ToString(); }
        statsText.gameObject.SetActive(false);
        upgradeText.text = upgradeInfo;
        upgradeCost = _upgradeCost;
        upgradeCostText.text = upgradeCost.ToString();
        int towerWavesActive = Mathf.Min(6, tower.roundsActive);
        if (tower.roundsActive == -1)
        {
            towerWavesActive = 6;
            sellValueRect.gameObject.SetActive(false);
        }
        else
        {
            sellValueInfo.text = "Sell value = <color=#C58500><b>" + (70 + towerWavesActive * 5) + "</color></b>%";
            if (towerWavesActive < 6) { sellValueInfo.text += "\nFull value in <color=#C58500><b>" + (6 - towerWavesActive) + "</color></b> wave" + ((6 - towerWavesActive) > 1 ? "s" : ""); }
        }
        sellCostText.text = Mathf.Floor(((float)sellCost * (0.70f + (towerWavesActive * 0.05f)))).ToString();

        targetingPanel.SetActive(false);
        fireRateChargeFill.gameObject.SetActive(false);
        preText.gameObject.SetActive(false);
        utilityText.text = utilityDescription;
        utilityText.gameObject.SetActive(true);

        towerCol = tower.GetComponent<CapsuleCollider>();
        mainCam = Camera.main;
        fireRateChargeFill.color = chargeCols[1];
    }

    public void UpdateKillsText(int kills)
    {
        killsText.text = "Kills:" + kills.ToString();
    }
    public void UpdateSellCost(int sellCost, int roundsActive)
    {
        int towerWavesActive = Mathf.Min(6, roundsActive);
        if (roundsActive == -1)
        {
            towerWavesActive = 6;
            sellValueRect.gameObject.SetActive(false);
        }
        else
        {
            sellValueInfo.text = "Sell value = <color=#C58500><b>" + (70 + towerWavesActive * 5) + "</color></b>%";
            if (towerWavesActive < 6) { sellValueInfo.text += "\nFull value in <color=#C58500><b>" + (6 - towerWavesActive) + "</color></b> wave" + ((6 - towerWavesActive) > 1 ? "s" : ""); }
        }
        sellCostText.text = Mathf.Floor(((float)sellCost * (0.70f + (towerWavesActive * 0.05f)))).ToString();
    }

    public void OnUpgradeButton()
    {
        if (!GameManager.instance.GetDoesPlayerHaveEnoughGold(upgradeCost)) 
        {
            AudioManager.instance.PlayNotEnoughGoldSFX();
            return;
        }
        GameManager.instance.GivePlayerGold(-upgradeCost, false);
        selectedTower.UpgradeTower();
        
        //update upgrade info and stats
    }

    public void OnSellButton()
    {
        AudioManager.instance.PlayButtonCloseSFX();
        selectedTower.SellTower();
        Destroy(gameObject);
    }

    private void LateUpdate()
    {
        SetPosition();
        if (Input.GetMouseButtonDown(0))
        {
            bool inside = false;
            for(int i = 0; i < clickRects.Length; i++)
            {
                Vector2 localPos = clickRects[i].InverseTransformPoint(Input.mousePosition);
                if (clickRects[i].gameObject.activeInHierarchy && clickRects[i].rect.Contains(localPos))
                {
                    inside = true;
                    break;
                }
            }
            if(!inside)
            {
                AudioManager.instance.PlayButtonCloseSFX();
                selectedTower.HideUpgradeInfo();
                Destroy(gameObject);
            }
        }
    }

    public bool CheckIsMouseInside()
    {
        bool inside = false;
        for (int i = 0; i < clickRects.Length; i++)
        {
            Vector2 localPos = clickRects[i].InverseTransformPoint(Input.mousePosition);
            if (clickRects[i].gameObject.activeInHierarchy && clickRects[i].rect.Contains(localPos))
            {
                inside = true;
                break;
            }
        }
        return inside;
    }

    public void UpdateCharge(float fill) 
    {
        fireRateChargeFill.fillAmount = fill;
        fireRateChargeFill.color = Color.Lerp(chargeCols[0], chargeCols[1], fill);
    }
}