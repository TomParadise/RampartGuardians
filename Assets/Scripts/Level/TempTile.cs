using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempTile
{
    public bool isOccupied = false;
    public bool previouslyLockedIn = false;
    public TempTile parentTile;
    public int costFromStart;
    public int costToEnd;
    public int totalCost;
    public Vector2 coordinates;

    public void ResetCostToStart(Vector2 origin)
    {
        costFromStart = (int)Mathf.Abs(origin.x - coordinates.x) + (int)Mathf.Abs(origin.y - coordinates.y);
    }
}
