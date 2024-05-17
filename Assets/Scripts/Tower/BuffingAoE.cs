using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffingAoE : MonoBehaviour
{
    [SerializeField] private BuffingTower buffingTower;

    private void OnTriggerEnter(Collider other)
    {
        Tower tower = other.GetComponent<Tower>();
        if(tower == null) { return; }
        buffingTower.TowerTriggerEnter(tower);
    }

    private void OnTriggerExit(Collider other)
    {
        Tower tower = other.GetComponent<Tower>();
        if (tower == null) { return; }
        buffingTower.TowerTriggerExit(other.GetComponent<Tower>());
    }
}
