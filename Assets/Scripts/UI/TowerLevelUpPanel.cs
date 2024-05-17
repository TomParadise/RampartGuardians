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
    [SerializeField] private RectTransform clickRect;
    [SerializeField] private ButtonSidesChange selectedTargetingButton;
    [SerializeField] private ButtonSidesChange[] targetingButtons;
    [SerializeField] private GameObject targetingPanel;
    private int upgradeCost;
    private Tower selectedTower;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Image fireRateChargeFill;
    [SerializeField] private Color[] chargeCols;
    [SerializeField] private RectTransform targetingRect;
    [SerializeField] private GameObject targetingButtonHolder;

    private Camera mainCam;
    private CapsuleCollider towerCol;

    public void SetPosition()
    {
        Vector2 screenPos = mainCam.WorldToScreenPoint(towerCol.bounds.center + Vector3.up * towerCol.height * 0.55f * selectedTower.transform.lossyScale.y);
        if (screenPos.y > Screen.height  * 0.6f) { screenPos = mainCam.WorldToScreenPoint(selectedTower.transform.position) - new Vector3(0, 162); }
        else { screenPos.y += 195.5f; }
        rect.position = screenPos;
    }

    public void SetTargeting(int buttonIndex)
    {
        selectedTargetingButton.UnClick();
        selectedTargetingButton = targetingButtons[buttonIndex];
        selectedTargetingButton.OnSelect();
        selectedTower.GetComponent<AttackingTower>().SetTargeting((AttackingTower.TargetType)buttonIndex);
    }

    public void Init(Tower tower, string name, int level, float damage, float fireRate, float range, string upgradeInfo, int _upgradeCost, int sellCost, int initTargeting, int targetType = 1, int killCount = 0)
    {
        selectedTower = tower;
        titleText.text = name;
        levelText.text = level.ToString();
        statsText.text = damage.ToString() + "\n" + (1 / fireRate).ToString("F1") + "s\n" + range.ToString() + "m";
        upgradeText.text = upgradeInfo;
        upgradeCost = _upgradeCost;
        upgradeCostText.text = upgradeCost.ToString();
        sellCostText.text = sellCost.ToString();
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
        levelText.text = level.ToString();
        statsText.gameObject.SetActive(false);
        upgradeText.text = upgradeInfo;
        upgradeCost = _upgradeCost;
        upgradeCostText.text = upgradeCost.ToString();
        sellCostText.text = sellCost.ToString();
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

    public void OnUpgradeButton()
    {
        if (!GameManager.instance.GetDoesPlayerHaveEnoughGold(upgradeCost)) { return; }
        GameManager.instance.GivePlayerGold(-upgradeCost);
        selectedTower.UpgradeTower();
        
        //update upgrade info and stats
    }

    public void OnSellButton()
    {
        selectedTower.SellTower();
        Destroy(gameObject);
    }

    private void LateUpdate()
    {
        SetPosition();
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 localPos = clickRect.InverseTransformPoint(Input.mousePosition);
            if (!clickRect.rect.Contains(localPos))
            {
                selectedTower.HideUpgradeInfo();
                Destroy(gameObject);
            }
        }
    }

    public void UpdateCharge(float fill) 
    {
        fireRateChargeFill.fillAmount = fill;
        fireRateChargeFill.color = Color.Lerp(chargeCols[0], chargeCols[1], fill);
    }
}