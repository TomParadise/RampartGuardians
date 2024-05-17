using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTowardsCamera : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    private Camera mainCam;

    public void SetSprite(int index)
    {
        if(index == -1) { GetComponent<SpriteRenderer>().sprite = null; }
        else { GetComponent<SpriteRenderer>().sprite = sprites[index]; }
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(mainCam.transform.position - transform.position);
    }
}
