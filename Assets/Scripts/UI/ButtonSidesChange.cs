using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSidesChange : MonoBehaviour
{
    [SerializeField] private Image[] buttonTopBottom;
    [SerializeField] private Image[] buttonLeftRight;
    [SerializeField] private Image[] buttonCorners;

    [SerializeField] private Sprite unclickedTopBottom;
    [SerializeField] private Sprite clickedTopBottom;

    [SerializeField] private Sprite unclickedLeftRight;
    [SerializeField] private Sprite clickedLeftRight;

    [SerializeField] private Sprite unclickedCorner;
    [SerializeField] private Sprite clickedCorner;
    [SerializeField] private Button button;

    public void OnSelect()
    {
        for (int i = 0; i < buttonTopBottom.Length; i++) { buttonTopBottom[i].sprite = clickedTopBottom; }
        for (int i = 0; i < buttonLeftRight.Length; i++) { buttonLeftRight[i].sprite = clickedLeftRight; }
        for (int i = 0; i < buttonCorners.Length; i++) { buttonCorners[i].sprite = clickedCorner; }
        button.interactable = false;
    }

    public void UnClick()
    {
        button.interactable = true;
        for (int i = 0; i < buttonTopBottom.Length; i++) { buttonTopBottom[i].sprite = unclickedTopBottom; }
        for (int i = 0; i < buttonLeftRight.Length; i++) { buttonLeftRight[i].sprite = unclickedLeftRight; }
        for (int i = 0; i < buttonCorners.Length; i++) { buttonCorners[i].sprite = unclickedCorner; }
    }
}
