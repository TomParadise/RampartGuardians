using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableObjectOnExit : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<MeleeTower>().DisableHitboxes();
    }
}
