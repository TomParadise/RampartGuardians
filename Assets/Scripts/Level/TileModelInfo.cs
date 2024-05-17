using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileModelInfo : MonoBehaviour
{
    [SerializeField] private TowerPositioner[] towerPositioners;

    public void Init(int pathIndex)
    {
        foreach(TowerPositioner towerPositioner in towerPositioners)
        {
            towerPositioner.pathIndex = pathIndex;
        }
    }

    public void InitPositioners()
    {
        foreach (TowerPositioner towerPositioner in towerPositioners)
        {
            towerPositioner.InitWithIndex();
        }
    }
}
