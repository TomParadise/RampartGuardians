using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageNumbers : PooledObject
{
    [SerializeField] private CartoonFX.CFXR_ParticleText particleText;

    public void Init(int damage)
    {
        transform.position += Vector3.up * 0.2f;
        particleText.UpdateText(damage.ToString() + (damage > 20 ? "!" : ""));
        float scale = damage == 5 ? 0.09f : (damage == 10 ? 0.1f : (damage == 15 ? 0.115f : (damage == 20 ? 0.125f : 0.15f)));
        transform.localScale = new Vector3(scale, scale, scale);
    }
    private void OnParticleSystemStopped()
    {
        Release();
    }
}
