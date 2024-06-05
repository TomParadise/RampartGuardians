using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplittingEnemy : Enemy
{
    public override void Kill()
    {
        Vector3 spawnPos = transform.position;
        spawnPos.y = targetTile.transform.position.y;
        GameManager.EnemyTypes type = GameManager.EnemyTypes.Standard;
        if(GameManager.instance.stageCount > 20) { type = GameManager.EnemyTypes.EliteStandard; }
        if(GameManager.instance.stageCount > 40) { type = GameManager.EnemyTypes.EliteRogue; }
        GameManager.instance.SpawnEnemy(type, spawnPos + transform.forward * 0.025f, targetTile);
        GameManager.instance.SpawnEnemy(type, spawnPos - transform.forward * 0.025f, targetTile);
        base.Kill();
    }
}
