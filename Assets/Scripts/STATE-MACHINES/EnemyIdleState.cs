using UnityEngine;

public class EnemyIdleState : StateMachineBehaviour {
    AttackController attackController;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        attackController = animator.GetComponent<AttackController>();
        attackController.SetIdleMaterial();

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        // Check for available target (enemies set their targets in SeekState)
       if (attackController.targetToAttack == null && attackController.gridTarget == Vector3.zero) {
            // Transition to follow state
            animator.SetBool("isSeeking", true);
       }
    }
}
