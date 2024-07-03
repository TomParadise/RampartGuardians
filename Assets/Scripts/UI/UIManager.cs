using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image HPFill;
    [SerializeField] private TextMeshProUGUI HPText;
    [SerializeField] private TextMeshProUGUI GoldText;
    [SerializeField] private Image[] timeCycleImages;
    [SerializeField] private Light skyLight;
    [SerializeField] private Light shadowLight;
    [SerializeField] private Color[] lightColours;
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject towerUpgradePrefab;
    [SerializeField] private TowerPanelUI towerPanelUI;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private WaveCompletedPanel waveCompletedPanel;
    [SerializeField] private EnemyHPDisplay enemyHPDisplay;
    [SerializeField] private TowerPurchasePanel towerPurchasePanel;
    [SerializeField] private PauseUI pauseUI;
    [SerializeField] private GameOverUI gameOverUI;
    private TowerLevelUpPanel activeUpgradePanel = null;
    private Coroutine skyColourCo = null;

    public GameOverUI GetGameOverUI() { return gameOverUI; }

    public PauseUI GetPauseMenu() { return pauseUI; }

    public void OpenPauseUI() { pauseUI.OpenPauseUI(); }

    public void ClosePauseUI() { pauseUI.ClosePauseUI(); }

    public TowerPanelUI GetTowerPanel() { return towerPanelUI; }

    public void UpdateWaveText(string text) { waveText.text = text; }

    private void Start()
    {
        timeCycleImages[1].CrossFadeAlpha(0f, 0f, true);
        timeCycleImages[0].CrossFadeAlpha(1f, 0f, true);
        timeCycleImages[0].transform.SetAsFirstSibling();
        pauseUI.gameObject.SetActive(true);
    }

    public void ShowTowerPurchaseInfo(int towerIndex)
    {
        towerPurchasePanel.Init(towerIndex);
    }

    public void HideTowerPurchaseInfo()
    {
        towerPurchasePanel.DisablePanel();
    }

    public void ShowEnemyHPDisplay(Sprite portraitSprite, int HP, int MaxHp, int damage, string enemyName, Enemy enemy, bool armoured)
    {
        enemyHPDisplay.Init(portraitSprite, HP, MaxHp, damage, enemyName, enemy, armoured);
    }

    public void HideEnemyHPDisplay() { enemyHPDisplay.HideDisplay(); }

    public void DisplayWaveCompleteInfo(int wave, int reward)
    {
        waveCompletedPanel.Init(wave, reward);
    }

    public void ForceStopWavePopup() { waveCompletedPanel.ForceStopCo(); }

    public void StartWaveButton()
    {
        startButton.gameObject.SetActive(false);
        GameManager.instance.StartStage();
    }

    public void EnableStartWaveButton() { startButton.gameObject.SetActive(true); }

    public void SetHP(float HP, float maxHP)
    {
        HPText.text = HP.ToString();
        HPFill.fillAmount = HP / maxHP;
    }

    public void SetGold(int gold)
    {
        GoldText.text = gold.ToString();
    }

    //pass in the time between spawning enemies * the total number of enemies
    public void RoundStart(float maxSpawnTimer)
    {
        if (skyColourCo != null) { StopCoroutine(skyColourCo); }
        skyColourCo = StartCoroutine(LerpSkyColours(maxSpawnTimer));
    }
    public void RoundEnd()
    {
        timeCycleImages[1].CrossFadeAlpha(0f, 0.25f, true);
        timeCycleImages[0].CrossFadeAlpha(1f, 0f, true);
        timeCycleImages[0].transform.SetAsFirstSibling();
        if (skyColourCo != null) { StopCoroutine(skyColourCo); }
        StartCoroutine(LerpSkyToDay());
        enemyHPDisplay.HideDisplay();
    }

    private IEnumerator LerpSkyColours(float maxTime)
    {
        if(maxTime < 4f) { maxTime = 4f; }
        float timer = 0;
        Quaternion startRot = shadowLight.transform.rotation;
        Quaternion goalRot = Quaternion.Euler(0, -90, 0);
        while (timer < maxTime/2)
        {
            while (GameManager.instance.gameState == GameManager.GameState.Paused) { yield return null; }

            timer += Time.deltaTime;
            skyLight.color = Color.Lerp(lightColours[0], lightColours[1], timer / maxTime);
            shadowLight.color = skyLight.color;
            shadowLight.transform.rotation = Quaternion.Lerp(startRot, goalRot, timer / maxTime);
            yield return null;
        }
        timeCycleImages[0].CrossFadeAlpha(0f, 0.25f, true);
        timeCycleImages[1].CrossFadeAlpha(1f, 0f, true);
        timeCycleImages[1].transform.SetAsFirstSibling();
        while (timer < maxTime)
        {
            while (GameManager.instance.gameState == GameManager.GameState.Paused) { yield return null; }

            timer += Time.deltaTime;
            skyLight.color = Color.Lerp(lightColours[0], lightColours[1], timer / maxTime);
            shadowLight.color = skyLight.color;
            shadowLight.transform.rotation = Quaternion.Lerp(startRot, goalRot, timer / maxTime);
            yield return null;
        }
        shadowLight.transform.rotation = goalRot;
        skyLight.color = lightColours[1];
        shadowLight.color = skyLight.color;
    }
    private IEnumerator LerpSkyToDay()
    {
        float timer = 0;
        Color startCol = skyLight.color;
        Quaternion startRot = Quaternion.Euler(180, -30, 0);
        Quaternion goalRot = Quaternion.Euler(50, -30, 0);
        while (timer < 2f)
        {
            timer += Time.deltaTime;
            skyLight.color = Color.Lerp(startCol, lightColours[0], timer / 2f);
            shadowLight.color = skyLight.color;
            shadowLight.transform.rotation = Quaternion.Lerp(startRot, goalRot, timer / 2f);
            yield return null;
        }
        shadowLight.transform.rotation = Quaternion.Euler(50, -30, 0);
        skyLight.color = lightColours[0];
        shadowLight.color = skyLight.color;
    }
    //init for utility towers
    public TowerLevelUpPanel InitTowerUpgradeInfo(Tower tower, string name, int level, string utilityDescription, string upgradeInfo, int _upgradeCost, int sellCost)
    {
        if(activeUpgradePanel != null && activeUpgradePanel.CheckIsMouseInside()) { return null; }
        TowerLevelUpPanel panelInfo = Instantiate(towerUpgradePrefab, transform).GetComponent<TowerLevelUpPanel>();
        panelInfo.Init(tower, name, level, utilityDescription, upgradeInfo, _upgradeCost, sellCost);
        activeUpgradePanel = panelInfo;
        return panelInfo;
    }

    public TowerLevelUpPanel InitTowerUpgradeInfo(Tower tower, string name, int level, float damage, float fireRate, float range, string upgradeInfo, int _upgradeCost, int sellCost, int targetingTower, int targetingType, int killCount = 0)
    {
        if (activeUpgradePanel != null && activeUpgradePanel.CheckIsMouseInside()) { return null; }
        TowerLevelUpPanel panelInfo = Instantiate(towerUpgradePrefab, transform).GetComponent<TowerLevelUpPanel>();
        panelInfo.Init(tower, name, level, damage, fireRate, range, upgradeInfo, _upgradeCost, sellCost, targetingTower, targetingType, killCount);
        activeUpgradePanel = panelInfo;
        return panelInfo;
    }
    public TowerLevelUpPanel InitSorcerorUpgradeInfo(Tower tower, string name, int level, float damage, float fireRate, float range, string upgradeInfo, int _upgradeCost, int sellCost, int targetingTower, int targetingType, int killCount = 0)
    {
        if (activeUpgradePanel != null && activeUpgradePanel.CheckIsMouseInside()) { return null; }
        TowerLevelUpPanel panelInfo = Instantiate(towerUpgradePrefab, transform).GetComponent<TowerLevelUpPanel>();
        panelInfo.InitSorceror(tower, name, level, damage, fireRate, range, upgradeInfo, _upgradeCost, sellCost, targetingTower, targetingType, killCount);
        activeUpgradePanel = panelInfo;
        return panelInfo;
    }

    //private void Update()
    //{
    //    if (Application.isEditor)
    //    {
    //        if (Input.GetKeyDown(KeyCode.Backspace))
    //        {
    //            bool active = transform.GetChild(0).gameObject.activeInHierarchy;
    //            for (int i = 0; i < 6; i++)
    //            {
    //                transform.GetChild(i).gameObject.SetActive(!active);
    //            }
    //        }
    //    }
    //}
}
