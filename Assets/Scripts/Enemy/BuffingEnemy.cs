using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffingEnemy : Enemy
{
    [SerializeField] private GameObject buffingTrigger;
    private float copiedSpeed;
    public override void Init(Tile spawnTile)
    {
        base.Init(spawnTile);
        BuffSpeed();
    }

    public override void ResetObject()
    {
        base.ResetObject();
        copiedSpeed = baseSpeed;
    }

    public override void SetBuffedSpeed(bool buffing)
    {
        if (buffing)
        {
            speed = copiedSpeed * (slowed ? 0.75f : 1.25f);
        }
        else
        {
            speed = copiedSpeed * (slowed ? 0.6f : 1);
        }
    }

    public override void SetSlowedSpeed(bool slowing)
    {
        if (slowing)
        {
            speed = copiedSpeed * (buffed ? 0.75f : 0.6f);
        }
        else
        {
            speed = copiedSpeed * (buffed ? 1.25f : 1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (other.gameObject == gameObject ||
            enemy == null) { return; }
        
        enemy.BuffSpeed();
        if(enemy.baseSpeed < copiedSpeed)
        {
            copiedSpeed = enemy.baseSpeed;
        }
        else if(copiedSpeed >= baseSpeed && enemy.baseSpeed > copiedSpeed)
        { 
            copiedSpeed = enemy.baseSpeed;
        }
        speed = copiedSpeed * (slowed ? (buffed ? 0.75f : 0.6f) : (buffed ? 1.25f : 1f));
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == gameObject ||
            other.GetComponent<Enemy>() == null) { return; }

        other.GetComponent<Enemy>().CheckForBuffingAoEs(buffingTrigger);

        Collider[] cols = Physics.OverlapSphere(transform.position, 0.5f, 1 << 7);
        float lowestSpeed = baseSpeed;
        float highestSpeed = baseSpeed;
        foreach (Collider col in cols)
        {
            if(col == other) { continue; }

            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy.baseSpeed < lowestSpeed) { lowestSpeed = enemy.baseSpeed; }
            if (enemy.baseSpeed >= highestSpeed) { highestSpeed = enemy.baseSpeed; }
        }
        if(lowestSpeed == baseSpeed)
        {
            copiedSpeed = highestSpeed;
        }
        else
        {
            copiedSpeed = lowestSpeed;
        }
        speed = copiedSpeed * (slowed ? (buffed ? 0.75f : 0.6f) : (buffed ? 1.25f : 1f));
    }

    private void OnDisable()
    {
        if (GameManager.instance.gameState != GameManager.GameState.Planning)
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, 0.5f, 1 << 7);

            foreach (Collider col in cols)
            {
                if (col.gameObject == gameObject) { continue; }

                col.GetComponent<Enemy>().CheckForBuffingAoEs(buffingTrigger);
            }
        }
    }
}
