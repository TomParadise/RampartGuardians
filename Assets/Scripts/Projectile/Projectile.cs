using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : PooledObject
{
    public float damage;
    public bool piercing;
    public float maxRange;
    public float speed;
    public Transform target;
    private bool alive = false;
    public float effectRange;

    public bool hasEffect = false;
    [SerializeField] private int effectIndex;
    [SerializeField] private Collider hitbox;
    private Tower parentTower;
    public bool rotateWhileFlying = false;
    public Vector3 rotateVector;

    [SerializeField] private Transform rotateObject;
    [SerializeField] private TrailRenderer trail;

    public void Init(float _damage, float _maxRange, float _speed, bool _piercing, Transform _target, float _effectRange, Tower _parentTower)
    {
        if (_target != null) { _target.GetComponent<Enemy>().incomingProjectiles.Add(this); }
        
        damage = _damage;
        maxRange = _maxRange;
        speed = _speed;
        piercing = _piercing;
        target = _target;
        effectRange = _effectRange;
        alive = true;
        parentTower = _parentTower;
        StartCoroutine(Shooting());
    }

    public IEnumerator Shooting()
    {
        float range = 0;
        Vector3 targetDir = (target == null ? transform.forward : ((target.position + Vector3.up * 0.25f) - transform.position).normalized);
        transform.rotation = Quaternion.LookRotation(targetDir);
        Vector3 prevPos = transform.position;
        while(range < maxRange && alive)
        {
            while(GameManager.instance.gameState != GameManager.GameState.Playing) { yield return null; }

            if(target != null)
            {
                targetDir = ((target.position + Vector3.up * 0.25f) - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(targetDir);
            }
            transform.position += targetDir * speed * Time.deltaTime;
            range += Vector3.Distance(transform.position, prevPos);
            prevPos = transform.position;

            if (rotateWhileFlying) { rotateObject.Rotate(rotateVector * Time.deltaTime); }
            yield return null;
        }
        if (!alive) { yield break; }
        KillProjectile();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!hasEffect)
        {
            GameObject effect = GameManager.instance.GetEffect(effectIndex);
            effect.SetActive(true);
            effect.transform.position = Physics.ClosestPoint(transform.position, other, other.transform.position, other.transform.rotation);
            other.GetComponent<Enemy>().TakeDamage(damage, parentTower);
        }
        if(!piercing)
        {
            KillProjectile();
        }
        piercing = false;
        if(target != null && other.transform == target) { target = null; }
    }

    public void KillProjectile()
    {
        if (!alive) { return; }
        alive = false;
        hitbox.enabled = false;
        if (hasEffect)
        {
            GameObject effect = GameManager.instance.GetEffect(effectIndex);
            effect.SetActive(true);
            effect.transform.position = transform.position;
            effect.GetComponent<Explosion>().Init(damage, effectRange, parentTower);
        }
        Release();
    }

    public override void ResetObject()
    {
        hitbox.enabled = true;
        if (trail != null) { trail.Clear(); }
        base.ResetObject();
    }
}
