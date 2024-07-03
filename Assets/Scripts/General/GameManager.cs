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
    private int enemyCount = 0;

    public int playerHP = 2000;
    public int maxHP = 20;
    public int playerGold = 10000;
    public bool spawningEnemies = true;

    private float currentWaveTotalEnemyCount;

    public List<BuffingTower> buffingTowers = new List<BuffingTower>();
    public List<Tower> towers = new List<Tower>();
    public List<Enemy> enemies = new List<Enemy>();

    public int waveCounter = 1;
    private float maxSpawnTimer = 0.55f;

    private int totalGoldEarned = 0;
    private int totalKills = 0;
    private int totalUpgrades = 0;
    private int totalTowers = 0;

    [SerializeField] private AudioClip startWaveSFX;

    private Coroutine victoryFireworksCo = null;

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
        Paused,
        GameOver
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        gameState = GameState.Planning;
        prevGameState = GameState.Planning;
        InitPools();
        InitGame();
    }

    public void InitPools()
    {
        int length = effectPrefabs.Length;
        effectPools = new ObjectPool<PooledObject>[length];
        int[] capacities = new int[10] { 1, 8, 25, 10, 10, 35, 10, 15, 1, 9 };
        for (int i = 0; i < length; i++)
        {
            GameObject poolObject = new GameObject(effectPrefabs[i].name + " pool gameobject");
            poolObject.transform.parent = effectHolder;
            effectPools[i] = poolObject.AddComponent<GenericPool>().Init(effectPrefabs[i], capacities[i]).GetPool();
        }
        length = enemyPrefabs.Length;
        enemyPools = new ObjectPool<PooledObject>[length];
        capacities = new int[12] { 15, 30, 3, 4, 19, 3, 3, 35, 40, 7, 10, 3};
        for (int i = 0; i < length; i++)
        {
            GameObject poolObject = new GameObject(enemyPrefabs[i].name + " pool gameobject");
            poolObject.transform.parent = enemyHolder;
            enemyPools[i] = poolObject.AddComponent<GenericPool>().Init(enemyPrefabs[i], 0).GetPool();
        }
        length = towerPrefabs.Length;
        towerPools = new ObjectPool<PooledObject>[length];
        capacities = new int[8] { 5, 5, 5, 5, 5, 3, 3, 3 };
        for (int i = 0; i < length; i++)
        {
            GameObject poolObject = new GameObject(towerPrefabs[i].name + " pool gameobject");
            poolObject.transform.parent = towerHolder;
            towerPools[i] = poolObject.AddComponent<GenericPool>().Init(towerPrefabs[i], capacities[i]).GetPool();
        }
        length = projectilePrefabs.Length;
        projectilePools = new ObjectPool<PooledObject>[length];
        capacities = new int[3] { 5, 5, 15 };
        for (int i = 0; i < length; i++)
        {
            GameObject poolObject = new GameObject(projectilePrefabs[i].name + " pool gameobject");
            poolObject.transform.parent = projectileHolder;
            projectilePools[i] = poolObject.AddComponent<GenericPool>().Init(projectilePrefabs[i], capacities[i]).GetPool();
        }
        GameObject popUpPoolObject = new GameObject(goldPopUpPrefab.name + " pool gameobject");
        popUpPoolObject.transform.parent = effectHolder;
        goldPopUpPool = popUpPoolObject.AddComponent<GenericPool>().Init(goldPopUpPrefab, 10).GetPool();
        GameObject damagePoolObject = new GameObject(damagePopUpPrefab.name + " pool gameobject");
        damagePoolObject.transform.parent = effectHolder;
        damagePopUpPool = damagePoolObject.AddComponent<GenericPool>().Init(damagePopUpPrefab, 20).GetPool();
        GameObject HPDamageObject = new GameObject(HPDamagePrefab.name + " pool gameobject");
        HPDamageObject.transform.parent = effectHolder;
        HPDamagePool = HPDamageObject.AddComponent<GenericPool>().Init(HPDamagePrefab, 10).GetPool();
    }

    public void TowerUpgraded(int upgrades) { totalUpgrades += upgrades; }
    public void AddTower() { totalTowers++; }

    public bool GetDoesPlayerHaveEnoughGold(int cost) { return playerGold >= cost; }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
        {
            //if (Input.GetKeyDown(KeyCode.V))
            //{
            //    //LevelGeneration.instance.ExpandMap(100);
            //    OnVictory();
            //}
            //if (Input.GetKeyDown(KeyCode.D))
            //{
            //    //LevelGeneration.instance.ExpandMap(100);
            //    OnDefeat();
            //}
            if (Input.GetKeyDown(KeyCode.N))
            {
                StopStage();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            //LevelGeneration.instance.ExpandMap(100);
            //OnVictory();
            //StopStage(); 
            TogglePause();
        }
        if (gameState != GameState.Playing) { return; }
        EnemySpawnTimer();
    }

    public void TogglePause()
    {
        if(UIManager.GetPauseMenu().GetIsConfirmMenuOpen())
        {
            UIManager.GetPauseMenu().CancelRestartGame();
            return;
        }
        if(gameState == GameState.GameOver) { return; }
        if(gameState == GameState.Paused) 
        { 
            gameState = prevGameState;
            AudioManager.instance.UnPauseAudio();
            UIManager.ClosePauseUI();
        }
        else 
        { 
            gameState = GameState.Paused;
            AudioManager.instance.PauseAudio();
            UIManager.OpenPauseUI();
        }
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
                enemySpawnTimer = maxSpawnTimer + RandomHelpers.GenerateRandomSign() * Random.Range(0f, 0.05f);
            }
        }
    }

    public void SpawnEnemy()
    {
        int chosenIndex = -1;

        float rand = Random.value;
        float totalChance = 0;
        //prioritise spawning large enemies e.g. golems and necromancers before other groups
        for (int i = 0; i < currentSpawnGroup.enemySpawnInfos.Count; i++)
        {
            EnemySpawnInfo info = currentSpawnGroup.enemySpawnInfos[i];
            if (info.enemyType == EnemyTypes.Golem 
                || info.enemyType == EnemyTypes.EliteGolem 
                || info.enemyType == EnemyTypes.Necromancer)
            {
                chosenIndex = i;
            }
        }
        //if there are no large enemies, randomly pick an enemy from all the groups to spawn
        if (chosenIndex == -1)
        {
            for (int i = 0; i < currentSpawnGroup.enemySpawnInfos.Count; i++)
            {
                totalChance += currentSpawnGroup.enemySpawnInfos[i].enemyCount / currentWaveTotalEnemyCount;
                if (rand <= totalChance)
                {
                    chosenIndex = i;
                    break;
                }
            }
        }

        Enemy enemy = enemyPools[(int)currentSpawnGroup.enemySpawnInfos[chosenIndex].enemyType].Get().GetComponent<Enemy>();
        enemy.transform.SetParent(enemyHolder);
        Tile spawnTile = LevelGeneration.instance.GetSpawnTile();
        enemy.transform.position = spawnTile.transform.position;
        enemy.Init(spawnTile);
        enemy.gameObject.SetActive(true);
        enemies.Add(enemy);
        enemyCount++;

        currentSpawnGroup.enemySpawnInfos[chosenIndex].enemyCount--;
        currentWaveTotalEnemyCount--;
        if (currentSpawnGroup.enemySpawnInfos[chosenIndex].enemyCount <= 0) { currentSpawnGroup.enemySpawnInfos.RemoveAt(chosenIndex); }
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

        foreach(BuffingTower buffingTower in buffingTowers)
        {
            buffingTower.CheckThenBuff(tower);
        }
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
    public Transform GetTowerHolder() { return towerHolder; }

    public void InitGame()
    {
        if(victoryFireworksCo != null) { StopCoroutine(victoryFireworksCo); }
        victoryFireworksCo = null;
        camMovement.canMove = true;

        playerHP = maxHP;
        enemySpawnTimer = 0;
        currentWaveTotalEnemyCount = 0;
        LevelGeneration.instance.CreateMap(5); 
        buffingTowers = new List<BuffingTower>();
        towers = new List<Tower>();
        waveCounter = 1;
        playerGold = 1000;
        UIManager.UpdateWaveText("Wave: " + waveCounter.ToString());
        UIManager.SetHP(playerHP, maxHP);
        UIManager.SetGold(playerGold);
        UIManager.EnableStartWaveButton();
        UIManager.ForceStopWavePopup();
        gameState = GameState.Planning;
        prevGameState = GameState.Planning;

        totalGoldEarned = 0;
        totalKills = 0;
        totalUpgrades = 0;
        totalTowers = 0;

        enemyCount = 0;

        AudioManager.instance.TransitionMusic(AudioManager.MusicState.Planning, 1, false);

        ReleaseAllProjectiles();
        ReleaseAllEnemies();
    }

    public void StartStage()
    {
        currentSpawnGroup = new EnemySpawnGroups(enemySpawnGroups[waveCounter - 1].enemySpawnInfos);
        for(int i = 0; i < currentSpawnGroup.enemySpawnInfos.Count; i++)
        {
            currentWaveTotalEnemyCount += currentSpawnGroup.enemySpawnInfos[i].enemyCount;
        }
        gameState = GameState.Playing;
        prevGameState = GameState.Playing;
        spawningEnemies = true;

        maxSpawnTimer = Mathf.Lerp(0.6f, 0.35f, currentWaveTotalEnemyCount / 30);
        maxSpawnTimer = Mathf.Round(maxSpawnTimer * 16.666f) / 16.666f;

        UIManager.RoundStart(currentWaveTotalEnemyCount * maxSpawnTimer);


        AudioManager.instance.TransitionMusic(AudioManager.MusicState.Playing, waveCounter);
        AudioManager.instance.PlaySFX(startWaveSFX);

        foreach(Tower tower in towers)
        {
            tower.OnRoundStart();
        }
    }

    public bool GetIsWaveStarted()
    {
        return gameState == GameState.Playing || (gameState == GameState.Paused && prevGameState == GameState.Playing);
    }

    public bool GetIsGamePlayingOrPlanning()
    {
        return gameState == GameState.Playing || gameState == GameState.Planning;
    }

    public void GivePlayerGold(int goldValue)
    {
        totalGoldEarned += goldValue;

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
        totalKills++;

        enemies.Remove(enemy);
        playerGold += goldValue;
        UIManager.SetGold(playerGold);
        enemyCount--;
        GoldEarnedPopUp popUp = goldPopUpPool.Get().GetComponent<GoldEarnedPopUp>();
        popUp.transform.position = pos;
        popUp.Init(goldValue, 1f);
        popUp.gameObject.SetActive(true);
        if(enemyCount <= 0 && !spawningEnemies)
        {
            StopStage();
        }
    }

    public void EarnGoldPopUp(int goldValue, Vector3 pos, float timer = 1f)
    {
        GoldEarnedPopUp popUp = goldPopUpPool.Get().GetComponent<GoldEarnedPopUp>();
        popUp.transform.position = pos;
        popUp.Init(goldValue, timer);
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
            OnDefeat();
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
            if(i <= 16)
            {
                reward += 5;
            }
            else if(i <= 30)
            {
                if (i % 2 == 0) { reward += 5; }
            }
            else if(i <= 45)
            {
                if (i % 5 == 0) { reward += 5; }
            }
        }
        UIManager.DisplayWaveCompleteInfo(waveCounter, reward);
        GivePlayerGold(reward);
        waveCounter++;
        string waveText = "Wave: " + waveCounter.ToString();
        if(waveCounter == 50) { waveText = "Final wave"; }
        UIManager.UpdateWaveText(waveText);
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

        AudioManager.instance.TransitionMusic(AudioManager.MusicState.Planning, waveCounter);

        if(waveCounter == 51)
        {
            OnVictory();
        }
    }

    public void ReleaseAllProjectiles()
    {
        int projectileCount = projectileHolder.childCount;
        for (int i = projectileCount - 1; i >= 0; i--)
        {
            Projectile projectile = projectileHolder.GetChild(i).GetComponent<Projectile>();
            if (projectile == null) { continue; }
            projectile.ForceRelease();
        }
    }

    public void ReleaseAllEnemies()
    {
        int enemyCount = enemyHolder.childCount;
        for (int i = enemyCount - 1; i >= 0; i--)
        {
            Enemy enemy = enemyHolder.GetChild(i).GetComponent<Enemy>();
            if (enemy == null) { continue; }
            enemy.ForceRelease();
        }
    }

    public void OnVictory()
    {
        AudioManager.instance.TransitionMusic(AudioManager.MusicState.Victory, waveCounter);
        gameState = GameState.GameOver;
        camMovement.canMove = false;

        foreach (Tower tower in towers)
        {
            tower.TogglePause(true);
        }
        foreach (Enemy enemy in enemies)
        {
            enemy.TogglePause(true);
        }

        victoryFireworksCo = StartCoroutine(VictoryCo());
    }

    private IEnumerator VictoryCo()
    {
        float timer = 3f;

        GameObject castleTile = LevelGeneration.instance.GetCastleTile().transform.GetChild(0).gameObject;
        float rot = castleTile.transform.eulerAngles.y;
        if (rot < 0) { rot += 360; }
        Vector3 goalPos = new Vector3(1, 4, 4);
        Quaternion goalRot = Quaternion.Euler(new Vector3(45, 180, 0));
        if (rot == 90)
        {
            goalPos = new Vector3(4, 4, 1);
            goalRot = Quaternion.Euler(new Vector3(45, -90, 0));
        }
        else if (rot == 180)
        {
            goalPos = new Vector3(1, 4, -2);
            goalRot = Quaternion.Euler(new Vector3(45, 0, 0));
        }
        else if (rot == 270)
        {
            goalPos = new Vector3(-2, 4, 1);
            goalRot = Quaternion.Euler(new Vector3(45, 90, 0));
        }
        Camera cam = Camera.main;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        while (timer > 0)
        {
            while (gameState == GameState.Paused) { yield return null; }
            cam.transform.position = Vector3.Lerp(goalPos, startPos, timer / 3);
            cam.transform.rotation = Quaternion.Lerp(goalRot, startRot, timer / 3);
            timer -= Time.deltaTime;
            yield return null;
        }

        cam.transform.position = goalPos;
        cam.transform.rotation = goalRot;
        float totalTimer = 0;
        while(true)
        {
            timer = Random.Range(0.2f, 0.55f);
            while (timer > 0)
            {
                while (gameState == GameState.Paused) { yield return null; }
                timer -= Time.deltaTime;
                if (totalTimer >= 0) { totalTimer += Time.deltaTime; }
                yield return null;
            }
            GameObject firework = GetEffect(9);
            firework.transform.position = castleTile.transform.position + Vector3.up * 0.5f;
            firework.gameObject.SetActive(true);
            if(totalTimer > 4)
            {
                totalTimer = -1;
                UIManager.GetGameOverUI().Init(true, waveCounter - 1, totalKills, totalGoldEarned, towers.Count, totalUpgrades);
            }
        }
    }

    public void OnDefeat()
    {
        AudioManager.instance.TransitionMusic(AudioManager.MusicState.Defeat, waveCounter);
        gameState = GameState.GameOver;
        camMovement.canMove = false;

        foreach (Tower tower in towers)
        {
            tower.TogglePause(true);
        }
        foreach (Enemy enemy in enemies)
        {
            enemy.TogglePause(true);
        }

        StartCoroutine(DefeatCo());
    }

    private IEnumerator DefeatCo()
    {
        float timer = 3f;

        GameObject castleTile = LevelGeneration.instance.GetCastleTile().transform.GetChild(0).gameObject;
        float rot = castleTile.transform.eulerAngles.y;
        if(rot < 0) { rot += 360; }
        Vector3 goalPos = new Vector3(2, 2, 0);
        Quaternion goalRot = Quaternion.Euler(new Vector3(35, -45, 0));
        if(rot == 0 || rot == 270)
        {
            goalPos = new Vector3(0, 2, 2);
            goalRot = Quaternion.Euler(new Vector3(35, 135, 0));
        }
        Camera cam = Camera.main;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        while(timer > 0)
        {
            while(gameState == GameState.Paused) { yield return null; }
            cam.transform.position = Vector3.Lerp(goalPos, startPos, timer / 3);
            cam.transform.rotation = Quaternion.Lerp(goalRot, startRot, timer / 3);
            timer -= Time.deltaTime;
            yield return null;
        }

        cam.transform.position = goalPos;
        cam.transform.rotation = goalRot;
        timer = 1f;
        while (timer > 0)
        {
            while (gameState == GameState.Paused) { yield return null; }
            timer -= Time.deltaTime;
            yield return null;
        }

        GameObject castleExplosion = GetEffect(8);
        castleExplosion.transform.position = castleTile.transform.position + Vector3.up * 0.75f;
        castleExplosion.gameObject.SetActive(true);
        castleExplosion = GetEffect(1);
        castleExplosion.gameObject.SetActive(true);
        castleTile.transform.GetChild(1).gameObject.SetActive(false);
        castleTile.transform.GetChild(2).gameObject.SetActive(true);

        timer = 2f;
        while (timer > 0)
        {
            while (gameState == GameState.Paused) { yield return null; }
            timer -= Time.deltaTime;
            yield return null;
        }

        UIManager.GetGameOverUI().Init(false, waveCounter - 1, totalKills, totalGoldEarned, totalTowers, totalUpgrades);
    }

    public void ToggleAllBuffingTowerAreas(bool enabled)
    {
        foreach(BuffingTower buffingTower in buffingTowers)
        {
            buffingTower.ToggleRadius(enabled);
        }
    }
}
