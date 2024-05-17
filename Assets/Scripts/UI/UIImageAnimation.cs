using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIImageAnimation : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] animationSprites;
    private float timer = 0;
    private int spriteIndex = 0;

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = 0.1f;
            image.sprite = animationSprites[spriteIndex];
            if (++spriteIndex >= animationSprites.Length) { spriteIndex = 0; }
        }
    }
}
