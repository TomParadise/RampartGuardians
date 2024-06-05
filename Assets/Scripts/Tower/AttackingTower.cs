using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingTower : Tower
{
    public Transform projectileSpawnPoint;
    [SerializeField] public int projectileIndex;
    [SerializeField] public float effectRange = 0;
    public bool rotateWithAttack = true;

    public float shotTimer = 0;
    public float maxFireRateTimer = 1;

    [SerializeField] public AudioClip attackSFX;
    public void SetTargeting(TargetType _targetType)
    {
        targetType = _targetType;
    }

    public override void OnRoundEnd()
    {
        base.OnRoundEnd();
        shotTimer = 0;
    }

    public override void ResetObject()
    {
        base.ResetObject();
        maxFireRateTimer = 1 / fireRate;
        shotTimer = 0;
        if (levelUpPanel != null) { levelUpPanel.UpdateCharge(1); }
    }

    public override void UpgradeTower()
    {
        base.UpgradeTower();
        maxFireRateTimer = 1 / fireRate;
    }

    private void OnEnable()
    {
        animator.Play("Float");
    }
    private void Update()
    {
        if (GameManager.instance.gameState == GameManager.GameState.Playing && placed)
        {
            if (shotTimer <= 0)
            {
                if (StartShoot())
                {
                    SetShotTimer();
                }
            }
            else
            {
                shotTimer -= Time.deltaTime;
                if(levelUpPanel != null)
                {
                    levelUpPanel.UpdateCharge(1 - (shotTimer / (maxFireRateTimer * (1 - buffAmount))));
                }
            }
        }
    }

    public virtual bool StartShoot()
    {
        return ShootCheck();
    }

    public virtual void SetShotTimer()
    {
        shotTimer = maxFireRateTimer;
        shotTimer *= (1 - buffAmount);
    }

    public virtual bool ShootCheck()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, range - 0.05f, 1 << 7);
        if (cols.Length == 0) { return false; }
        else { Shoot(cols); }
        return true;
    }

    public Enemy GetTargetEnemy(Collider[] cols)
    {
        Enemy targetEnemy = cols[0].GetComponent<Enemy>();
        float targetVal = (targetType == TargetType.First || targetType == TargetType.Last) ? targetEnemy.GetDistanceToGoal() : (targetEnemy.currentHP);
        foreach (Collider col in cols)
        {
            Enemy colEnemy = col.GetComponent<Enemy>();
            if (targetType == TargetType.First)
            {
                float dist = colEnemy.GetDistanceToGoal();
                if (dist < targetVal)
                {
                    targetVal = dist;
                    targetEnemy = colEnemy;
                }
            }
            else if (targetType == TargetType.Last)
            {
                float dist = colEnemy.GetDistanceToGoal();
                if (dist > targetVal)
                {
                    targetVal = dist;
                    targetEnemy = colEnemy;
                }
            }
            else if (targetType == TargetType.HighestHP)
            {
                float HP = colEnemy.currentHP;
                if (HP > targetVal 
                    || (HP == targetVal && colEnemy.GetDistanceToGoal() < targetEnemy.GetDistanceToGoal()))
                {
                    targetVal = HP;
                    targetEnemy = colEnemy;
                }
            }
            else if (targetType == TargetType.LowestHP)
            {
                float HP = colEnemy.currentHP;
                if (HP < targetVal
                    || (HP == targetVal && colEnemy.GetDistanceToGoal() < targetEnemy.GetDistanceToGoal()))
                {
                    targetVal = HP;
                    targetEnemy = colEnemy;
                }
            }
        }
        return targetEnemy;
    }

    //default projectile shot check
    public virtual void Shoot(Collider[] cols)
    {
        Enemy targetEnemy = GetTargetEnemy(cols);
        transform.rotation = Quaternion.LookRotation((targetEnemy.transform.position - transform.position).normalized);
        Projectile projectile = GameManager.instance.GetProjectile(projectileIndex).GetComponent<Projectile>();
        projectile.transform.position = projectileSpawnPoint.position;
        projectile.gameObject.SetActive(true);
        projectile.Init(damage, range, shotSpeed, piercingProjectiles, targetEnemy.transform, effectRange, this);
        animator.Play("Shoot");

        AudioManager.instance.PlaySFX(attackSFX);
    }
}
