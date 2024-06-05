using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextFollowCursor : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI followText;
    [SerializeField] private RectTransform rect;

    public void Init(string text)
    {
        followText.text = text;
    }

    // Update is called once per frame
    void Update()
    {
        rect.position = Input.mousePosition + new Vector3(120, 25, 0);
    }
}
