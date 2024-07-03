using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI wavesText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI towersText;
    [SerializeField] private TextMeshProUGUI upgradesText;
    [SerializeField] private GameObject restartButton;

    public void Init(bool victory, int waves, int kills, int gold, int towers, int upgrades)
    {
        titleText.text = victory ? "You win!" : "Game Over!";
        wavesText.text = waves.ToString();
        wavesText.gameObject.SetActive(false);
        killsText.text = kills.ToString();
        killsText.gameObject.SetActive(false);
        goldText.text = gold.ToString();
        goldText.gameObject.SetActive(false);
        towersText.text = towers.ToString();
        towersText.gameObject.SetActive(false);
        upgradesText.text = upgrades.ToString();
        upgradesText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        gameObject.SetActive(true);
        StartCoroutine(LerpStats());
    }

    public void RestartGame()
    {
        GameManager.instance.InitGame();
        AudioManager.instance.PlayButtonHoverSFX();
        gameObject.SetActive(false);
    }

    public IEnumerator LerpStats()
    {
        TextMeshProUGUI[] texts = new TextMeshProUGUI[5] { wavesText, killsText, goldText, towersText, upgradesText };
        float timer;

        for (int i = 0; i < texts.Length; i++)
        {
            timer = 0;
            while (timer < 0.5f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            texts[i].gameObject.SetActive(true);
            AudioManager.instance.PlayButtonHoverSFX();
        }

        restartButton.gameObject.SetActive(true);
    }
}
