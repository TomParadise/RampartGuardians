using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeTower : AttackingTower
{
    [SerializeField] private bool spin = false;
    [SerializeField] private GameObject[] meleeHitboxes;
    [SerializeField] private TrailRenderer meleeTrail;
    private Enemy targetEnemy = null;

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
        animator.Play("Shoot");
        meleeTrail.Clear();
        meleeTrail.gameObject.SetActive(true);
        if (spin) { StartCoroutine(SpinAttack()); }
        else
        {
            targetEnemy = GetTargetEnemy(cols);
            if (rotateWithAttack) { transform.rotation = Quaternion.LookRotation((targetEnemy.transform.position - transform.position).normalized); }
            for (int i = 0; i < meleeHitboxes.Length; i++)
            {
                meleeHitboxes[i].SetActive(true);
            }
        }
    }

    private IEnumerator SpinAttack()
    {
        List<Collider> hitCols = new List<Collider>();
        Vector3[] attackPoints = new Vector3[8] {new Vector3(-0.5f, 0, 0.5f), new Vector3(0, 0, 0.5f), new Vector3(0.5f, 0, 0.5f),
                                                 new Vector3(0.5f, 0, 0), new Vector3(0.5f, 0, -0.5f), new Vector3(0, 0, -0.5f), 
                                                 new Vector3(-0.5f, 0, -0.5f), new Vector3(-0.5f, 0, 0)};
        for(int i = 0; i < attackPoints.Length; i++)
        {
            float timer = 0.08f;
            while(timer > 0)
            {
                while(GameManager.instance.gameState != GameManager.GameState.Playing) { yield return null; }

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
            impact.transform.position = transform.position + attackPoints[i];
            impact.SetActive(true);
        }
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
