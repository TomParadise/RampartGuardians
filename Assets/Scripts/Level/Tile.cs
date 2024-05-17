using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Tile parentTile;
    public List<Tile> childTiles = new List<Tile>();
    public int costFromStart;
    public int costToEnd;
    //coordinates that correlate to world position
    public Vector2 coordinates;
    //the original coordinates when the tile was placed, only relevant for multiple paths on the same wall situations
    public Vector2 originalCoordinates;
    //the grid width and height from when this tile was placed, only relevant for multiple paths on the same wall situations
    public Vector2 originalGridSize;

    public Vector2 prepends;
    public Vector2 appends;

    public List<Vector2> ignoreTiles = new List<Vector2>();

    public int forceBlockXPrepend = 0;
    public int forceBlockYPrepend = 0;

    public BlankTile spawnTile = null;

    public void Init(Vector2 _coordinates, Vector2 _prepends, Vector2 _appends, Tile _parentTile = null)
    {
        if(_parentTile != null)
        {
            parentTile = _parentTile;
        }
        originalCoordinates = coordinates;
        prepends = _prepends;
        appends = _appends;
        coordinates = _coordinates + prepends;
        transform.position = new Vector3(coordinates.x, 0, coordinates.y);
    }
    public void ReInitPrepends(Vector2 _prepends)
    {
        if (prepends.x != _prepends.x)
        {
            coordinates.x = originalCoordinates.x - prepends.x;
        }
        if (prepends.y != _prepends.y)
        {
            coordinates.y = originalCoordinates.y - prepends.y;
        }
    }

    public void InitIgnorePrependCoords(Vector2 _coordinates, Vector2 _prepends, Vector2 _appends, Tile _parentTile = null)
    {
        if(_parentTile != null)
        {
            parentTile = _parentTile;
        }
        coordinates = _coordinates;
        prepends = _prepends;
        appends = _appends;
        transform.position = new Vector3(coordinates.x + prepends.x, 0, coordinates.y + prepends.y);
    }

    public void InstantBounce()
    {
        transform.localScale = Vector3.one;
    }

    public void BounceTile()
    {
        StartCoroutine(Bounce());
    }

    private IEnumerator Bounce()
    {
        float timer = 0;
        Vector3 goalScale = Vector3.one;
        while(timer < 0.5f)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, goalScale, timer / 0.5f);

            yield return null;
        }
        timer = 0f;
        while (timer < 0.2f)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(goalScale, Vector3.one, timer / 0.2f);

            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}
