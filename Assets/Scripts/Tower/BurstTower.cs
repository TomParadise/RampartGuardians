using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstTower : AttackingTower
{
    public int projectileCount = 3;
    public override void UpgradeTower()
    {
        projectileCount += (int)upgrades[level - 1].uniqueUpgrade;
        base.UpgradeTower();
    }

    public override void ResetObject()
    {
        base.ResetObject();
        projectileCount = baseProjectileCount;
    }

    public override void SetShotTimer()
    {
        shotTimer = maxFireRateTimer + projectileCount * 0.2f * (1 - buffAmount);
        shotTimer *= (1 - buffAmount);
    }

    public override bool StartShoot()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, range, 1 << 7);
        if (cols.Length == 0) { return false; }
        else
        {
            StartCoroutine(BurstTimer());
            return true;
        }
    }

    private IEnumerator BurstTimer()
    {
        float timer;
        float burstTimer = 1f / projectileCount;
        for(int i = 0; i < projectileCount; i++)
        {
            timer = burstTimer * (1 - buffAmount);
            while(timer > 0)
            {
                while(GameManager.instance.gameState != GameManager.GameState.Playing) { yield return null; }

                timer -= Time.deltaTime;
                yield return null;
            }
            ShootCheck();
        }
    }
}
