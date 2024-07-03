using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledAudio : PooledObject
{
    public void ReturnToPoolAfterTimer(float timer)
    {
        StartCoroutine(ReturnToPoolCo(timer));
    }

    private IEnumerator ReturnToPoolCo(float timer)
    {
        while (timer > 0)
        {
            //while (GameManager.instance.gameState == GameManager.GameState.Paused) { yield return null; }

            timer -= Time.deltaTime;
            yield return null;
        }
        Release();
    }
}
