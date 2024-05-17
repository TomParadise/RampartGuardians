using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledParticleSystem : PooledObject
{
    private void OnParticleSystemStopped()
    {
        Release();
    }
}
