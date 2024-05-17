using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHPDisplay : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private TextMeshProUGUI HPText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private Image HPFill;
    [SerializeField] private Image armoredIcon;
    private Enemy clickedEnemy;

    public void Init(Sprite portraitSprite, int HP, int MaxHp, int damage, string enemyName, Enemy enemy, bool armoured)
    {
        if(clickedEnemy != null && clickedEnemy != enemy) { clickedEnemy.StopShowingInfo(false); }
        portrait.sprite = portraitSprite;
        HPText.text = HP.ToString() + "/" + MaxHp.ToString();
        HPFill.fillAmount = (float)HP / MaxHp;
        damageText.text = damage.ToString();
        nameText.text = enemyName;
        clickedEnemy = enemy;
        if(armoured)
        {
            armoredIcon.gameObject.SetActive(true);
        }
        else { armoredIcon.gameObject.SetActive(false); }
        gameObject.SetActive(true);
    }

    public void HideDisplay()
    {
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0) && clickedEnemy.clickedOn)
        {
            clickedEnemy.StopShowingInfo();
            HideDisplay();
        }
    }
}
