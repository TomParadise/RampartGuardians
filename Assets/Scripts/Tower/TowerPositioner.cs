using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPositioner : MonoBehaviour
{
    public bool occupied = false;
    public bool utility = false;
    public int pathIndex = 10000;
    // Start is called before the first frame update
    //void Start()
    //{
    //    Collider[] cols = Physics.OverlapSphere(transform.position, 0.1f, (1 << 6));
    //    if(cols.Length > 0 && cols[0] != GetComponent<Collider>())
    //    {
    //        gameObject.SetActive(false);
    //        return;
    //    }

    //    LevelGeneration.instance.AddTowerPositioner(this, utility);
    //}

    public void InitWithIndex()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 0.1f, (1 << 6));
        if (cols.Length > 0)
        {
            foreach(Collider col in cols)
            {
                if(col != GetComponent<Collider>() && col.GetComponent<TowerPositioner>().pathIndex <= pathIndex)
                {
                    gameObject.SetActive(false);
                    return;
                }
            }
        }

        LevelGeneration.instance.AddTowerPositioner(this, utility);
    }

    public void ToggleActive(bool enabled)
    {
        if (occupied) { return; }
        gameObject.SetActive(enabled);
    }

    public void SetOccupied()
    {
        occupied = true;
        gameObject.SetActive(false);
    }

    public void OnTowerSold()
    {
        occupied = false;
    }
}
