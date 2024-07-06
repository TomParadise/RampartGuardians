using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Camera cam;
    public Vector2 mapMinimums;
    public Vector2 mapMaximums;
    private Vector3 prevMousePos;
    private Vector3 mouseDownPos;
    public float rotSpeed = 3;
    public float moveSpeed = 3;
    public float zoomSpeed = 500;
    public bool holdingTower = false;
    [SerializeField] private Image[] mouseIcons;

    private Coroutine scrollIconCo = null;

    public bool canMove = true;
    private bool leftDown = false;
    private bool rightDown = false;

    public void ResetPos()
    {
        Vector3 pos = transform.position;
        pos.x = 2;
        pos.z = -1.5f;
        pos.y = 4;
        cam.transform.position = pos;
        cam.transform.rotation = Quaternion.Euler(50, 0, 0);
    }

    public void ClampPosAndRot()
    {
        cam.transform.position = new Vector3(
            Mathf.Clamp(cam.transform.position.x, mapMinimums.x, mapMaximums.x),
            Mathf.Clamp(cam.transform.position.y, 4, 10),
            Mathf.Clamp(cam.transform.position.z, mapMinimums.y, mapMaximums.y));
        Vector3 rot = cam.transform.rotation.eulerAngles;
        rot = new Vector3(
            Mathf.Clamp(rot.x, 25, 60),
             rot.y,
              0);
        cam.transform.rotation = Quaternion.Euler(rot);
    }

    private void Update()
    {
        if (GameManager.instance.gameState == GameManager.GameState.Paused) { return; }
        if (!canMove) { return; }
        Drag();
        if (!holdingTower) { return; }
        Vector2 mousePos = Input.mousePosition;
        Vector3 forward = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * Vector3.forward;
        Vector3 right = cam.transform.right;
        if (mousePos.x < 10)
        {
            cam.transform.position -= right * Time.deltaTime * moveSpeed * 2;
        }
        else if(mousePos.x > Screen.width - 10)
        {
            cam.transform.position += right * Time.deltaTime * moveSpeed * 2;
        }
        if(mousePos.y < 10)
        {
            cam.transform.position -= forward * Time.deltaTime * moveSpeed * 2;
        }
        else if(mousePos.y > Screen.height - 10)
        {
            cam.transform.position += forward * Time.deltaTime * moveSpeed * 2;
        }
        ClampPos();
    }

    public void Drag()
    {
        //left button moves the camera
        if (leftDown)
        {
            Vector3 forward = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * Vector3.forward;
            Vector3 right = cam.transform.right;
            cam.transform.position += forward * (prevMousePos.y - Input.mousePosition.y) * Time.deltaTime * Mathf.Lerp(1.5f, 5.7f, (cam.transform.position.y - 4f) / 10f) * moveSpeed * (1f / (Screen.height / 874f));
            cam.transform.position += right * (prevMousePos.x - Input.mousePosition.x) * Time.deltaTime * Mathf.Lerp(1.5f, 5.7f, (cam.transform.position.y - 4f) / 10f) * moveSpeed * (1f / (Screen.width / 1554f));
            ClampPos();
            prevMousePos = Input.mousePosition;
        }
        //right button rotates the camera
        else if (rightDown)
        {
            cam.transform.RotateAround(mouseDownPos, Vector3.up, (prevMousePos.x - Input.mousePosition.x) * Time.deltaTime * rotSpeed);
            prevMousePos = Input.mousePosition;
            ClampPos();
        }
    }

    private void ClampPos()
    {
        Vector3 pos = cam.transform.position;
        float xMin = Mathf.Lerp(mapMinimums.x, mapMinimums.x - 4, (pos.y - 4) / 6);
        float xMax = Mathf.Lerp(mapMaximums.x, mapMaximums.x + 4, (pos.y - 4) / 6);
        float zMin = Mathf.Lerp(mapMinimums.y, mapMinimums.y - 4, (pos.y - 4) / 6);
        float zMax = Mathf.Lerp(mapMaximums.y, mapMaximums.y + 4, (pos.y - 4) / 6);
        pos = new Vector3(
            Mathf.Clamp(pos.x, xMin, xMax),
            Mathf.Clamp(pos.y, 4, 10),
            Mathf.Clamp(pos.z, zMin, zMax));
        cam.transform.position = pos;
    }

    public void OnPointerUp(BaseEventData bed)
    {
        if (!canMove) { return; }

        PointerEventData ped = (PointerEventData)bed;
        //left button moves the camera
        if (ped.pointerId == -1)
        {
            Color col = Color.white;
            col.a = 0.6f;
            mouseIcons[0].color = col;
            leftDown = false;
        }
        //right button rotates the camera
        else if (ped.pointerId == -2)
        {
            Color col = Color.white;
            col.a = 0.6f;
            mouseIcons[2].color = col;
            rightDown = false;
        }
    }

    public void OnPointerDown(BaseEventData bed)
    {
        if (!canMove) { return; }

        PointerEventData ped = (PointerEventData)bed;
        //left mouse button
        if(ped.pointerId == -1)
        {
            prevMousePos = Input.mousePosition;
            Color col = Color.white;
            col.a = 1f;
            mouseIcons[0].color = col;
            leftDown = true;
        }
        //right mouse button
        else if (ped.pointerId == -2)
        {
            prevMousePos = Input.mousePosition;
            RaycastHit hitData;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hitData, 50, 1 << 9))
            {
                mouseDownPos = hitData.point;
            }
            else
            {
                mouseDownPos = Vector3.zero;
            }
            Color col = Color.white;
            col.a = 1f;
            mouseIcons[2].color = col;
            rightDown = true;
        }
    }

    public void OnPointerDrag(BaseEventData bed)
    {
    }

    public void OnScroll(BaseEventData bed)
    {
        if (!canMove) { return; }

        PointerEventData ped = (PointerEventData)bed;
        if (ped.scrollDelta.y != 0)
        {
            if ((ped.scrollDelta.y < 0 && cam.transform.position.y < 10) ||
                (ped.scrollDelta.y > 0 && cam.transform.position.y > 4))
            {
                cam.transform.position += cam.transform.forward * ped.scrollDelta.y * Time.deltaTime * zoomSpeed;
                ClampPos();
            }
            if (scrollIconCo != null) { StopCoroutine(scrollIconCo); }
            scrollIconCo = StartCoroutine(ScrollIconTimer());
        }
    }

    private IEnumerator ScrollIconTimer()
    {
        Color col = Color.white;
        col.a = 1f;
        mouseIcons[1].color = col;

        float timer = 0.25f;
        while(timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        col.a = 0.6f;
        mouseIcons[1].color = col;
    }
}
