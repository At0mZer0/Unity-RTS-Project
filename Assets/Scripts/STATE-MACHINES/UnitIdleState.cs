using UnityEngine;

public class UnitIdleState : StateMachineBehaviour {
    
    AttackController attackController;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        attackController = animator.GetComponent<AttackController>();
        attackController.SetIdleMaterial();

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        // Check for available target (enemies set their targets in SeekState)
       if(attackController.targetToAttack != null) {
            // Transition to follow state
            animator.SetBool("isFollowing", true);
       }
    }



}
