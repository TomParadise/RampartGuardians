using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoldEarnedPopUp : PooledObject
{
    private Camera mainCam;
    [SerializeField] private SpriteRenderer rend;
    [SerializeField] private Color clearCol;

    [SerializeField] private Sprite[] textSprites;
    float timer = 0;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        transform.position += Vector3.up * 0.55f;
    }

    public void Init(int goldAmount)
    {
        transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
        int[] digits = GetIntArray(goldAmount);
        int child = 1;
        for(int i = digits.Length - 1; i >= 0; i--)
        {
            GameObject digit = transform.GetChild(child).gameObject;
            digit.GetComponent<SpriteRenderer>().sprite = textSprites[digits[i]];
            digit.transform.localPosition = new Vector3(-0.1f - (0.3f * child++), 0, 0);
            digit.SetActive(true);
        }
        transform.GetChild(0).localPosition = new Vector3(-(digits.Length + 1) * 0.3f - 0.1f, 0, 0);
        Vector3 pos = transform.position;
        pos += transform.right * ((digits.Length + 1) * 0.3f - 0.1f) / 3;
        transform.position = pos;
    }

    int[] GetIntArray(int num)
    {
        List<int> listOfInts = new List<int>();
        while (num > 0)
        {
            listOfInts.Add(num % 10);
            num = num / 10;
        }
        listOfInts.Reverse();
        return listOfInts.ToArray();
    }

    private void Update()
    {
        //if(GameManager.instance.gameState != GameManager.GameState.Playing) { return; }
        transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
        transform.position += Vector3.up * Time.deltaTime * 0.2f;
        timer += Time.deltaTime;

        if(timer < 0.2f)
        {
            return;
        }
        float alpha = Mathf.Lerp(1, 0, (timer - 0.2f) / 0.8f);
        for (int i = 0; i < 5; i++)
        {
            SpriteRenderer rend = transform.GetChild(i).GetComponent<SpriteRenderer>();
            Color col = rend.color;
            col.a = alpha;
            rend.color = col;
        }
        if (timer >= 1f) { Release(); }
    }

    public override void ResetObject()
    {
        base.ResetObject();
        timer = 0;
        for(int i = 0; i < 5; i++)
        {
            if(i > 0 && i < 4)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            SpriteRenderer rend = transform.GetChild(i).GetComponent<SpriteRenderer>();
            Color col = rend.color;
            col.a = 1;
            rend.color = col;
        }
    }
}
