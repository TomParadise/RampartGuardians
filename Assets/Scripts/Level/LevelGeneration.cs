using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class LevelGeneration : MonoSingleton<LevelGeneration>
{
    public enum TileTypes
    {
        Castle,
        Blank,
        RoadStraight,
        RoadCorner,
        RoadThreeway,
    }
    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    [SerializeField] private Transform tileHolder;
    [SerializeField] private GameObject baseTilePrefab;
    [SerializeField] private GameObject[] tilePrefabs;
    private int gridWidth = 5;
    private int gridHeight = 5;

    private TempTile[][] tempTiles;
    public List<Tile> endTiles;

    private List<Tile> gridTiles;
    private int expansionCount = 0;
    private int spawnerIndex = 0;
    public bool junction = true;

    public Vector2 prepends;
    public Vector2 appends;

    private List<TowerPositioner> towerPositioners = new List<TowerPositioner>();
    private List<TowerPositioner> utilityPositioners = new List<TowerPositioner>();

    public List<Tile> spawnTiles;
    public BlankTile[][] blankTiles;
    public int spawnerCounter = 0;

    [SerializeField] private CameraMovement cameraMovement;

    private GenericPool spawnTilePool;
    private GenericPool finalSpawnTilePool;

    [SerializeField] private TileDetails[] detailPrefabs;


    [System.Serializable]
    private struct TileDetails
    {
        public TileDetails(GameObject prefab, float chance)
        {
            this.detailPrefab = prefab;
            this.percentChance = chance;
        }
        public GameObject detailPrefab;
        public float percentChance;
    }


    private void Start()
    {
        GameObject spawnPoolObject = new GameObject(tilePrefabs[5].name + " pool gameobject");
        spawnPoolObject.transform.parent = transform;
        spawnTilePool = spawnPoolObject.AddComponent<GenericPool>().Init(tilePrefabs[5], 4);
        GameObject finalSpawnPoolObject = new GameObject(tilePrefabs[6].name + " pool gameobject");
        finalSpawnPoolObject.transform.parent = transform;
        finalSpawnTilePool = finalSpawnPoolObject.AddComponent<GenericPool>().Init(tilePrefabs[6], 4);
    }

    public void AddTowerPositioner(TowerPositioner towerPositioner, bool utility) 
    { 
        if(utility)
        {
            if (!utilityPositioners.Contains(towerPositioner))
            {
                utilityPositioners.Add(towerPositioner);
            }
        }
        else
        {
            if (!towerPositioners.Contains(towerPositioner))
            {
                towerPositioners.Add(towerPositioner);
            }
        }
    }


    public void InitFirstPath()
    {
        prepends = Vector2.zero;
        appends = Vector2.zero;
        expansionCount = 0;
        endTiles = new List<Tile>();
        junction = true;
        Random.InitState(System.DateTime.Now.Millisecond);

        towerPositioners = new List<TowerPositioner>();
        utilityPositioners = new List<TowerPositioner>();
        spawnTiles = new List<Tile>();
        spawnerCounter = 0;

        gridWidth = 5;
        gridHeight = 5;
        float rand = Random.value;
        int x;
        int y;
        Vector2 endTile;
        if (rand <= 0.33f)
        {
            x = 0;
            y = RandomHelpers.GenerateRandomList(1, gridHeight - 1, Mathf.FloorToInt(gridHeight / 2))[0];
            endTile.x = -1;
            endTile.y = y;
        }
        else if (rand <= 0.66f)
        {
            x = gridWidth - 1;
            y = RandomHelpers.GenerateRandomList(1, gridHeight - 1, Mathf.FloorToInt(gridHeight / 2))[0];
            endTile.x = gridWidth;
            endTile.y = y;
        }
        else
        {
            x = RandomHelpers.GenerateRandomList(1, gridWidth - 1, Mathf.FloorToInt(gridWidth / 2))[0];
            y = gridHeight - 1;
            endTile.x = x;
            endTile.y = gridHeight;
        }
        //print("Destination = " + x + ", " + y);


        tempTiles = new TempTile[gridHeight][];
        for (int i = 0; i < tempTiles.Length; i++)
        {
            tempTiles[i] = new TempTile[gridWidth];
            for (int j = 0; j < tempTiles[i].Length; j++)
            {
                TempTile tile = new TempTile();
                tile.coordinates = new Vector2(j, i);
                tile.costFromStart = Mathf.Abs(1 - j) + Mathf.Abs(1 - i);
                tile.costToEnd = Mathf.Abs(x - j) + Mathf.Abs(y - i);
                tempTiles[i][j] = tile;
            }
        }

        //spawn castle tile at 1,1
        Tile spawnTile = Instantiate(baseTilePrefab, tileHolder).GetComponent<Tile>();
        spawnTile.Init(new Vector2(1, 1), prepends, appends, spawnTile);

        gridTiles = new List<Tile>();
        gridTiles.Add(spawnTile);

        tempTiles[1][1].parentTile = tempTiles[1][1];
        tempTiles[1][1].isOccupied = true;
        //StartCoroutine(FindAPathToPoint(tempTiles[1][1], tempTiles[y][x]));
        List<TempTile> path = FindPathToPoint(tempTiles[1][1], tempTiles[y][x], 7, -Vector2.one, -Vector2.one, 0);

        //for (int i = tempTiles.Length - 1; i >= 0; i--)
        //{
        //    string row = "";
        //    for (int j = 0; j < tempTiles[i].Length; j++)
        //    {
        //        row += tempTiles[i][j].costFromStart + " | ";
        //    }
        //    print(row);
        //}

        for (int i = path.Count - 1; i >= 0; i--)
        {
            Tile tile = Instantiate(baseTilePrefab, tileHolder).GetComponent<Tile>();
            tile.Init(path[i].coordinates, prepends, appends, gridTiles[gridTiles.Count - 1]);
            tile.costFromStart = tempTiles[(int)path[i].coordinates.y][(int)path[i].coordinates.x].costFromStart;
            tile.costToEnd = tempTiles[(int)path[i].coordinates.y][(int)path[i].coordinates.x].costToEnd;

            gridTiles[gridTiles.Count - 1].childTiles.Add(tile);

            gridTiles.Add(tile);

        }

        Tile nextTile = Instantiate(baseTilePrefab, tileHolder).GetComponent<Tile>();
        nextTile.InitIgnorePrependCoords(endTile, prepends, appends, gridTiles[gridTiles.Count - 1]);
        nextTile.costFromStart = 0;
        nextTile.originalCoordinates = endTile;
        nextTile.originalGridSize = new Vector2(gridWidth, gridHeight);
        gridTiles.Add(nextTile);
        endTiles.Add(nextTile);
    }

    public void IncreaseMapSize()
    {
        //add 2 tiles in the direction that the end tile is in
        //50/50 chance to turn left or right and add 2 tiles in that direction
        expansionCount++;
        //get the current end tile that has the shortest path
        Tile startTile = endTiles[0];
        int bestDist = 10000;
        int bestPathJunctionCount = 10000;
        for (int i = 0; i < endTiles.Count; i++)
        {
            int junctionCount = 0;
            Tile parent = endTiles[i].parentTile;
            int dist = 0;
            while (parent != gridTiles[0])
            {
                if (parent.childTiles.Count > 1)
                {
                    junctionCount++;
                }
                dist++;
                parent = parent.parentTile;
            }
            if ((dist < bestDist
                && junctionCount <= bestPathJunctionCount) ||
                junctionCount < bestPathJunctionCount)
            {
                bestDist = dist;
                startTile = endTiles[i];
                bestPathJunctionCount = junctionCount;
            }
        }

        bool xPrepend = false;
        bool yPrepend = false;

        Vector2 coords = startTile.originalCoordinates;
        Vector2 origin = startTile.coordinates;
        if (startTile.prepends.x > prepends.x) { origin.x += startTile.prepends.x - prepends.x; }
        if (startTile.prepends.y > prepends.y) { origin.y += startTile.prepends.y - prepends.y; }
        //expand left or right
        if (coords.x < 0 || coords.x == startTile.originalGridSize.x)
        {
            xPrepend = coords.x < 0;
            yPrepend = junction ? (origin.y < gridHeight / 2) : (origin.y < gridHeight / 2);
            if(!junction)
            {
                foreach(var tile in endTiles)
                {
                    if(tile != startTile)
                    {
                        if((((xPrepend && tile.originalCoordinates.x < 0) || (!xPrepend && tile.originalCoordinates.x == tile.originalGridSize.x)) 
                            && (yPrepend && tile.originalCoordinates.y < origin.y) || (!yPrepend && tile.originalGridSize.y > origin.y))) 
                        {
                            yPrepend = !yPrepend;
                            break; 
                        }
                    }
                }
            }
            if(startTile.forceBlockYPrepend != 0) { yPrepend = startTile.forceBlockYPrepend == 1; }
        }
        else if (coords.y < 0 || coords.y == startTile.originalGridSize.y) //expand down or up
        {
            yPrepend = coords.y < 0;
            xPrepend = junction ? (origin.x < gridWidth / 2) : (origin.x < gridWidth / 2);
            if (!junction)
            {
                foreach (var tile in endTiles)
                {
                    if (tile != startTile)
                    {
                        if ((((yPrepend && tile.originalCoordinates.y < 0) || (!yPrepend && tile.originalCoordinates.y == tile.originalGridSize.y))
                            && (xPrepend && tile.originalCoordinates.x < origin.x) || (!xPrepend && tile.originalGridSize.x > origin.x)))
                        {
                            xPrepend = !xPrepend;
                            break;
                        }
                    }
                }
            }
            if (startTile.forceBlockXPrepend != 0) { xPrepend = startTile.forceBlockXPrepend == 1; }
        }

        //generate random path of length 7 + 2*expansionCount

        if (xPrepend) { origin.x += 2; }
        if (yPrepend) { origin.y += 2; }
        //print("origin = " + origin.x + " , " + origin.y);

        prepends.x -= xPrepend ? 2 : 0;
        prepends.y -= yPrepend ? 2 : 0;
        appends.x += xPrepend ? 0 : 2;
        appends.y += yPrepend ? 0 : 2;

        gridWidth += 2;
        gridHeight += 2;

        //EXPAND THE GRID
        TempTile[][] newTiles = new TempTile[gridHeight][];
        for (int i = 0; i < newTiles.Length; i++)
        {
            newTiles[i] = new TempTile[gridWidth];
            for (int j = 0; j < newTiles[i].Length; j++)
            {
                if ((yPrepend && i < 2) || (!yPrepend && i >= gridHeight - 2) ||
                    (xPrepend && j < 2) || (!xPrepend && j >= gridHeight - 2))
                {
                    //new  tiles that need to be added
                    TempTile tile = new TempTile();
                    tile.coordinates = new Vector2(j, i);
                    newTiles[i][j] = tile;
                }
                else // existing tiles in the old array
                {
                    TempTile tempTile = tempTiles[i - (yPrepend ? 2 : 0)][j - (xPrepend ? 2 : 0)];
                    tempTile.coordinates += new Vector2(xPrepend ? 2 : 0, yPrepend ? 2 : 0);
                    newTiles[i][j] = tempTile;
                }
            }
        }
        tempTiles = newTiles;

        //for (int i = 0; i < gridTiles.Count; i++)
        //{
        //    gridTiles[i].ReInitPrepends(prepends);
        //}

        int pathCount = junction ? 2 : 1;
        Vector2 previousGridMins = new Vector2(xPrepend ? 2 : 0, yPrepend ? 2 : 0);
        Vector2 previousGridMaxs = new Vector2(xPrepend ? gridWidth : gridWidth - 2, yPrepend ? gridHeight : gridHeight - 2);
        for (int pathIndex = 0; pathIndex < pathCount; pathIndex++)
        {
            TempTile forcedStartTile = null;
            List<TempTile> forceIgnoreTiles = new List<TempTile>();
            //add all other end tiles to the ignore list so this path does not cross a future path
            int x = 0;
            int y = 0;
            int desiredDistance = 0;
            Vector2 newEndTile = Vector2.zero;
            //print(previousGridMins);
            //print(previousGridMaxs);
            if (pathCount > 1)
            {//THIS TILE WILL BE A JUNCTION AND SPLIT OFF INTO 2 PATHS
                if (pathIndex == 0)
                {
                    //first path in a junction MUST go on the wall that was prepended/appended
                    if (coords.x < 0 || coords.x == startTile.originalGridSize.x)
                    {//this end tile is on the left/right wall
                        //if this end tile junction is on the centre tile, we need to allow for a smaller path
                        desiredDistance = (int)(startTile.originalGridSize.x + (origin.y == Mathf.FloorToInt(gridHeight / 2) ? -1 : 1));

                        x = xPrepend ? 0 : gridWidth - 1;
                        List<int> ignoreVals = CheckForUnavailableEndSpacesOnNewlyExtendedSide(origin, xPrepend, yPrepend, new List<Tile> { startTile }, false);
                        //foreach(int i in ignoreVals) { print("IGNORE " + i); }
                        ignoreVals.Add((int)origin.y);

                        int xDist = (int)Mathf.Abs(x - origin.x) + 1;
                        //print("distance " + xDist);
                        for (int i = 0; i < gridHeight; i++)
                        {
                            //print("distance for " + i + " = " + (xDist + (int)Mathf.Abs(i - origin.y)) + (xDist + (int)Mathf.Abs(i - origin.y) > desiredDistance ? " IGNORE" : " ALLOW"));
                            if (xDist + (int)Mathf.Abs(i - origin.y) > desiredDistance) { ignoreVals.Add(i); }
                        }
                        //for (int i = 0; i < ignoreVals.Count; i++)
                        //{
                        //    print("IGNORE " + i);
                        //}
                        y = RandomHelpers.GenerateRandomList(1, gridHeight - 1, ignoreVals)[0];

                        newEndTile.x = xPrepend ? -1 : gridWidth;
                        newEndTile.y = y;

                        //Force this tile to go in the opposite direction to the grid y expansion
                        if ((coords.x == startTile.originalGridSize.x && startTile.appends.x != appends.x - (xPrepend ? 0 : 2)) 
                            || (coords.x < 0 && startTile.prepends.x != prepends.x + (xPrepend ? 2 : 0)))
                        {
                            desiredDistance += 2;
                            //forcedStartTile = tempTiles[(int)origin.y][(int)origin.x + (xPrepend ? -1 : 1)];
                            if (xPrepend) { previousGridMins.x += (startTile.prepends.x - prepends.x - 2); }
                            else { previousGridMaxs.x -= (appends.x - startTile.appends.x - 2); }
                            
                            if (yPrepend)
                            {
                                for(int i = (int)origin.y - 1; i >= 0; i--)
                                {
                                    forceIgnoreTiles.Add(tempTiles[i][(int)origin.x]);
                                    startTile.ignoreTiles.Add(new Vector2(origin.x,i));
                                    forceIgnoreTiles.Add(tempTiles[i][(int)origin.x + (xPrepend ? -1 : 1)]);
                                }
                            }
                            else
                            {
                                for (int i = (int)origin.y + 1; i < gridHeight; i++)
                                {
                                    forceIgnoreTiles.Add(tempTiles[i][(int)origin.x]);
                                    startTile.ignoreTiles.Add(new Vector2(origin.x, i));
                                    forceIgnoreTiles.Add(tempTiles[i][(int)origin.x + (xPrepend ? -1 : 1)]);
                                }
                            }
                        }
                        forcedStartTile = tempTiles[(int)origin.y + (yPrepend ? 1 : -1)][(int)origin.x];

                    }
                    else
                    {//this end tile is on the top/bottom wall
                        //if this end tile junction is on the centre tile, we need to allow for a smaller path
                        desiredDistance = (int)(startTile.originalGridSize.y + (origin.x == Mathf.FloorToInt(gridWidth / 2) ? -1 : 1));

                        y = yPrepend ? 0 : gridHeight - 1;
                        List<int> ignoreVals = CheckForUnavailableEndSpacesOnNewlyExtendedSide(origin, xPrepend, yPrepend, new List<Tile> { startTile }, true);
                        ignoreVals.Add((int)origin.x);

                        int yDist = (int)Mathf.Abs(y - origin.y) + 1;
                        //print("distance " + yDist);
                        for (int i = 0; i < gridWidth; i++)
                        {
                            //print("distance for " + i + " = " + (yDist + (int)Mathf.Abs(i - origin.x)) + (yDist + (int)Mathf.Abs(i - origin.x) > desiredDistance ? " IGNORE" : " ALLOW"));
                            if (yDist + (int)Mathf.Abs(i - origin.x) > desiredDistance) { ignoreVals.Add(i); }
                        }
                        //for (int i = 0; i < ignoreVals.Count; i++)
                        //{
                        //    print("IGNORE " + i);
                        //}
                        x = RandomHelpers.GenerateRandomList(1, gridWidth - 1, ignoreVals)[0];
                        newEndTile.x = x;
                        newEndTile.y = yPrepend ? -1 : gridHeight;

                        //Force this tile to go in the opposite direction to the grid y expansion
                        if ((coords.y == startTile.originalGridSize.y && startTile.appends.y != appends.y - (yPrepend ? 0 : 2)) ||
                            (coords.y < 0 && startTile.prepends.y != prepends.y + (yPrepend ? 2 : 0)))
                        {
                            desiredDistance += 2;
                            //forcedStartTile = tempTiles[(int)origin.y + (yPrepend ? -1 : 1)][(int)origin.x];
                            if (yPrepend) { previousGridMins.y += (startTile.prepends.y - prepends.y - 2); }
                            else { previousGridMaxs.y -= (appends.y - startTile.appends.y - 2); }

                            if (xPrepend)
                            {
                                for (int i = (int)origin.x - 1; i >= 0; i--)
                                {
                                    forceIgnoreTiles.Add(tempTiles[(int)origin.y][i]);
                                    startTile.ignoreTiles.Add(new Vector2(i, origin.y));
                                    forceIgnoreTiles.Add(tempTiles[(int)origin.y + (yPrepend ? -1 : 1)][i]);
                                }
                            }
                            else
                            {
                                for (int i = (int)origin.x + 1; i < gridWidth; i++)
                                {
                                    forceIgnoreTiles.Add(tempTiles[(int)origin.y][i]);
                                    startTile.ignoreTiles.Add(new Vector2(i, origin.y));
                                    forceIgnoreTiles.Add(tempTiles[(int)origin.y + (yPrepend ? -1 : 1)][i]);
                                }
                            }
                        }
                        forcedStartTile = tempTiles[(int)origin.y][(int)origin.x + (xPrepend ? 1 : -1)];

                    }
                }
                else
                {
                    //second path in a junction MUST go on the OTHER prepended/appended wall
                    if (coords.x < 0 || coords.x == startTile.originalGridSize.x)
                    {//this end tile is on the top/bottom wall
                        desiredDistance = (int)(startTile.originalGridSize.x + 1);

                        y = yPrepend ? 0 : gridHeight - 1;
                        List<int> ignoreVals = CheckForUnavailableEndSpacesOnNewlyExtendedSide(origin, xPrepend, yPrepend, new List<Tile> { startTile, endTiles[endTiles.Count - 1] }, true);
                        //for (int i = 0; i < ignoreVals.Count; i++)
                        //{
                        //    print("PRE IGNORE " + ignoreVals[i]);
                        //}
                        ignoreVals.Add((int)origin.x);

                        int yDist = (int)Mathf.Abs(y - origin.y);
                        //print("distance " + yDist);
                        for (int i = 0; i < gridWidth; i++)
                        {
                            //print("distance for " + i + " = " + (yDist + (int)Mathf.Abs(i - origin.x)) + (yDist + (int)Mathf.Abs(i - origin.x) > desiredDistance ? " IGNORE" : " ALLOW"));
                            if (yDist + (int)Mathf.Abs(i - origin.x) > desiredDistance) { ignoreVals.Add(i); }
                        }
                        //for (int i = 0; i < ignoreVals.Count; i++)
                        //{
                        //    print("IGNORE " + ignoreVals[i]);
                        //}
                        List<int> possibleXs = RandomHelpers.GenerateRandomList(1, gridWidth - 1, ignoreVals);
                        if (possibleXs.Count == 0) { return; }
                        x = possibleXs[0];
                        newEndTile.x = x;
                        newEndTile.y = yPrepend ? -1 : gridHeight;

                        //grid mins and maxes have already been modified by the first path from this junction
                        //if (ignoreVals.Count > 1)
                        //{
                        //    if (xPrepend) { previousGridMins.x += 2; }
                        //    else { previousGridMaxs.x -= 2; }
                        //}

                        //Force this tile to go in the same direction to the grid y expansion
                        forcedStartTile = tempTiles[(int)origin.y + (yPrepend ? -1 : 1)][(int)origin.x];
                    }
                    else
                    {//this end tile is on the left/right wall
                        desiredDistance = (int)(startTile.originalGridSize.y + 1);

                        x = xPrepend ? 0 : gridWidth - 1;
                        List<int> ignoreVals = CheckForUnavailableEndSpacesOnNewlyExtendedSide(origin, xPrepend, yPrepend, new List<Tile> { startTile, endTiles[endTiles.Count - 1] }, false);

                        //for (int i = 0; i < ignoreVals.Count; i++)
                        //{
                        //    print("PRE IGNORE " + ignoreVals[i]);
                        //}

                        ignoreVals.Add((int)origin.y);

                        int xDist = (int)Mathf.Abs(x - origin.x);
                        //print("distance " + xDist);
                        for (int i = 0; i < gridHeight; i++)
                        {
                            //print("distance for " + i + " = " + (xDist + (int)Mathf.Abs(i - origin.y)) + (xDist + (int)Mathf.Abs(i - origin.y) > desiredDistance ? " IGNORE" : " ALLOW"));
                            if (xDist + (int)Mathf.Abs(i - origin.y) > desiredDistance) { ignoreVals.Add(i); }
                        }
                        //for(int i = 0; i < ignoreVals.Count; i++)
                        //{
                        //    print("IGNORE " + ignoreVals[i]);
                        //}
                        List<int> possibleYs = RandomHelpers.GenerateRandomList(1, gridHeight - 1, ignoreVals);
                        if(possibleYs.Count == 0) { return; }
                        y = possibleYs[0];
                        newEndTile.x = xPrepend ? -1 : gridWidth;
                        newEndTile.y = y;

                        //grid mins and maxes have already been modified by the first path from this junction
                        //if (ignoreVals.Count > 1)
                        //{
                        //    if (yPrepend) { previousGridMins.y += 2; }
                        //    else { previousGridMaxs.y -= 2; }
                        //}

                        //Force this tile to go in the opposite direction to the grid y expansion
                        forcedStartTile = tempTiles[(int)origin.y][(int)origin.x + (xPrepend ? -1 : 1)];
                    }
                }
                if (forcedStartTile != null) { forcedStartTile.isOccupied = true; }
            }
            else
            {//this path is not a junction and will extend normally
                List<Vector2> possiblePositions = new List<Vector2>();
                if(startTile.forceBlockXPrepend != 0 || (startTile.forceBlockXPrepend == 0 && startTile.forceBlockYPrepend == 0))
                {
                    x = xPrepend ? 0 : gridWidth - 1;
                    //Find all y values that would be too far for the given distance and remove them from the possible values
                    List<int> valsTooFar = new List<int>();
                    int dist = (int)Mathf.Abs(x - origin.x);

                    List<int> ignoreVals = CheckForUnavailableEndSpacesOnNewlyExtendedSide(origin, xPrepend, yPrepend, new List<Tile> { startTile }, false);
                    ignoreVals.Add((int)origin.y);
                    for (int i = 0; i < gridHeight; i++)
                    {
                        if (dist + (int)Mathf.Abs(i - origin.y) > (4 + 2 * expansionCount) ||
                            ignoreVals.Contains(i)) { valsTooFar.Add(i); }
                    }
                    valsTooFar.Add((int)origin.y);
                    foreach (int possibleY in RandomHelpers.GenerateRandomList(1, gridHeight - 1, valsTooFar))
                    {
                        possiblePositions.Add(new Vector2(x, possibleY));
                    }
                }
                if(startTile.forceBlockYPrepend != 0 || (startTile.forceBlockXPrepend == 0 && startTile.forceBlockYPrepend == 0))
                {
                    y = yPrepend ? 0 : gridHeight - 1;
                    //Find all x values that would be too far for the given distance and remove them from the possible values
                    List<int> valsTooFar = new List<int>();
                    int dist = (int)Mathf.Abs(y - origin.y);

                    List<int> ignoreVals = CheckForUnavailableEndSpacesOnNewlyExtendedSide(origin, xPrepend, yPrepend, new List<Tile> { startTile }, true);
                    ignoreVals.Add((int)origin.x);
                    for (int i = 0; i < gridWidth; i++)
                    {
                        if (dist + (int)Mathf.Abs(i - origin.x) > (4 + 2 * expansionCount) ||
                            ignoreVals.Contains(i)) { valsTooFar.Add(i); }
                    }
                    valsTooFar.Add((int)origin.x);
                    foreach (int possibleX in RandomHelpers.GenerateRandomList(1, gridWidth - 1, valsTooFar))
                    {
                        possiblePositions.Add(new Vector2(possibleX, y));
                    }
                }

                int randPosIndex = Random.Range(0, possiblePositions.Count);
                x = (int)possiblePositions[randPosIndex].x;
                y = (int)possiblePositions[randPosIndex].y;

                if(x == 0 || x == gridWidth - 1)
                {
                    if (coords.x < 0 || coords.x == startTile.originalGridSize.x)
                    {
                            if (xPrepend) { previousGridMins.x += (startTile.prepends.x - prepends.x - 2); }
                            else { previousGridMaxs.x -= (appends.x - startTile.appends.x - 2); }
                    }
                    else
                    {
                        if (startTile.appends.y != appends.y - (yPrepend ? 0 : 2) || startTile.prepends.y != prepends.y + (yPrepend ? 2 : 0))
                        {
                            if (yPrepend) { previousGridMins.y += (startTile.prepends.y - prepends.y - 2); }
                            else { previousGridMaxs.y -= (appends.y - startTile.appends.y - 2); }
                        }
                    }

                    newEndTile.x = xPrepend ? -1 : gridWidth;
                    newEndTile.y = y;
                }
                else
                {
                    if (coords.x < 0 || coords.x == startTile.originalGridSize.x)
                    {
                        if (xPrepend) { previousGridMins.x += (startTile.prepends.x - prepends.x - 2); }
                        else { previousGridMaxs.x -= (appends.x - startTile.appends.x - 2); }
                    }
                    else
                    {
                        if (yPrepend) { previousGridMins.y += (startTile.prepends.y - prepends.y - 2); }
                        else { previousGridMaxs.y -= (appends.y - startTile.appends.y - 2); }
                    }

                    newEndTile.x = x;
                    newEndTile.y = yPrepend ? -1 : gridHeight;
                }
                desiredDistance = 4 + 2 * expansionCount;
            }
            for (int i = 0; i < endTiles.Count; i++)
            {
                if (endTiles[i] != startTile)
                {
                    if(pathIndex == 1 && endTiles[i] == endTiles[endTiles.Count - 1]) { continue; }
                    Vector2 previousPrepends = new Vector2(Mathf.Abs(endTiles[i].prepends.x - endTiles[i].prepends.x), Mathf.Abs(endTiles[i].prepends.y - endTiles[i].prepends.y)) - new Vector2(xPrepend ? 2 : 0, yPrepend ? 2 : 0);

                    //print("End tile: " + (endTiles[i].originalCoordinates.x - previousPrepends.x + " , " + (endTiles[i].originalCoordinates.y - previousPrepends.y)));
                    //forceIgnoreTiles.Add(tempTiles[(int)(endTiles[i].originalCoordinates.y - prepends.y)][(int)(endTiles[i].originalCoordinates.x - prepends.x)]);

                    if (endTiles[i].originalCoordinates.x < 0)
                    {
                        for (int j = (int)(endTiles[i].originalCoordinates.x - previousPrepends.x); j > 0; j--)
                        {
                            forceIgnoreTiles.Add(tempTiles[(int)(endTiles[i].originalCoordinates.y - previousPrepends.y)][j]);
                            forceIgnoreTiles.Add(tempTiles[(int)(endTiles[i].originalCoordinates.y + 1 - previousPrepends.y)][j]);
                            forceIgnoreTiles.Add(tempTiles[(int)(endTiles[i].originalCoordinates.y - 1 - previousPrepends.y)][j]);
                        }
                    }
                    else if (endTiles[i].originalCoordinates.y < 0)
                    {
                        for (int j = (int)(endTiles[i].originalCoordinates.y - previousPrepends.y); j > 0; j--)
                        {
                            forceIgnoreTiles.Add(tempTiles[j][(int)(endTiles[i].originalCoordinates.x - previousPrepends.x)]);
                            forceIgnoreTiles.Add(tempTiles[j][(int)(endTiles[i].originalCoordinates.x + 1 - previousPrepends.x)]);
                            forceIgnoreTiles.Add(tempTiles[j][(int)(endTiles[i].originalCoordinates.x - 1 - previousPrepends.x)]);
                        }
                    }
                    else if (endTiles[i].originalCoordinates.x == endTiles[i].originalGridSize.x)
                    {
                        for (int j = (int)(endTiles[i].originalCoordinates.x - previousPrepends.x); j < gridWidth - 1; j++)
                        {
                            forceIgnoreTiles.Add(tempTiles[(int)(endTiles[i].originalCoordinates.y - previousPrepends.y)][j]);
                            forceIgnoreTiles.Add(tempTiles[(int)(endTiles[i].originalCoordinates.y + 1 - previousPrepends.y)][j]);
                            forceIgnoreTiles.Add(tempTiles[(int)(endTiles[i].originalCoordinates.y - 1 - previousPrepends.y)][j]);
                        }
                    }
                    else if (endTiles[i].originalCoordinates.y == endTiles[i].originalGridSize.y)
                    {
                        for (int j = (int)(endTiles[i].originalCoordinates.y - previousPrepends.y); j < gridHeight - 1; j++)
                        {
                            forceIgnoreTiles.Add(tempTiles[j][(int)(endTiles[i].originalCoordinates.x - previousPrepends.x)]);
                            forceIgnoreTiles.Add(tempTiles[j][(int)(endTiles[i].originalCoordinates.x + 1 - previousPrepends.x)]);
                            forceIgnoreTiles.Add(tempTiles[j][(int)(endTiles[i].originalCoordinates.x - 1 - previousPrepends.x)]);
                        }
                    }
                    //print(i);
                }
            }
            //print(previousGridMins);
            //print(previousGridMaxs);
            //print("Desired Distance: " + desiredDistance);
            //
            //print("Destination = " + x + ", " + y);


            //Set the costs for the tiles to the new start and end
            for (int i = 0; i < tempTiles.Length; i++)
            {
                for (int j = 0; j < tempTiles[i].Length; j++)
                {
                    tempTiles[i][j].costFromStart = (int)(Mathf.Abs(origin.x - j) + Mathf.Abs(origin.y - i));
                    tempTiles[i][j].costToEnd = Mathf.Abs(x - j) + Mathf.Abs(y - i);
                }
            }



            for (int i = gridHeight - 1; i >= 0; i--)
            {
                string coordinates = "";
                for (int j = 0; j < gridWidth; j++)
                {
                    coordinates += tempTiles[i][j].costToEnd + " | ";
                }
                //print(coordinates);
            }

            tempTiles[(int)origin.y][(int)origin.x].parentTile = tempTiles[(int)origin.y][(int)origin.x];

            tempTiles[(int)origin.y][(int)origin.x].isOccupied = true;
            //StartCoroutine(FindAPathToPoint(tempTiles[(int)origin.y][(int)origin.x], tempTiles[y][x], 7 + 2 * expansionCount));
            List<TempTile> path = FindPathToPoint(tempTiles[(int)origin.y][(int)origin.x], tempTiles[y][x], desiredDistance, previousGridMins, previousGridMaxs, 1, forcedStartTile, forceIgnoreTiles);

            if (path == null)
            {
                IncreaseMapSize();
                if (pathIndex == 1)
                {
                    forcedStartTile.isOccupied = false;
                    return;
                }
                else
                {
                    if (endTiles.Contains(startTile)){ endTiles.Remove(startTile); }
                }
                //Debug.DrawLine(new Vector3(tempTiles[y][x].coordinates.x + prepends.x - 0.15f, 1, tempTiles[y][x].coordinates.y + prepends.y - 0.15f),
                //    new Vector3(tempTiles[y][x].coordinates.x + prepends.x + 0.15f, 1, tempTiles[y][x].coordinates.y + prepends.y + 0.15f), Color.red, 10f);
                //Debug.Break();
            }

            startTile.Init(tempTiles[(int)origin.y][(int)origin.x].coordinates, prepends, appends);
            gridTiles.Add(startTile);
            //OFFSET WORLD POSITIONS RELATIVE TO ANY X / Y PREPENDS
            for (int i = path.Count - 1; i >= 0; i--)
            {
                Tile tile = Instantiate(baseTilePrefab, tileHolder).GetComponent<Tile>();
                tile.Init(path[i].coordinates, prepends, appends, gridTiles[gridTiles.Count - 1]);
                tile.costFromStart = tempTiles[(int)path[i].coordinates.y][(int)path[i].coordinates.x].costFromStart;
                tile.costToEnd = tempTiles[(int)path[i].coordinates.y][(int)path[i].coordinates.x].costToEnd;

                gridTiles[gridTiles.Count - 1].childTiles.Add(tile);

                gridTiles.Add(tile);
            }

            Tile nextTile = Instantiate(baseTilePrefab, tileHolder).GetComponent<Tile>();
            nextTile.InitIgnorePrependCoords(newEndTile, prepends, appends, gridTiles[gridTiles.Count - 1]);
            nextTile.originalCoordinates = newEndTile;
            nextTile.originalGridSize = new Vector2(gridWidth, gridHeight);
            nextTile.costFromStart = 0;
            gridTiles.Add(nextTile);

            if(pathCount > 1)
            {
                if(pathIndex == 0)
                {
                    if(coords.y < 0 || coords.y == startTile.originalGridSize.y)
                    {
                        if (xPrepend) { nextTile.forceBlockXPrepend = -1; }
                        else { nextTile.forceBlockXPrepend = 1; }
                    }
                    else
                    {
                        if (yPrepend) { nextTile.forceBlockYPrepend = -1; }
                        else { nextTile.forceBlockYPrepend = 1; }
                    }
                }
                else if(pathIndex == 1)
                {
                    if(coords.y < 0 || coords.y == startTile.originalGridSize.y)
                    {
                        if (!xPrepend) { nextTile.forceBlockXPrepend = -1; }
                        else { nextTile.forceBlockXPrepend = 1; }
                    }
                    else
                    {
                        if (!yPrepend) { nextTile.forceBlockYPrepend = -1; }
                        else { nextTile.forceBlockYPrepend = 1; }
                    }
                }
            }

            if (endTiles.Contains(startTile)) { endTiles.Remove(startTile); }
            endTiles.Add(nextTile);

            if(pathIndex == 1) 
            {
                if(coords.x < 0 || coords.x == startTile.originalGridSize.x)
                {
                    nextTile.forceBlockYPrepend = yPrepend ? -1 : 1;
                }
                else
                {
                    nextTile.forceBlockXPrepend = xPrepend ? -1 : 1;
                }
            }


            //break;
        }

        for(int y = 0; y < tempTiles.Length; y++)
        {
            for(int x = 0; x < tempTiles[y].Length; x++)
            {
                if (tempTiles[y][x].isOccupied)
                {
                    tempTiles[y][x].previouslyLockedIn = true;
                }
            }
        }

        junction = !junction;
    }

    public void TogglePositioners(bool enabled, bool utility)
    {
        if(utility)
        {
            foreach (TowerPositioner positioner in utilityPositioners)
            {
                positioner.ToggleActive(enabled);
            }
        }
        else
        {
            foreach (TowerPositioner positioner in towerPositioners)
            {
                positioner.ToggleActive(enabled);
            }
        }
    }

    public List<TempTile> FindPathToPoint(TempTile origin, TempTile destination, int idealCost, Vector2 previousMins, Vector2 previousMaxs, int forcedLeniency = 1, TempTile forcedStartTile = null, List<TempTile> forceIgnoreTiles = null)
    {
        //get adjacent tile scores to reach the goal
        //if the score to reach the goal + that tiles current score is within range follow it
        int returnedToStartCount = 0;
        List<TempTile> openList = new List<TempTile>();
        List<TempTile> closedList = new List<TempTile>();
        List<TempTile> currentPath = new List<TempTile>();
        int leniencyAmount = forcedLeniency;
        if (forcedStartTile != null)
        {
            currentPath.Add(origin);
            openList.Add(forcedStartTile);
            forcedStartTile.parentTile = origin;
            forcedStartTile.costFromStart = 1;
        }
        else { openList.Add(origin); }
        while (openList.Count > 0)
        {
            TempTile currentTile = openList[openList.Count - 1];
            openList.RemoveAt(openList.Count - 1);
            if (!currentPath.Contains(currentTile)) { currentPath.Add(currentTile); }
            //print(" moved onto : " + currentTile.coordinates);
            if (currentTile.costFromStart > idealCost + 1)
            {
                closedList.Add(currentTile);
                currentTile.ResetCostToStart(origin.coordinates);
                openList.Add(currentTile.parentTile);
                currentPath.Remove(currentTile);
                continue;
            }

            if (currentTile == destination)
            {
                if (Mathf.Abs(currentTile.costFromStart - idealCost) > leniencyAmount)
                {
                    closedList.Add(currentTile);
                    currentTile.ResetCostToStart(origin.coordinates);
                    openList.Add(currentTile.parentTile);
                    currentPath.Remove(currentTile);
                    continue;
                }
                List<TempTile> path = new List<TempTile>();
                while (currentTile != origin)
                {
                    //print(currentTile.coordinates);
                    path.Add(currentTile);
                    currentTile.isOccupied = true;
                    currentTile = currentTile.parentTile;
                }
                return path;
            }


            List<TempTile> neighbourTiles = FindAdjacentTiles(currentTile, previousMins, previousMaxs);
            bool empty = true;
            foreach (TempTile neighbourTile in neighbourTiles)
            {
                if (closedList.Contains(neighbourTile) || currentPath.Contains(neighbourTile) ||(forceIgnoreTiles != null && forceIgnoreTiles.Contains(neighbourTile))) { continue; }

                int tentativeCost = currentTile.costFromStart + 1;

                //if the new tile IS the destination BUT we have not got enough distance, ignore evaluating it
                if (neighbourTile == destination && Mathf.Abs(tentativeCost - idealCost) > leniencyAmount) { continue; }

                //if the new tile's cost will go over the maximum, ignore this neighbour
                if (tentativeCost + neighbourTile.costToEnd > idealCost + 1) { continue; }

                if (Mathf.Abs(tentativeCost - idealCost) <= Mathf.Abs(neighbourTile.costFromStart - idealCost))
                {
                    neighbourTile.costFromStart = tentativeCost;
                    neighbourTile.parentTile = currentTile;
                    if (openList.Contains(neighbourTile)) { openList.Remove(neighbourTile); }
                    openList.Add(neighbourTile);
                    empty = false;
                }
                else
                {
                    if (!openList.Contains(neighbourTile)) { openList.Add(neighbourTile); }
                }

            }
            if (empty)
            {
                for (int i = closedList.Count - 1; i >= 0; i--)
                {
                    if (!currentPath.Contains(closedList[i].parentTile))
                    {
                        closedList.RemoveAt(i);
                    }
                }
                closedList.Add(currentTile);
                currentTile.ResetCostToStart(origin.coordinates);
                if(currentTile != forcedStartTile) { openList.Add(currentTile.parentTile); }
                else { openList.Add(currentTile); }
                currentPath.Remove(currentTile);
                if (currentTile.parentTile == origin) 
                {
                    returnedToStartCount++;
                }
                if(returnedToStartCount > 3)
                {
                    leniencyAmount += expansionCount;
                    //print("LENIENCY INCREASED TO : " + leniencyAmount);
                    if(leniencyAmount == (expansionCount * 5) + 1)
                    {
                        return null;
                    }
                    else
                    {
                        returnedToStartCount = 0;
                        closedList.Clear();
                    }
                }
            }
        }

        return null;
    }

    public TempTile GetBestCostTile(List<TempTile> tiles, int goalCost)
    {
        TempTile bestCostTile = tiles[0];
        int currentAbsCost = Mathf.Abs(bestCostTile.costToEnd - goalCost);
        for (int i = 1; i < tiles.Count; i++)
        {
            if(Mathf.Abs(tiles[i].costToEnd - goalCost) < currentAbsCost)
            {
                bestCostTile = tiles[i];
                currentAbsCost = Mathf.Abs(bestCostTile.costToEnd - goalCost);
            }
        }
        return bestCostTile;
    }

    private bool GetIsTileAdjacentToPreviousPath(TempTile origin)
    {
        int y = (int)origin.coordinates.y;
        int x = (int)origin.coordinates.x;
        for (int y2 = -1; y2 <= 1; y2++)
        {
            for (int x2 = -1; x2 <= 1; x2++)
            {
                if (y + y2 >= 0 && y + y2 <= gridHeight - 1 &&
                    x + x2 >= 0 && x + x2 <= gridHeight - 1)
                {
                    if (tempTiles[y + y2][x + x2].previouslyLockedIn)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    //get all adjacent tiles and order them by distance to goal
    private List<TempTile> FindAdjacentTiles(TempTile origin, Vector2 ignoreMins, Vector2 ignoreMaxs)
    {
        List<TempTile> tiles = new List<TempTile>();

        Vector2 adjacentCoords = origin.coordinates;
        for(int x = -1; x < 2; x+=2)
        {
            if (adjacentCoords.x + x >= 0 &&
                adjacentCoords.x + x < gridWidth &&
                tempTiles[(int)adjacentCoords.y][(int)adjacentCoords.x + x] != origin &&
                !tempTiles[(int)adjacentCoords.y][(int)adjacentCoords.x + x].isOccupied &&
                (adjacentCoords.x + x < ignoreMins.x || adjacentCoords.x + x >= ignoreMaxs.x 
                || adjacentCoords.y < ignoreMins.y || adjacentCoords.y >= ignoreMaxs.y
                || !GetIsTileAdjacentToPreviousPath(tempTiles[(int)adjacentCoords.y][(int)adjacentCoords.x + x])))
            {
                tiles.Add(tempTiles[(int)adjacentCoords.y][(int)adjacentCoords.x + x]);
            }
        }

        for (int y = -1; y < 2; y += 2)
        {
            if (adjacentCoords.y + y >= 0 &&
                adjacentCoords.y + y < gridHeight &&
                tempTiles[(int)adjacentCoords.y + y][(int)adjacentCoords.x] != origin &&
                !tempTiles[(int)adjacentCoords.y + y][(int)adjacentCoords.x].isOccupied &&
                (adjacentCoords.y + y < ignoreMins.y || adjacentCoords.y + y >= ignoreMaxs.y
                || adjacentCoords.x < ignoreMins.x || adjacentCoords.x >= ignoreMaxs.x
                || !GetIsTileAdjacentToPreviousPath(tempTiles[(int)adjacentCoords.y + y][(int)adjacentCoords.x])))
            {
                tiles.Add(tempTiles[(int)adjacentCoords.y + y][(int)adjacentCoords.x]);
            }
        }

        int listSize = tiles.Count;
        List<TempTile> randomList = new List<TempTile>();
        for (int i = 0; i < listSize; i++)
        {
            var thisNumber = Random.Range(0, tiles.Count);
            randomList.Add(tiles[thisNumber]);
            tiles.RemoveAt(thisNumber);
        }

        TempTile tempTile;
        for (int write = 0; write < listSize; write++)
        {
            for (int sort = 0; sort < listSize - 1; sort++)
            {
                if (randomList[sort].costToEnd > randomList[sort + 1].costToEnd)
                {
                    tempTile = randomList[sort + 1];
                    randomList[sort + 1] = randomList[sort];
                    randomList[sort] = tempTile;
                }
            }
        }

        return randomList;
    }

    //call to get one of the current spawn tiles
    //it is not random, it cycles through all spawners so they get an even amount of enemies spawning between them
    //public Tile GetSpawnTile()
    //{
    //    if(++spawnerIndex >= endTileCoords.Count) { spawnerIndex = 0; }
    //    return gridTiles[endTileCoords[spawnerIndex].y];
    //}

    // Update is called once per frame
    //private float timer = 0;
    //void Update()
    //{
    //    if (Application.isEditor)
    //    {
    //        if (Input.GetKey(KeyCode.Space))
    //        {
    //            timer += Time.deltaTime;
    //            if (timer > 0.1f)
    //            {
    //                timer = 0;
    //                CreateMap(5);
    //                //DestroyAllTiles();
    //                //InitFirstPath();
    //                //SetPathConnections();
    //                //SetPathModelsAndRotations();
    //                //CreateBlankTiles();
    //            }
    //        }
    //        if (Input.GetKeyDown(KeyCode.T))
    //        {
    //            ExpandMap(50);
    //        }
    //        if (Input.GetKeyDown(KeyCode.N))
    //        {
    //            StartCoroutine(GrowMap());
    //        }
    //    }
    //}

    private IEnumerator GrowMap()
    {
        for (int i = 0; i < 50; i++)
        {
            float timer = 0.05f;
            while(timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
            ExpandMap(i > 20 ? 2 : 1);
        }
    }

    public void CreateMap(int expansionCount)
    {
        DestroyAllTiles();
        InitFirstPath();
        for(int i = 0; i < expansionCount; i++)
        {
            IncreaseMapSize();
        }
        AddBufferAroundMap();

        SetPathConnections();
        UpdateTileCost(gridTiles[0]);
        SetPathModelsAndRotations();
        Tile tile = gridTiles[0];
        for(int i = 0; i <= 5; i++)
        {
            tile = tile.childTiles[0];
        }
        spawnTiles.Add(tile);

        foreach(Tile endTile in endTiles)
        {
            if (endTile.prepends != prepends)
            {
                //print(endTile.prepends.y + " - " + prepends.y + " = " + Mathf.Abs(endTile.prepends.y - prepends.y) + " | " + endTile.prepends.x + " - " + prepends.x + " = " + Mathf.Abs(endTile.prepends.x - prepends.x));
                //print(endTile.coordinates.y + " , " + endTile.coordinates.x + " | " + (int)(endTile.coordinates.y + Mathf.Abs(endTile.prepends.y - prepends.y)) + " , " + (int)(endTile.coordinates.x + Mathf.Abs(endTile.prepends.x - prepends.x)));
                int y = (int)(endTile.coordinates.y + Mathf.Abs(endTile.prepends.y - prepends.y));
                int x = (int)(endTile.coordinates.x + Mathf.Abs(endTile.prepends.x - prepends.x));
                if (y >= 0 && y < tempTiles.Length &&
                    x >= 0 && x < tempTiles[y].Length)
                {
                    tempTiles[y][x].isOccupied = true;
                }
            }
        }

        CreateBlankTiles(System.Array.IndexOf(gridTiles.ToArray(), tile));

        DynamicGI.UpdateEnvironment();

        StartCoroutine(HideStartTiles(System.Array.IndexOf(gridTiles.ToArray(), tile)));
        StartCoroutine(DisablePositionersOnStart());

        UpdateCamLimits();
        cameraMovement.ClampPosAndRot();
        cameraMovement.ResetPos();
    }

    private void AddBufferAroundMap()
    {
        prepends.x -= 2;
        prepends.y -= 2;
        appends.x += 2;
        appends.y += 2;

        gridWidth += 4;
        gridHeight += 4;

        //EXPAND THE GRID
        TempTile[][] newTiles = new TempTile[gridHeight][];
        for (int y = 0; y < newTiles.Length; y++)
        {
            newTiles[y] = new TempTile[gridWidth];
            for (int x = 0; x < newTiles[y].Length; x++)
            {
                if ((y < 2) || (y >= gridHeight - 2) ||
                    (x < 2) || (x >= gridWidth - 2))
                {
                    //new  tiles that need to be added
                    TempTile tile = new TempTile();
                    tile.coordinates = new Vector2(x, y);
                    newTiles[y][x] = tile;
                }
                else // existing tiles in the old array
                {
                    TempTile tempTile = tempTiles[y - 2][x - 2];
                    tempTile.coordinates += new Vector2(2, 2);
                    newTiles[y][x] = tempTile;
                }
            }
        }
        tempTiles = newTiles;
    }

    public void UpdateCamLimits()
    {
        Vector2 minimums = cameraMovement.mapMinimums;
        Vector2 maximums = cameraMovement.mapMaximums;
        foreach(Tile tile in spawnTiles)
        {
            Tile parent = tile;
            while(parent.parentTile != parent)
            {
                if(tile.transform.position.x - 2 < minimums.x) { minimums.x = tile.transform.position.x - 2; }
                else if(tile.transform.position.x + 2 > maximums.x) { maximums.x = tile.transform.position.x + 2; }
                if(tile.transform.position.z - 2 < minimums.y) { minimums.y = tile.transform.position.z - 2; }
                else if(tile.transform.position.z + 2 > maximums.y) { maximums.y = tile.transform.position.z + 2; }
                parent = parent.parentTile;
            }
        }
        for(int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (blankTiles[y][x] != null && blankTiles[y][x].bounced)
                {
                    if (blankTiles[y][x].transform.position.x - 2 < minimums.x) { minimums.x = blankTiles[y][x].transform.position.x - 2; }
                    else if (blankTiles[y][x].transform.position.x + 2 > maximums.x) { maximums.x = blankTiles[y][x].transform.position.x + 2; }
                    if (blankTiles[y][x].transform.position.z - 2 < minimums.y) { minimums.y = blankTiles[y][x].transform.position.z - 2; }
                    else if (blankTiles[y][x].transform.position.z + 2 > maximums.y) { maximums.y = blankTiles[y][x].transform.position.z + 2; }
                }
            }
        }
        cameraMovement.mapMinimums = minimums;
        cameraMovement.mapMaximums = maximums;
    }

    private void UpdateTileCost(Tile startTile)
    {
        Tile tile = startTile;
        while(tile.childTiles.Count > 0)
        {
            foreach(Tile child in tile.childTiles)
            {
                child.costFromStart = tile.costFromStart + 1;
            }
            if(tile.childTiles.Count > 1)
            {
                UpdateTileCost(tile.childTiles[1]);
            }
            tile = tile.childTiles[0];
        }
    }

    private IEnumerator DisablePositionersOnStart()
    {
        yield return null;
        TogglePositioners(false, false);
        TogglePositioners(false, true);
    }
    private IEnumerator HideStartTiles(int startIndex)
    {
        yield return null;
        for (int i = 0; i <= startIndex; i++)
        {
            if(i < startIndex) { gridTiles[i].BounceTile(); }            
        }

        gridTiles[startIndex].spawnTile = spawnTilePool.GetPool().Get().GetComponent<BlankTile>();
        gridTiles[startIndex].spawnTile.InitSpawnTile(gridTiles[startIndex].transform.position, gridTiles[startIndex].parentTile.transform.position);

        for (int i = startIndex; i < gridTiles.Count; i++)
        {
            gridTiles[i].transform.localScale = Vector3.zero;
        }
    }

    public void DestroyAllTiles()
    {
        for (int i = tileHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(tileHolder.GetChild(i).gameObject);
        }
        spawnTilePool.ReleaseChildren();
        finalSpawnTilePool.ReleaseChildren();
    }

    //check for any end tiles that are ON the newly appended/prepended tiles,
    //ignore the currently used tile
    //returnX = true or false for wether the values checked should be x values (true) or y values (false)
    public List<int> CheckForUnavailableEndSpacesOnNewlyExtendedSide(Vector2 origin, bool xPrepend, bool yPrepend, List<Tile> ignoreTiles, bool returnX)
    {
        List<int> blockedCoords = new List<int>();

        foreach(Tile tile in endTiles)
        {
            if(ignoreTiles.Contains(tile)) { continue; }
            Vector2 additionalPrepends = new Vector2(xPrepend ? 2 : 0, yPrepend ? 2 : 0);
            if(tile.prepends.x > prepends.x) { additionalPrepends.x = tile.prepends.x - prepends.x; }
            if(tile.prepends.y > prepends.y) { additionalPrepends.y = tile.prepends.y - prepends.y; }
            if (!returnX)
            {
                if ((xPrepend && tile.originalCoordinates.x == -1) ||
                    (!xPrepend && tile.originalCoordinates.x == tile.originalGridSize.x))
                {
                    //all tiles on and above are not valid
                    if (origin.y < tile.coordinates.y + (int)additionalPrepends.y)
                    {
                        for (int i = gridHeight; i >= tile.coordinates.y - 1; i--)
                        {
                            blockedCoords.Add(i + (int)additionalPrepends.y);
                        }
                    }
                    else//all tiles on and below are not valid
                    {
                        for (int i = 0; i <= tile.coordinates.y + 1; i++)
                        {
                            blockedCoords.Add(i + (int)additionalPrepends.y);
                        }
                    }
                }
                if(tile.originalCoordinates.y < 0)
                {
                    for (int i = (int)tile.coordinates.y; i < tile.coordinates.y + 2; i++)
                    {
                        blockedCoords.Add(i + (int)additionalPrepends.y);
                    }
                }
                else if(tile.originalCoordinates.y == tile.originalGridSize.y)
                {
                    for (int i = (int)tile.coordinates.y; i > tile.coordinates.y - 2; i--)
                    {
                        blockedCoords.Add(i + (int)additionalPrepends.y);
                    }
                }
            }
            else
            {
                if ((yPrepend && tile.originalCoordinates.y == -1) ||
                (!yPrepend && tile.originalCoordinates.y == tile.originalGridSize.y))
                {
                    //all tiles on and above are not valid
                    if (origin.x < tile.coordinates.x + (int)additionalPrepends.x)
                    {
                        for (int i = gridWidth; i >= tile.coordinates.x - 1; i--)
                        {
                            blockedCoords.Add(i + (int)additionalPrepends.x);
                        }
                    }
                    else//all tiles on and below are not valid
                    {
                        for (int i = 0; i <= (int)tile.coordinates.x + 1; i++)
                        {
                            blockedCoords.Add(i + (int)additionalPrepends.x);
                        }
                    }
                }
                if (tile.originalCoordinates.x < 0)
                {
                    for (int i = (int)tile.coordinates.x; i < tile.coordinates.x + 2; i++)
                    {
                        blockedCoords.Add(i + (int)additionalPrepends.x);
                    }
                }
                else if (tile.originalCoordinates.x == tile.originalGridSize.x)
                {
                    for (int i = (int)tile.coordinates.x; i > tile.coordinates.x - 2; i--)
                    {
                        blockedCoords.Add(i + (int)additionalPrepends.x);
                    }
                }
            }
        }

        return blockedCoords;
    }

    public void SetPathConnections()
    {
        for(int i = 0; i < endTiles.Count; i++)
        {
            Tile tile = endTiles[i];
            while(tile != gridTiles[0])
            {
                if (!tile.parentTile.childTiles.Contains(tile))
                {
                    tile.parentTile.childTiles.Add(tile);
                }
                tile = tile.parentTile;
            }
        }
    }

    public void CreateBlankTiles(int spawnTileIndex)
    {
        List<int> detailsIndices = RandomHelpers.GenerateRandomListOfCount(0, gridHeight * gridWidth, Mathf.FloorToInt((gridHeight * gridWidth) * 0.35f));
        blankTiles = new BlankTile[gridHeight][];
        for(int i = 0; i < blankTiles.Length; i++)
        {
            blankTiles[i] = new BlankTile[gridWidth];
        }

        for (int y = 0; y < gridHeight; y++)
        {
            for(int x = 0; x < gridWidth; x++)
            {
                if (tempTiles[y][x].isOccupied)
                {
                    for(int y2 = -1; y2 <= 1; y2++)
                    {
                        for (int x2 = -1; x2 <= 1; x2++)
                        {
                            if(y + y2 >= 0 && y + y2 <= gridHeight - 1 &&
                                x + x2 >= 0 && x + x2 <= gridHeight - 1)
                            {
                                if (!tempTiles[y + y2][x + x2].isOccupied &&
                                    blankTiles[y + y2][x + x2] == null)
                                {
                                    BlankTile blankTile = Instantiate(
                                        tilePrefabs[(int)TileTypes.Blank],
                                        new Vector3(x + x2 + prepends.x, 0, y + y2 + prepends.y),
                                        Quaternion.identity, tileHolder)
                                        .GetComponent<BlankTile>();
                                    blankTile.Init(new Vector2(x + x2, y + y2));
                                    if (detailsIndices.Contains((y + y2) * gridWidth + (x + x2)))
                                    {
                                        GameObject detail = null;
                                        float value = Random.value;
                                        for(int i = 0; i < detailPrefabs.Length; i++)
                                        {
                                            if(value <= detailPrefabs[i].percentChance)
                                            {
                                                detail = Instantiate(detailPrefabs[i].detailPrefab);
                                                break;
                                            }
                                        }
                                        if(detail == null) { detail = Instantiate(detailPrefabs[0].detailPrefab); }
                                        blankTile.CreateDetails(detail);
                                    }
                                    else
                                    {
                                        utilityPositioners.Add(blankTile.EnableUtilityPositioner());
                                    }
                                    blankTile.transform.localScale = Vector3.zero;
                                    //blankTile.BounceTile();
                                    blankTiles[y + y2][x + x2] = blankTile;
                                }
                            }
                        }
                    }
                }
            }
        }

        for (int i = 0; i <= spawnTileIndex; i++)
        {
            int yPos = (int)(gridTiles[i].transform.position.z - prepends.y);
            int xPos = (int)(gridTiles[i].transform.position.x - prepends.x);
            for (int y = -1; y < 2; y += 2)
            {
                bool corner = true;
                if (yPos < gridHeight - 1 && yPos > 0 && blankTiles[yPos + y][xPos] != null)
                {
                    blankTiles[yPos + y][xPos].BounceTile();
                }
                else { corner = false; }
                for (int x = -1; x < 2; x += 2)
                {
                    if (xPos < gridWidth - 1 && xPos > 0 && blankTiles[yPos][xPos + x] != null)
                    {
                        blankTiles[yPos][xPos + x].BounceTile();

                        if (yPos < gridHeight - 1 && yPos > 0 && corner && blankTiles[yPos + y][xPos + x] != null)
                        {
                            blankTiles[yPos + y][xPos + x].BounceTile();
                        }
                    }
                }
            }
        }
    }

    public void SetPathModelsAndRotations()
    {
        List<Tile> usedTiles = new List<Tile>();
        List<TileModelInfo> tileModels = new List<TileModelInfo>();
        for (int i = 0; i < endTiles.Count; i++)
        {
            Tile tile = endTiles[i];
            while (tile != gridTiles[0] && !usedTiles.Contains(tile))
            {
                if(tile.childTiles.Count == 1)
                {
                    //straight path tile
                    if((tile.transform.position.x == tile.childTiles[0].transform.position.x && tile.transform.position.x == tile.parentTile.transform.position.x) ||
                        (tile.transform.position.z == tile.childTiles[0].transform.position.z && tile.transform.position.z == tile.parentTile.transform.position.z))
                    {
                        float rot = (tile.transform.position.x == tile.childTiles[0].transform.position.x && tile.transform.position.x == tile.parentTile.transform.position.x) ? 0 : 90;
                        tileModels.Add(Instantiate(tilePrefabs[(int)TileTypes.RoadStraight], tile.transform.position, Quaternion.Euler(0, rot, 0), tile.transform).GetComponent<TileModelInfo>());
                    }
                    else
                    {//corner tile
                        float rot = 0;
                        if((tile.parentTile.transform.position.x > tile.transform.position.x && tile.childTiles[0].transform.position.z > tile.transform.position.z) ||
                            (tile.childTiles[0].transform.position.x > tile.transform.position.x && tile.parentTile.transform.position.z > tile.transform.position.z))
                        {
                            rot = 90;
                        }
                        else if((tile.parentTile.transform.position.x > tile.transform.position.x && tile.childTiles[0].transform.position.z < tile.transform.position.z) ||
                            (tile.childTiles[0].transform.position.x > tile.transform.position.x && tile.parentTile.transform.position.z < tile.transform.position.z))
                        {
                            rot = 180;
                        }
                        else if((tile.parentTile.transform.position.x < tile.transform.position.x && tile.childTiles[0].transform.position.z < tile.transform.position.z) ||
                            (tile.childTiles[0].transform.position.x < tile.transform.position.x && tile.parentTile.transform.position.z < tile.transform.position.z))
                        {
                            rot = 270;
                        }
                        tileModels.Add(Instantiate(tilePrefabs[(int)TileTypes.RoadCorner], tile.transform.position, Quaternion.Euler(0, rot, 0), tile.transform).GetComponent<TileModelInfo>());
                    }
                }
                else if(tile.childTiles.Count == 2)
                {//triple path
                    float rot = TriplePathCheck(tile.transform.position, tile.parentTile.transform.position);
                    tileModels.Add(Instantiate(tilePrefabs[(int)TileTypes.RoadThreeway], tile.transform.position, Quaternion.Euler(0, rot, 0), tile.transform).GetComponent<TileModelInfo>());
                }
                else if(tile.childTiles.Count == 0) 
                { 
                    tileModels.Add(Instantiate(tilePrefabs[(int)TileTypes.RoadStraight], tile.transform.position, Quaternion.Euler(0, 0, 0), tile.transform).GetComponent<TileModelInfo>());
                }
                usedTiles.Add(tile);
                tileModels[tileModels.Count - 1].Init(tile.costFromStart);
                tile = tile.parentTile;
            }
        }
        foreach(TileModelInfo model in tileModels) { model.InitPositioners(); }

        Tile startTile = gridTiles[0];
        float rotation = 0;
        if (startTile.childTiles[0].transform.position.x > startTile.transform.position.x) { rotation = 90; }
        else if (startTile.childTiles[0].transform.position.x < startTile.transform.position.x) { rotation = 270; }
        else if (startTile.childTiles[0].transform.position.z < startTile.transform.position.x) { rotation = 180; }
        Instantiate(tilePrefabs[(int)TileTypes.Castle], startTile.transform.position, Quaternion.Euler(0, rotation, 0), startTile.transform);
    }

    //pass in the positions of the 3 adjacent tiles and the tile they are connecting from
    //returns which direction does not have a connection
    public float TriplePathCheck(Vector3 tile, Vector3 parent)
    {
        if(parent.x < tile.x) { return 270; }
        if(parent.x > tile.x) { return 90; }
        if(parent.z < tile.z) { return 180; }
        return 0;
    }

    public Tile GetSpawnTile()
    {
        if(++spawnerCounter >= spawnTiles.Count) { spawnerCounter = 0; }
        return spawnTiles[spawnerCounter];
    }

    public void ExpandMap(int expansionCount)
    {
        for (int i = 0; i < expansionCount; i++)
        {
            Tile newSpawnTile = null;
            List<Tile> tilesToBounce = new List<Tile>();
            Vector2 minimums = Vector2.zero;
            Vector2 maximums = Vector2.zero;
            int bestTilePathCount = -1;
            for (int spawnTile = 0; spawnTile < spawnTiles.Count; spawnTile++)
            {
                if(spawnTiles[spawnTile].childTiles.Count == 0) { continue; }
                int tilePathCount = 1;
                Tile parent = spawnTiles[spawnTile].parentTile;
                while (parent.parentTile != parent)
                {
                    tilePathCount++;
                    parent = parent.parentTile;
                }
                if (newSpawnTile == null || tilePathCount < bestTilePathCount)
                {
                    newSpawnTile = spawnTiles[spawnTile];
                    bestTilePathCount = tilePathCount;
                }
            }
            if(newSpawnTile == null) { continue; }
            Tile tile = newSpawnTile;
            spawnTiles.Remove(newSpawnTile);
            for (int childTile = 0; childTile < tile.childTiles.Count; childTile++)
            {
                Tile child = tile.childTiles[childTile];
                tilesToBounce.Add(child);
                spawnTiles.Add(child);
                if (child.prepends.x < minimums.x) { minimums.x = child.prepends.x; }
                if (child.prepends.y < minimums.y) { minimums.x = child.prepends.y; }
                if (child.appends.x > maximums.x) { maximums.x = child.appends.x; }
                if (child.appends.y > maximums.y) { maximums.x = child.appends.y; }
            }

            tile.spawnTile.Release();
            tile.spawnTile = null;
            tile.InstantBounce();

            foreach (Tile bounce in tilesToBounce)
            {
                //bounce.BounceTile(); 
                if(bounce.childTiles.Count > 0)
                {
                    bounce.spawnTile = spawnTilePool.GetPool().Get().GetComponent<BlankTile>();
                }
                else
                {
                    bounce.spawnTile = finalSpawnTilePool.GetPool().Get().GetComponent<BlankTile>();
                }
                bounce.spawnTile.InitSpawnTile(bounce.transform.position, bounce.parentTile.transform.position);
                int yPos = (int)(bounce.transform.position.z - prepends.y);
                int xPos = (int)(bounce.transform.position.x - prepends.x);
                for (int y = -1; y < 2; y += 2)
                {
                    bool corner = true;
                    if (yPos + y < blankTiles.Length && yPos + y >= 0 && xPos >= 0 && xPos < blankTiles[yPos + y].Length && blankTiles[yPos + y][xPos] != null)
                    {
                        blankTiles[yPos + y][xPos].BounceTile();
                    }
                    else { corner = false; }
                    for (int x = -1; x < 2; x += 2)
                    {
                        if (yPos >= 0 && yPos < blankTiles.Length && xPos + x < blankTiles[yPos].Length && xPos + x >= 0 && blankTiles[yPos][xPos + x] != null)
                        {
                            blankTiles[yPos][xPos + x].BounceTile();

                            if (yPos + y < blankTiles.Length && yPos + y >= 0 && corner && xPos >= 0 && xPos < blankTiles[yPos + y].Length && blankTiles[yPos + y][xPos + x] != null)
                            {
                                blankTiles[yPos + y][xPos + x].BounceTile();
                            }
                        }
                    }
                }
            }
        }
        UpdateCamLimits();
    }

    public GameObject GetCastleTile()
    {
        return gridTiles[0].gameObject;
    }
}
