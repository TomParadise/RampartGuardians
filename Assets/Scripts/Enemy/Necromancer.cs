using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Necromancer : Enemy
{
    private float maxSpawnTimer = 5;
    private float spawnTimer = 0;

    public override void Init(Tile spawnTile)
    {
        base.Init(spawnTile);
        maxSpawnTimer = 4.5f;
        if (GameManager.instance.stageCount >= 30) { maxSpawnTimer = 3.5f; }
    }

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
        GameManager.EnemyTypes type = GameManager.EnemyTypes.Standard;
        if (GameManager.instance.stageCount >= 20) { type = GameManager.EnemyTypes.EliteStandard; }
        if (GameManager.instance.stageCount >= 40) { type = GameManager.EnemyTypes.EliteRogue; }
        GameManager.instance.SpawnEnemy(type, spawnPos, targetTile);
    }
}
