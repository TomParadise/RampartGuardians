using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Necromancer : Enemy
{
    private float maxSpawnTimer = 5;
    private float spawnTimer = 0;
    
    public override void Update()
    {
        base.Update();
        if(GameManager.instance.gameState != GameManager.GameState.Playing) { return; }
        if(!walking) { return; }

        spawnTimer += Time.deltaTime;

        if(spawnTimer >= maxSpawnTimer)
        {
            SpawnEnemy();
            spawnTimer = 0;
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPos = transform.position;
        spawnPos.y = 0;
        GameManager.instance.SpawnEnemy(GameManager.EnemyTypes.Standard, spawnPos, targetTile);
    }
}
