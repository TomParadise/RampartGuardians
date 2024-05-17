using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    [SerializeField] private MeleeTower meleeTower;
    private void OnTriggerEnter(Collider other)
    {
        meleeTower.OnEnemyHit(other, Physics.ClosestPoint(transform.position, other, other.transform.position, other.transform.rotation));
    }
}
