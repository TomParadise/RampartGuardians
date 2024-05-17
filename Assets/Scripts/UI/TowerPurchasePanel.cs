using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerPurchasePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI preText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI utilityText;
    [SerializeField] private RectTransform descriptionPanel;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private GameObject[] towerPrefabs;
    public Tower[] towerObjects;

    private void Start()
    {
        towerObjects = new Tower[towerPrefabs.Length];
        for(int i = 0; i < towerPrefabs.Length; i++)
        {
            towerObjects[i] = Instantiate(towerPrefabs[i]).GetComponent<Tower>();
            towerObjects[i].gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }

    public void DisablePanel()
    {
        gameObject.SetActive(false);
    }

    public void Init(int towerIndex)
    {
        gameObject.SetActive(true);
        utilityText.gameObject.SetActive(towerIndex >= 6);
        preText.gameObject.SetActive(towerIndex < 6);
        statsText.gameObject.SetActive(towerIndex < 6);
        if (towerIndex < 6)
        {
            statsText.text = towerObjects[towerIndex].damageType + "\n"
                + towerObjects[towerIndex].startDamage.ToString() + (towerObjects[towerIndex].baseProjectileCount > 1 ? "x" + towerObjects[towerIndex].baseProjectileCount : "") + "\n"
                + (1 / towerObjects[towerIndex].startFireRate).ToString("F1") + "s\n"
                + towerObjects[towerIndex].startRange.ToString() + "m";
            descriptionPanel.gameObject.SetActive(true);
            descriptionText.text = towerObjects[towerIndex].descriptionInfo;
            descriptionText.ForceMeshUpdate(true);
            descriptionPanel.sizeDelta = new Vector2(132, 10 + 30 * descriptionText.textInfo.lineCount);
        }
        else
        {
            utilityText.text = towerObjects[towerIndex].damageType;
            descriptionPanel.gameObject.SetActive(false);
        }
        nameText.text = towerObjects[towerIndex].towerName;
        costText.text = towerObjects[towerIndex].towerCost.ToString();
        GetComponent<RectTransform>().anchoredPosition = new Vector2(169 + 130 * towerIndex, 164);
    }
}
