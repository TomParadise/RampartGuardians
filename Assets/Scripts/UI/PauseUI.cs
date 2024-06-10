using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameObject restartMenu;
    [SerializeField] private Image background;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

    private void Awake()
    {
        masterSlider.value = 0.5f;
        musicSlider.value = 1f;
        SFXSlider.value = 1f;
        ClosePauseUI();
        //OnMasterVolumeChanged(0.5f);
        //OnMusicVolumeChanged(1f);
        //OnSFXVolumeChanged(1f);
    }

    public void OpenPauseUI()
    {
        AudioManager.instance.PlayButtonConfirmSFX();
        gameObject.SetActive(true);
        restartMenu.SetActive(false);
        StartCoroutine(FadeBackground());
    }

    private IEnumerator FadeBackground()
    {
        float timer = 0;
        Color col = background.color;
        col.a = 0;
        while(timer < 0.25f)
        {
            timer += Time.deltaTime;
            col.a = Mathf.Lerp(0, 0.5f, timer / 0.25f);
            background.color = col;
            yield return null;
        }
        col.a = 0.5f;
        background.color = col;
    }

    public void OnMasterVolumeChanged(float value)
    {
        AudioManager.instance.ChangeMasterVolume(value);
    }
    public void OnSFXVolumeChanged(float value)
    {
        AudioManager.instance.ChangeSFXVolume(value);
    }
    public void OnMusicVolumeChanged(float value)
    {
        AudioManager.instance.ChangeMusicVolume(value);
    }

    public void ResumeGame()
    {
        GameManager.instance.TogglePause();
        ClosePauseUI();
    }

    public void OpenRestartGameMenu()
    {
        restartMenu.SetActive(true);
        AudioManager.instance.PlayButtonConfirmSFX();
    }
    public void ConfirmRestartGame()
    {
        GameManager.instance.InitGame();
        AudioManager.instance.PlayButtonConfirmSFX();
        gameObject.SetActive(false);
    }
    public void CancelRestartGame()
    {
        AudioManager.instance.PlayButtonCloseSFX();
        restartMenu.SetActive(false);
    }

    public bool GetIsConfirmMenuOpen() { return restartMenu.activeInHierarchy; }

    public void ClosePauseUI()
    {
        AudioManager.instance.PlayButtonCloseSFX();
        gameObject.SetActive(false);
    }
}
