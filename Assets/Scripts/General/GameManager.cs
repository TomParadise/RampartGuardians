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
    private float enemySpawnTimer = 0;
    public GameState gameState;
    public int stageCount = 0;
    private int enemyCount = 0;

    public int playerHP = 2000;
    public int maxHP = 20;
    public int playerGold = 10000;
    public bool spawningEnemies = true;

    private float currentWaveTotalEnemyCount;

    public List<BuffingTower> buffingTowers = new List<BuffingTower>();
    public List<Tower> towers = new List<Tower>();

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
        Paused
    }

    // Start is called before the first frame update
    void Start()
    {
        InitPools();
        InitGame();
    }

    public void InitPools()
    {
        int length = enemyPrefabs.Length;
        enemyPools = new ObjectPool<PooledObject>[length];
        int[] capacities = new int[12] { 50, 20, 10, 10, 10, 5, 5, 25, 10, 5, 5, 5};
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
        length = effectPrefabs.Length;
        effectPools = new ObjectPool<PooledObject>[length];
        capacities = new int[5] { 5, 5, 10, 10, 5 };
        for (int i = 0; i < length; i++)
        {
            GameObject poolObject = new GameObject(effectPrefabs[i].name + " pool gameobject");
            poolObject.transform.parent = transform.GetChild(0);
            effectPools[i] = poolObject.AddComponent<GenericPool>().Init(effectPrefabs[i], capacities[i]).GetPool();
        }
        GameObject popUpPoolObject = new GameObject(goldPopUpPrefab.name + " pool gameobject");
        popUpPoolObject.transform.parent = transform.GetChild(0);
        goldPopUpPool = popUpPoolObject.AddComponent<GenericPool>().Init(goldPopUpPrefab, 20).GetPool();
        GameObject damagePoolObject = new GameObject(damagePopUpPrefab.name + " pool gameobject");
        damagePoolObject.transform.parent = transform.GetChild(0);
        damagePopUpPool = damagePoolObject.AddComponent<GenericPool>().Init(damagePopUpPrefab, 35).GetPool();
    }


    public bool GetDoesPlayerHaveEnoughGold(int cost) { return playerGold >= cost; }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) { StartStage(); }
        if (gameState == GameState.Paused) { return; }
        EnemySpawnTimer();
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

        enemyCount++;

        currentSpawnGroup.enemySpawnInfos[chosenIndex].enemyCount--;
        if(currentSpawnGroup.enemySpawnInfos[chosenIndex].enemyCount <= 0) { currentSpawnGroup.enemySpawnInfos.RemoveAt(chosenIndex); }
        if(currentSpawnGroup.enemySpawnInfos.Count == 0) { spawningEnemies = false; }
    }
    public void SpawnEnemy(EnemyTypes enemyType, Vector3 pos, Tile goalTile)
    {
        Enemy enemy = enemyPools[(int)enemyType].Get().GetComponent<Enemy>();
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

    public void EnemyKilled(int goldValue, Vector3 pos)
    {
        playerGold += goldValue;
        UIManager.SetGold(playerGold);
        enemyCount--;
        GoldEarnedPopUp popUp = goldPopUpPool.Get().GetComponent<GoldEarnedPopUp>();
        popUp.transform.position = pos;
        popUp.Init(goldValue);
        popUp.gameObject.SetActive(true);
        if(enemyCount <= 0 && !spawningEnemies)
        {
            print("enemy killed stop stage");
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
    }
    private void StopStage()
    {
        print("stop stage");
        int multiple = Mathf.FloorToInt(waveCounter / 10);
        int modulo = Mathf.FloorToInt(waveCounter / 2);
        int reward = 25 + (modulo - multiple) * 5 + 10 * multiple;
        UIManager.DisplayWaveCompleteInfo(waveCounter, reward);
        GivePlayerGold(reward);
        waveCounter++;
        UIManager.UpdateWaveText("Wave: " + waveCounter.ToString());
        UIManager.RoundEnd();
        ReleaseAllProjectiles();
        gameState = GameState.Paused;
        LevelGeneration.instance.ExpandMap();
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
