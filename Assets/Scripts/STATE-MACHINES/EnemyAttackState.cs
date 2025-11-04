using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAttackState : StateMachineBehaviour {
    NavMeshAgent agent;
    AttackController attackController;

    public float stopAttackingDistance = 12f;

    public float attackRate = 2f;
    private float attackTimer;


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        agent = animator.GetComponent<NavMeshAgent>();
        attackController = animator.GetComponent<AttackController>();
        attackController.SetAttackMaterial();
    }



    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
       if (attackController.targetToAttack != null) {
            LookAtTarget();

            agent.SetDestination(attackController.targetToAttack.position);

            if (attackTimer <= 0) {
                Attack();
                attackTimer = 1f / attackRate;
            } else {
                attackTimer -= Time.deltaTime; // uses deltaTime to get 
            }

            //  Should unit still attack?
            float distanceFromTarget = Vector3.Distance(attackController.targetToAttack.position, animator.transform.position);
            
            if (distanceFromTarget > stopAttackingDistance || attackController.targetToAttack == null) {
                animator.SetBool("isAttacking", false);
            }  
       } else {
        animator.SetBool("isAttacking", false);
        // Debug.Log("Enemy defeated target, returning to seeking state");
        attackController.gridTarget = Vector3.zero; // Reset grid target to search again if not within range of base already
       }
    }

    /// ------------------------------ ///

    private void Attack() {
            var damageToInflict = attackController.unitDamage;

            SoundManager.Instance.PlayInfantryAttackSound();

            // Actually attack the unit
            // attackController.targetToAttack.GetComponent<Unit>().TakeDamage(damageToInflict);

            var damageable = attackController.targetToAttack.GetComponent<IDamageable>();
            if (damageable != null) { // checks the targetToAttacks Components for IDamageable
                damageable.TakeDamage(damageToInflict);
            }
    }

    private void LookAtTarget() {
        Vector3 direction = attackController.targetToAttack.position - agent.transform.position;
        agent.transform.rotation = Quaternion.LookRotation(direction);

        var yRotation = agent.transform.eulerAngles.y;
        agent.transform.rotation = Quaternion.Euler(0, yRotation, 0);

    }
}
