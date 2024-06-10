using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffingEnemy : Enemy
{
    [SerializeField] private GameObject buffingTrigger;
    private float copiedSpeed;
    private List<Enemy> nearbyEnemies = new List<Enemy>();
    private Enemy copiedEnemy = null;

    public override void Init(Tile spawnTile)
    {
        base.Init(spawnTile);
        BuffSpeed(this);
    }

    public override void ResetObject()
    {
        base.ResetObject();
        copiedSpeed = baseSpeed;
        copiedEnemy = null;
        nearbyEnemies.Clear();
    }

    public override void SetBuffedSpeed(bool buffing)
    {
        if (buffing)
        {
            speed = copiedSpeed * (slowed ? 0.75f : 1.25f);
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
        if (!nearbyEnemies.Contains(enemy)) { nearbyEnemies.Add(enemy); }
        enemy.BuffSpeed(this);

        //copy this enemy's speed if there is NO copied enemy OR if it is slower than the copied enemy
        if(copiedEnemy == null)
        { 
            copiedEnemy = enemy;
            copiedSpeed = copiedEnemy.baseSpeed;
        }
        else if (enemy.baseSpeed < copiedEnemy.baseSpeed)
        {
            copiedEnemy = enemy;
            copiedSpeed = copiedEnemy.baseSpeed;
        }

        speed = copiedSpeed * (slowed ? (buffed ? 0.75f : 0.6f) : (buffed ? 1.25f : 1f));
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (other.gameObject == gameObject ||
            enemy == null) { return; }
        if (nearbyEnemies.Contains(enemy)) { nearbyEnemies.Remove(enemy); }
        enemy.CheckForBuffingAoEs(this);

        if(enemy == copiedEnemy)
        {
            copiedEnemy = null;
            copiedSpeed = baseSpeed;
            foreach (Enemy nearbyEnemy in nearbyEnemies)
            {
                //copy this enemy's speed if there is NO copied enemy OR if it is slower than the copied enemy
                if (copiedEnemy == null)
                {
                    copiedEnemy = nearbyEnemy;
                    copiedSpeed = copiedEnemy.baseSpeed;
                }
                else if (nearbyEnemy.baseSpeed < copiedEnemy.baseSpeed)
                {
                    copiedEnemy = nearbyEnemy;
                    copiedSpeed = copiedEnemy.baseSpeed;
                }
            }
            speed = copiedSpeed * (slowed ? (buffed ? 0.75f : 0.6f) : (buffed ? 1.25f : 1f));
        }
        else if(nearbyEnemies.Count == 0)
        {
            copiedEnemy = null;
            copiedSpeed = baseSpeed;
            speed = copiedSpeed * (slowed ? (buffed ? 0.75f : 0.6f) : (buffed ? 1.25f : 1f));
        }
    }

    public override void Kill()
    {
        foreach (Enemy nearbyEnemy in nearbyEnemies)
        {
            nearbyEnemy.CheckForBuffingAoEs(this);
        }
        base.Kill();
    }
}
