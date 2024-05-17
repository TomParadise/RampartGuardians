using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffingEnemy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<Enemy>().BuffSpeed();
    }

    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<Enemy>().CheckForBuffingAoEs();
    }
}
