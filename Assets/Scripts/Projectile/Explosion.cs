using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : PooledObject
{
    [SerializeField] private ParticleSystem[] systemsToScale;
    public void Init(float damage, float range, Tower attackingTower)
    {
        transform.localScale = Vector3.one * range * 0.3f;
        for (int i = 0; i < systemsToScale.Length; i++)
        {
            var main = systemsToScale[i].main;
            main.startSize = range * 2f;
        }
        Collider[] cols = Physics.OverlapSphere(transform.position, range, 1 << 7);

        for(int i = 0; i < cols.Length; i++)
        {
            Enemy enemy = cols[i].GetComponent<Enemy>();
            if (enemy.armoured) { enemy.TakeDamage(damage / 2, attackingTower); }
            else { enemy.TakeDamage(damage, attackingTower); }
        }
    }

    private void OnParticleSystemStopped()
    {
        Release();
    }
}
