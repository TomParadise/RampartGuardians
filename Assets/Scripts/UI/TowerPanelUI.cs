using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerPanelUI : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    private bool pointerInside = false;
    public bool holdingTower = false;
    public float speed = 100;

    public void OnPointerEnter()
    {
        pointerInside = true;
    }
    public void OnPointerExit()
    {
        pointerInside = false;
    }

    // Update is called once per frame
    void Update()
    {

        if(pointerInside && !holdingTower)
        {
            Vector2 pos = rect.anchoredPosition;
            pos.y += speed * Time.deltaTime;
            if(pos.y > 85) { pos.y = 85; }
            rect.anchoredPosition = pos;
        }
        else
        {
            Vector2 pos = rect.anchoredPosition;
            pos.y -= speed * Time.deltaTime;
            if(pos.y < -75) { pos.y = -75; }
            rect.anchoredPosition = pos;
        }
    }
}
