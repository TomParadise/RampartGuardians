using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Camera cam;
    public Vector2 mapMinimums;
    public Vector2 mapMaximums;
    private Vector3 prevMousePos;
    private Vector3 mouseDownPos;
    public float rotSpeed = 5;
    public float moveSpeed = 5;
    public float zoomSpeed = 500;
    public bool holdingTower = false;

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

    public void OnPointerDown(BaseEventData bed)
    {
        PointerEventData ped = (PointerEventData)bed;
        if(ped.pointerId == -1)
        {
            prevMousePos = Input.mousePosition;
        }
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
        }
    }

    public void OnPointerDrag(BaseEventData bed)
    {
        PointerEventData ped = (PointerEventData)bed;
        //left button moves the camera
        if (ped.pointerId == -1)
        {
            Vector3 forward = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * Vector3.forward;
            Vector3 right = cam.transform.right;
            cam.transform.position += forward * (prevMousePos.y - Input.mousePosition.y) * Time.deltaTime * moveSpeed;
            cam.transform.position += right * (prevMousePos.x - Input.mousePosition.x) * Time.deltaTime * moveSpeed;
            ClampPos();
            prevMousePos = Input.mousePosition;
        }
        //right button rotates the camera
        else if (ped.pointerId == -2)
        {
            cam.transform.RotateAround(mouseDownPos, Vector3.up, (prevMousePos.x - Input.mousePosition.x) * Time.deltaTime * rotSpeed);
            prevMousePos = Input.mousePosition;
            ClampPos();
        }
    }

    public void OnScroll(BaseEventData bed)
    {
        PointerEventData ped = (PointerEventData)bed;
        if (ped.scrollDelta.y != 0)
        {
            if ((ped.scrollDelta.y < 0 && cam.transform.position.y < 10) ||
                (ped.scrollDelta.y > 0 && cam.transform.position.y > 4))
            {
                cam.transform.position += cam.transform.forward * ped.scrollDelta.y * Time.deltaTime * zoomSpeed;
                ClampPos();
            }
        }
    }
}
