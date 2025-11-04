using UnityEngine;
using UnityEngine.AI;

public class EnemyFollowState : StateMachineBehaviour {
    AttackController attackController;
    NavMeshAgent agent;
    Animator anim;
    
    public float attackingDistance = 10f;
    private Vector3 targetPosition;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        anim = animator;
        attackController = animator.GetComponent<AttackController>();
        agent = animator.GetComponent<NavMeshAgent>();
        attackController.SetFollowMaterial();

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (ShouldTransitionToSeek()) {
            return;
        }
        DetermineTargetPosition();
        MoveTowardsTarget();
        CheckForAttackTranstition();
    }


    /// should I transition to previous state?
    private bool ShouldTransitionToSeek() {
        if (attackController.targetToAttack == null && attackController.gridTarget == Vector3.zero) {
            anim.SetBool("isFollowing", false);
            return true;
        }
        return false;
    }

    private void DetermineTargetPosition() {
        if (attackController.targetToAttack != null) {
            targetPosition = attackController.targetToAttack.position;

            if (anim.GetBool("usingGridTarget")) {
                anim.SetBool("usingGridTarget", false);
            }
        }
        else if (anim.GetBool("usingGridTarget")) {
            targetPosition = attackController.gridTarget;
        } else {
            anim.SetBool("isFollowing", false);
        }
    }

    private void MoveTowardsTarget() {
        if (agent.destination != targetPosition) {
            agent.SetDestination(targetPosition);
            anim.transform.LookAt(targetPosition);
        }
        if (anim.GetBool("usingGridTarget")) {
            CheckGridTargetReached();
        }
    }

    private void CheckGridTargetReached(){
        if (anim.GetBool("usingGridTarget")) {
            float distToTarget = Vector3.Distance(anim.transform.position, targetPosition);
            if (distToTarget <= agent.stoppingDistance + 0.5f) {
                anim.SetBool("usingGridTarget", false);
                anim.SetBool("isFollowing", false);
                attackController.gridTarget = Vector3.zero;
            }
        }
    }

    private void CheckForAttackTranstition() {
        if (!anim.GetBool("usingGridTarget") && attackController.targetToAttack != null) {
            float distFromTarget = Vector3.Distance(attackController.targetToAttack.position, anim.transform.position);
    
            if (distFromTarget < attackingDistance) {
                agent.SetDestination(anim.transform.position);
                anim.SetBool("isAttacking", true);
                
            }
        }
    }
}
