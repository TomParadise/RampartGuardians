using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlankTile : PooledObject
{
    [SerializeField] private Transform detailHolder;
    [SerializeField] private TowerPositioner utilityPositioner;
    public Vector2 coordinates;
    public bool bounced = false;

    public void Init(Vector2 coords)
    {
        coordinates = coords;
    }

    public override void ResetObject()
    {
        bounced = false;
        base.ResetObject();
    }

    public void InitSpawnTile(Vector3 pos, Vector3 parentPos)
    {
        gameObject.SetActive(true);
        if(parentPos.x != pos.x)
        {
            transform.rotation = Quaternion.Euler(0, parentPos.x < pos.x ? 270 : 90, 0);
        }
        else if(parentPos.z != pos.z)
        {
            transform.rotation = Quaternion.Euler(0, parentPos.z < pos.z ? 180 : 0, 0);
        }
        transform.position = pos;
        BounceTile();
    }

    public void CreateDetails(GameObject detail)
    {
        detail.transform.SetParent(detailHolder);
        detail.transform.position = detailHolder.position + new Vector3(Random.Range(-0.15f, 0.15f), 0, Random.Range(-0.15f, 0.15f));
        detail.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        detail.transform.localScale = Vector3.one;
    }

    public TowerPositioner EnableUtilityPositioner()
    {
        return utilityPositioner;
    }
    public void BounceTile()
    {
        if (bounced) { return; }
        bounced = true;
        StartCoroutine(Bounce());
    }

    private IEnumerator Bounce()
    {
        yield return null;
        float timer = 0;
        Vector3 goalScale = Vector3.one;
        while (timer < 0.5f)
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
