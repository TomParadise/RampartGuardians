using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailerCamera : MonoBehaviour
{
    public Vector3 startPos;
    public Quaternion startRot;
    public Vector3 endPos;
    public Quaternion endRot;

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                startPos = transform.position;
                startRot = transform.rotation;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                endPos = transform.position;
                endRot = transform.rotation;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                StartCoroutine(LerpCam());
            }
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                transform.position = startPos;
                transform.rotation = startRot;
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                transform.position = endPos;
                transform.rotation = endRot;
            }
        }
    }

    private IEnumerator LerpCam()
    {
        float timer = 0;
        while (timer < 2f)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, timer / 2f);
            transform.rotation = Quaternion.Lerp(startRot, endRot, timer / 2);
            yield return null;
        }
        transform.position = endPos;
        transform.rotation = endRot;
    }
}
