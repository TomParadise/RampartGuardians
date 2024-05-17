using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackOnExit : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Enemy>().DealDamage();
    }
}
