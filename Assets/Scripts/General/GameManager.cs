using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField] private UIManager UIManager;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject[] towerPrefabs;
    [SerializeField] private GameObject[] projectilePrefabs;
    [SerializeField] private GameObject[] effectPrefabs;
    [SerializeField] private GameObject goldPopUpPrefab;
    [SerializeField] private GameObject damagePopUpPrefab;
    [SerializeField] private GameObject HPDamagePrefab;
    [SerializeField] private EnemySpawnGroups[] enemySpawnGroups;
    [SerializeField] private EnemySpawnGroups currentSpawnGroup;
    [SerializeField] private Transform enemyHolder;
    [SerializeField] private Transform towerHolder;
    [SerializeField] private Transform projectileHolder;
    [SerializeField] private Transform effectHolder;
    [SerializeField] public CameraMovement camMovement;
    private ObjectPool<PooledObject>[] enemyPools;
    private ObjectPool<PooledObject>[] towerPools;
    private ObjectPool<PooledObject>[] projectilePools;
    private ObjectPool<PooledObject>[] effectPools;
    private ObjectPool<PooledObject> goldPopUpPool;
    private ObjectPool<PooledObject> damagePopUpPool;
    private ObjectPool<PooledObject> HPDamagePool;
    private float enemySpawnTimer = 0;
    public GameState gameState;
    public GameState prevGameState;
    public int stageCount = 0;
    private int enemyCount = 0;

    public int playerHP = 2000;
    public int maxHP = 20;
    public int playerGold = 10000;
    public bool spawningEnemies = true;

    private float currentWaveTotalEnemyCount;

    public List<BuffingTower> buffingTowers = new List<BuffingTower>();
    public List<Tower> towers = new List<Tower>();
    public List<Enemy> enemies = new List<Enemy>();

    private int waveCounter = 1;

    [System.Serializable]
    public struct EnemySpawnGroups
    {
        public EnemySpawnGroups(List<EnemySpawnInfo> _enemySpawnInfos)
        {
            this.enemySpawnInfos = new List<EnemySpawnInfo>();
            foreach(EnemySpawnInfo info in _enemySpawnInfos)
            {
                EnemySpawnInfo spawnInfo = new EnemySpawnInfo();
                spawnInfo.enemyType = info.enemyType;
                spawnInfo.enemyCount = info.enemyCount;
                this.enemySpawnInfos.Add(spawnInfo); 
            }
        }
        public List<EnemySpawnInfo> enemySpawnInfos;
    }
    
    [System.Serializable]
    public class EnemySpawnInfo
    {
        public EnemyTypes enemyType;
        public int enemyCount;
    }

    public enum EnemyTypes
    {
        Standard,
        Rogue,
        Tank,
        Mage,
        Splitter,
        Golem,
        Necromancer,
        EliteStandard,
        EliteRogue,
        EliteTank,
        EliteMage,
        EliteGolem
    }
    public enum GameState
    {
        Playing,
        Planning,
        Paused
    }

    // Start is called before the first frame update
    void Start()
    {
        gameState = GameState.Planning;
        prevGameState = GameState.Planning;
        InitPools();
        InitGame();
    }

    public void InitPools()
    {
        int length = effectPrefabs.Length;
        effectPools = new ObjectPool<PooledObject>[length];
        int[] capacities = new int[6] { 5, 5, 10, 10, 5, 15 };
        for (int i = 0; i < length; i++)
        {
            GameObject poolObject = new GameObject(effectPrefabs[i].name + " pool gameobject");
            poolObject.transform.parent = transform.GetChild(0);
            effectPools[i] = poolObject.AddComponent<GenericPool>().Init(effectPrefabs[i], capacities[i]).GetPool();
        }
        length = enemyPrefabs.Length;
        enemyPools = new ObjectPool<PooledObject>[length];
        capacities = new int[12] { 50, 20, 10, 10, 10, 5, 5, 25, 10, 5, 5, 5};
        for (int i = 0; i < length; i++)
        {
            GameObject poolObject = new GameObject(enemyPrefabs[i].name + " pool gameobject");
            poolObject.transform.parent = transform.GetChild(0);
            enemyPools[i] = poolObject.AddComponent<GenericPool>().Init(enemyPrefabs[i], capacities[i]).GetPool();
        }
        length = towerPrefabs.Length;
        towerPools = new ObjectPool<PooledObject>[length];
        capacities = new int[8] { 20, 20, 20, 20, 20, 20, 10, 10 };
        for (int i = 0; i < length; i++)
        {
            GameObject poolObject = new GameObject(towerPrefabs[i].name + " pool gameobject");
            poolObject.transform.parent = transform.GetChild(0);
            towerPools[i] = poolObject.AddComponent<GenericPool>().Init(towerPrefabs[i], capacities[i]).GetPool();
        }
        length = projectilePrefabs.Length;
        projectilePools = new ObjectPool<PooledObject>[length];
        capacities = new int[4] { 5, 5, 5, 5 };
        for (int i = 0; i < length; i++)
        {
            GameObject poolObject = new GameObject(projectilePrefabs[i].name + " pool gameobject");
            poolObject.transform.parent = transform.GetChild(0);
            projectilePools[i] = poolObject.AddComponent<GenericPool>().Init(projectilePrefabs[i], capacities[i]).GetPool();
        }
        GameObject popUpPoolObject = new GameObject(goldPopUpPrefab.name + " pool gameobject");
        popUpPoolObject.transform.parent = transform.GetChild(0);
        goldPopUpPool = popUpPoolObject.AddComponent<GenericPool>().Init(goldPopUpPrefab, 20).GetPool();
        GameObject damagePoolObject = new GameObject(damagePopUpPrefab.name + " pool gameobject");
        damagePoolObject.transform.parent = transform.GetChild(0);
        damagePopUpPool = damagePoolObject.AddComponent<GenericPool>().Init(damagePopUpPrefab, 35).GetPool();
        GameObject HPDamageObject = new GameObject(HPDamagePrefab.name + " pool gameobject");
        HPDamageObject.transform.parent = transform.GetChild(0);
        HPDamagePool = HPDamageObject.AddComponent<GenericPool>().Init(HPDamagePrefab, 5).GetPool();
    }


    public bool GetDoesPlayerHaveEnoughGold(int cost) { return playerGold >= cost; }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            LevelGeneration.instance.ExpandMap(100);
            //StopStage(); 
        }
        if (gameState != GameState.Playing) { return; }
        EnemySpawnTimer();
    }

    public void TogglePause()
    {
        if(gameState == GameState.Paused) { gameState = prevGameState; }
        else { gameState = GameState.Paused; }
        foreach(Tower tower in towers)
        {
            tower.TogglePause(gameState == GameState.Paused);
        }
        foreach(Enemy enemy in enemies)
        {
            enemy.TogglePause(gameState == GameState.Paused);
        }
    }

    private void EnemySpawnTimer()
    {
        if(spawningEnemies)
        {
            enemySpawnTimer -= Time.deltaTime;
            if (enemySpawnTimer <= 0)
            {
                SpawnEnemy();
                enemySpawnTimer = 0.55f;
            }
        }
    }

    public void SpawnEnemy()
    {
        int chosenIndex = Random.Range(0, currentSpawnGroup.enemySpawnInfos.Count);

        Enemy enemy = enemyPools[(int)currentSpawnGroup.enemySpawnInfos[chosenIndex].enemyType].Get().GetComponent<Enemy>();
        enemy.transform.SetParent(enemyHolder);
        Tile spawnTile = LevelGeneration.instance.GetSpawnTile();
        enemy.transform.position = spawnTile.transform.position;
        enemy.Init(spawnTile);
        enemy.gameObject.SetActive(true);
        enemies.Add(enemy);
        enemyCount++;

        currentSpawnGroup.enemySpawnInfos[chosenIndex].enemyCount--;
        if(currentSpawnGroup.enemySpawnInfos[chosenIndex].enemyCount <= 0) { currentSpawnGroup.enemySpawnInfos.RemoveAt(chosenIndex); }
        if(currentSpawnGroup.enemySpawnInfos.Count == 0) { spawningEnemies = false; }
    }
    public void SpawnEnemy(EnemyTypes enemyType, Vector3 pos, Tile goalTile)
    {
        Enemy enemy = enemyPools[(int)enemyType].Get().GetComponent<Enemy>();
        enemies.Add(enemy);
        enemy.transform.SetParent(enemyHolder);
        enemy.transform.position = pos;
        enemy.InitWithTarget(goalTile);
        enemy.gameObject.SetActive(true);

        enemyCount++;
    }

    public GameObject GetTower(int towerIndex)
    {
        GameObject tower = towerPools[towerIndex].Get().gameObject;
        tower.gameObject.SetActive(true);
        return tower;
    }

    public UIManager GetUIManager() { return UIManager; }

    public void PlaceTower(Tower tower, TowerPositioner positioner, int towerCost)
    {
        playerGold -= towerCost;
        UIManager.SetGold(playerGold);
        tower.PlaceOnPositioner(positioner, towerCost);
        tower.transform.SetParent(positioner.transform.parent);
        tower.transform.position = positioner.transform.position;
        positioner.SetOccupied();
    }

    public GameObject GetProjectile(int projectileIndex)
    {
        GameObject projectile = projectilePools[projectileIndex].Get().gameObject;
        projectile.transform.SetParent(projectileHolder);
        return projectile;
    }
    public GameObject GetEffect(int effectIndex)
    {
        GameObject effect = effectPools[effectIndex].Get().gameObject;
        effect.transform.SetParent(effectHolder);
        return effect;
    }

    public Transform GetEffectHolder() { return effectHolder; }

    public void InitGame()
    {
        playerHP = maxHP;
        enemySpawnTimer = 0;
        stageCount = -1;
        currentWaveTotalEnemyCount = 0;
        currentSpawnGroup = new EnemySpawnGroups(enemySpawnGroups[0].enemySpawnInfos);
        for (int i = 0; i < currentSpawnGroup.enemySpawnInfos.Count; i++)
        {
            currentWaveTotalEnemyCount += currentSpawnGroup.enemySpawnInfos[i].enemyCount;
        }
        LevelGeneration.instance.CreateMap(5); 
        buffingTowers = new List<BuffingTower>();
        towers = new List<Tower>();
        waveCounter = 1;
        playerGold = 900;
        UIManager.UpdateWaveText("Wave: " + waveCounter.ToString());
        UIManager.SetHP(playerHP, maxHP);
        UIManager.SetGold(playerGold);
        UIManager.EnableStartWaveButton();
        gameState = GameState.Planning;
        prevGameState = GameState.Planning;
    }

    public void StartStage()
    {
        stageCount++;
        currentSpawnGroup = new EnemySpawnGroups(enemySpawnGroups[stageCount].enemySpawnInfos);
        for(int i = 0; i < currentSpawnGroup.enemySpawnInfos.Count; i++)
        {
            currentWaveTotalEnemyCount += currentSpawnGroup.enemySpawnInfos[i].enemyCount;
        }
        gameState = GameState.Playing;
        prevGameState = GameState.Playing;
        spawningEnemies = true;
        UIManager.RoundStart(currentWaveTotalEnemyCount * 0.55f);
    }

    public void GivePlayerGold(int goldValue)
    {
        playerGold += goldValue;
        UIManager.SetGold(playerGold);
    }

    public void DamagePopUp(int damage, Vector3 pos)
    {
        DamageNumbers popUp = damagePopUpPool.Get().GetComponent<DamageNumbers>();
        popUp.transform.position = pos;
        popUp.Init(damage);
        popUp.gameObject.SetActive(true);
    }

    public void EnemyKilled(Enemy enemy, int goldValue, Vector3 pos)
    {
        enemies.Remove(enemy);
        playerGold += goldValue;
        UIManager.SetGold(playerGold);
        enemyCount--;
        GoldEarnedPopUp popUp = goldPopUpPool.Get().GetComponent<GoldEarnedPopUp>();
        popUp.transform.position = pos;
        popUp.Init(goldValue);
        popUp.gameObject.SetActive(true);
        if(enemyCount <= 0 && !spawningEnemies)
        {
            StopStage();
        }
    }

    public void EarnGoldPopUp(int goldValue, Vector3 pos)
    {
        GoldEarnedPopUp popUp = goldPopUpPool.Get().GetComponent<GoldEarnedPopUp>();
        popUp.transform.position = pos;
        popUp.Init(goldValue);
        popUp.gameObject.SetActive(true);
    }

    public void EnemyReachedGoal(int damage)
    {
        playerHP -= damage;
        UIManager.SetHP(playerHP, maxHP);
        enemyCount--;
        if(playerHP <= 0)
        {
            //game over lose
        }
        else if(enemyCount <= 0 && !spawningEnemies)
        {
            StopStage();
        }

        GoldEarnedPopUp popUp = HPDamagePool.Get().GetComponent<GoldEarnedPopUp>();
        popUp.transform.position = LevelGeneration.instance.GetCastleTile().transform.position + Vector3.up;
        popUp.InitAsDamage(damage);
        popUp.gameObject.SetActive(true);
    }
    private void StopStage()
    {
        int reward = 20;
        for(int i = 1; i <= waveCounter; i++)
        {
            if(i <= 10)
            {
                if(i % 2 == 0) { reward += 4; }
            }
            else if(i <= 20)
            {
                if (i % 2 == 0) { reward += 2; }
            }
            else if(i <= 30)
            {
                if (i % 2 == 0) { reward += 1; }
            }
            else if(i < 90)
            {
                if (i % 10 == 0) { reward += 1; }
            }
        }
        UIManager.DisplayWaveCompleteInfo(waveCounter, reward);
        GivePlayerGold(reward);
        waveCounter++;
        UIManager.UpdateWaveText("Wave: " + waveCounter.ToString());
        UIManager.RoundEnd();
        ReleaseAllProjectiles();
        gameState = GameState.Planning;
        prevGameState = GameState.Planning;
        LevelGeneration.instance.ExpandMap(waveCounter >= 20 ? 2 : 1);
        currentWaveTotalEnemyCount = 0;
        //UIManager.EnableStartWaveButton();

        foreach(Tower tower in towers)
        {
            tower.OnRoundEnd();
        }
    }

    public void ReleaseAllProjectiles()
    {
        int projectileCount = projectileHolder.childCount;
        for (int i = projectileCount - 1; i >= 0; i--)
        {
            projectileHolder.GetChild(i).GetComponent<Projectile>().KillProjectile();
        }
    }
}
