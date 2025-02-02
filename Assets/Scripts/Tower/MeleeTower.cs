using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeTower : AttackingTower
{
    [SerializeField] private bool spin = false;
    [SerializeField] private GameObject[] meleeHitboxes;
    [SerializeField] private TrailRenderer meleeTrail;
    private Enemy targetEnemy = null;
    public int doubleSpin = -1;

    public override void UpgradeTower()
    {
        if (level - 1 == upgrades.Length) { return; }
        if (upgrades[level - 1].uniqueUpgrade != 0)
        {
            doubleSpin = 0;
        }

        base.UpgradeTower();
    }

    public override void OnRoundEnd()
    {
        base.OnRoundEnd();
        if(doubleSpin > 0) { doubleSpin = 0; }
    }

    public override void ResetObject()
    {
        doubleSpin = -1;
        meleeTrail.Clear();
        meleeTrail.gameObject.SetActive(false);

        base.ResetObject();
    }

    public void DisableHitboxes()
    {
        for (int i = 0; i < meleeHitboxes.Length; i++)
        {
            meleeHitboxes[i].SetActive(false);
        }
        meleeTrail.gameObject.SetActive(false);
    }

    public override void Shoot(Collider[] cols)
    {
        meleeTrail.Clear();
        meleeTrail.gameObject.SetActive(true);
        if (spin) { StartCoroutine(SpinAttack()); }
        else
        {
            animator.Play("Shoot");
            targetEnemy = GetTargetEnemy(cols);
            if (rotateWithAttack) { transform.rotation = Quaternion.LookRotation((targetEnemy.transform.position - transform.position).normalized); }
            for (int i = 0; i < meleeHitboxes.Length; i++)
            {
                meleeHitboxes[i].SetActive(true);
            }
        }

        AudioManager.instance.PlaySFX(attackSFX);
    }

    private IEnumerator SpinAttack()
    {
        List<Collider> hitCols = new List<Collider>();
        Vector3[] attackPoints = new Vector3[8] {transform.forward * 0.5f, transform.forward * 0.5f + transform.right * 0.5f, transform.right * 0.5f,
                                                 transform.right * 0.5f - transform.forward * 0.5f , -transform.forward * 0.5f , -transform.forward * 0.5f - transform.right * 0.5f,
                                                 -transform.right * 0.5f, transform.forward * 0.5f - transform.right * 0.5f};
        if(doubleSpin > -1) { doubleSpin++; }
        //doubleSpin = 3;
        int spinCount = 1;
        if (doubleSpin >= 3)
        {
            doubleSpin = 0;
            animator.Play("DoubleSpin");
            spinCount = 2;
            GetComponent<Animator>().SetBool("Spin Complete", false);
        }
        else { animator.Play("Shoot"); }

        for(int j = 0; j < spinCount; j++)
        {
            for (int i = 0; i < attackPoints.Length; i++)
            {
                float timer = spinCount == 2 ? 0.0425f : 0.08f;
                while(timer > 0)
                {
                    while(GameManager.instance.gameState == GameManager.GameState.Paused) { yield return null; }

                    timer -= Time.deltaTime;
                    yield return null;
                }
                Collider[] cols = Physics.OverlapSphere(transform.position + attackPoints[i], 0.3f, 1 << 7);
                foreach(Collider col in cols)
                {
                    if(!hitCols.Contains(col))
                    {
                        hitCols.Add(col);
                        col.GetComponent<Enemy>().TakeDamage(damage, this);
                        break;
                    }
                }
                GameObject impact = GameManager.instance.GetEffect(2);
                impact.transform.position = transform.position + attackPoints[i] + Vector3.up * 0.25f;
                impact.SetActive(true);
            }
            hitCols.Clear();
        }
        if(spinCount == 2) { GetComponent<Animator>().SetBool("Spin Complete", true); }
        meleeTrail.gameObject.SetActive(false);
    }


    public void OnEnemyHit(Collider other, Vector3 hitPos)
    {
        if(!piercingProjectiles && other.gameObject != targetEnemy.gameObject) { return; }
        other.GetComponent<Enemy>().TakeDamage(damage, this);

        GameObject impact = GameManager.instance.GetEffect(2);
        impact.transform.position = hitPos;
        impact.SetActive(true);
    }
}
