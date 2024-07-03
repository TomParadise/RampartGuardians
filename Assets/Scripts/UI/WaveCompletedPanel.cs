using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveCompletedPanel : MonoBehaviour
{
    [SerializeField] private UIManager UIManager;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image panelImage;
    [SerializeField] private Sprite[] animSprites;
    [SerializeField] private Color textCol;
    private Coroutine wavePopupCo = null;

    public void ForceStopCo()
    {
        if (wavePopupCo != null) 
        {
            StopCoroutine(wavePopupCo);
            panelImage.sprite = animSprites[animSprites.Length - 1];
            Color clearCol = textCol;
            clearCol.a = 0;
            waveText.color = clearCol;
            descriptionText.color = clearCol;
        }
    }

    public void Init(int wave, int reward)
    {
        if(wavePopupCo != null) { StopCoroutine(wavePopupCo); }
        waveText.text = "Wave " + wave.ToString() + " complete";
        descriptionText.text = "Reward: +" + reward.ToString() + " gold";
        wavePopupCo = StartCoroutine(LerpAnimation());
    }

    private IEnumerator LerpAnimation()
    {
        Color clearCol = textCol;
        clearCol.a = 0;
        float timer;
        waveText.color = clearCol;
        descriptionText.color = clearCol;

        for (int i = animSprites.Length - 1; i >= 0; i--)
        {
            panelImage.sprite = animSprites[i];
            timer = 0.03f;
            while(timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
        timer = 0;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            waveText.color = Color.Lerp(clearCol, textCol, timer / 0.5f);
            descriptionText.color = waveText.color;
            yield return null;
        }
        while (timer < 4.25f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            waveText.color = Color.Lerp(textCol, clearCol, timer / 0.5f);
            descriptionText.color = waveText.color;
            yield return null;
        }
        waveText.color = clearCol;
        descriptionText.color = clearCol;
        for (int i = 0; i < animSprites.Length; i++)
        {
            panelImage.sprite = animSprites[i];
            timer = 0.03f;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
        UIManager.EnableStartWaveButton();
        wavePopupCo = null;
    }
}
