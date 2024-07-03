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
        if(GameManager.instance.waveCounter >= 20) { type = GameManager.EnemyTypes.EliteStandard; }
        if(GameManager.instance.waveCounter >= 40) { type = GameManager.EnemyTypes.EliteRogue; }
        float dist = 0.1f;
        if (GameManager.instance.waveCounter >= 30)
        {
            GameManager.instance.SpawnEnemy(type, spawnPos, targetTile);
            dist = 0.175f;
        }
        GameManager.instance.SpawnEnemy(type, spawnPos + transform.forward * dist, targetTile);
        GameManager.instance.SpawnEnemy(type, spawnPos - transform.forward * dist, targetTile);
        base.Kill();
    }
}
