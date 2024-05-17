using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplittingEnemy : Enemy
{
    public override void Kill()
    {
        Vector3 spawnPos = transform.position;
        spawnPos.y = targetTile.transform.position.y;
        GameManager.instance.SpawnEnemy(GameManager.EnemyTypes.Standard,spawnPos + transform.forward * 0.025f, targetTile);
        GameManager.instance.SpawnEnemy(GameManager.EnemyTypes.Standard, spawnPos - transform.forward * 0.025f, targetTile);
        base.Kill();
    }
}
