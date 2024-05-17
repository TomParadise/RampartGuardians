using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerButton : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private int towerIndex = 0;
    [SerializeField] private TowerPurchasePanel purchasePanel;
    private Tower towerInstance = null;
    public TowerPositioner positioner = null;
    public void OnPointerEnter()
    {
        GameManager.instance.GetUIManager().ShowTowerPurchaseInfo(towerIndex);
    }
    public void OnPointerExit()
    {
        GameManager.instance.GetUIManager().HideTowerPurchaseInfo();
    }

    public void OnDrag()
    {
        //check if the player has enough money before 
        //create the tower and enable positioners
        //make tower follow the cursor and snap position to the nearest grid space and compare to positioners
        //if release on a positioner then place the tower and pay the cost
        if (GameManager.instance.GetDoesPlayerHaveEnoughGold(purchasePanel.towerObjects[towerIndex].towerCost))
        {
            towerInstance = GameManager.instance.GetTower(towerIndex).GetComponent<Tower>();
            towerInstance.transform.SetParent(transform);
            LevelGeneration.instance.TogglePositioners(true, towerInstance.utilityTile);

            GameManager.instance.GetUIManager().GetTowerPanel().holdingTower = true;
            GameManager.instance.camMovement.holdingTower = true;
        }
    }

    private void Update()
    {
        if(towerInstance != null)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitData;
            Vector3 pos;
            if (Physics.Raycast(ray, out hitData, 50, 1 << 9))
            {
                pos = hitData.point;
            }
            else
            {
                pos = cam.transform.position + cam.transform.forward * 5;
            }
            Vector3 dir = cam.transform.forward;
            dir.y = 0;
            towerInstance.transform.rotation = Quaternion.LookRotation((-dir));
            towerInstance.CheckForPositioners(pos);

            if (Input.GetMouseButtonUp(0))
            {
                //place turret
                LevelGeneration.instance.TogglePositioners(false, towerInstance.utilityTile);
                if (positioner != null)
                {
                    GameManager.instance.PlaceTower(towerInstance, positioner, purchasePanel.towerObjects[towerIndex].towerCost);
                }
                else//stop trying to place turret
                {
                    towerInstance.Release();
                }
                towerInstance = null;
                GameManager.instance.GetUIManager().GetTowerPanel().holdingTower = false;
                GameManager.instance.camMovement.holdingTower = false;
            }
        }
    }


}
