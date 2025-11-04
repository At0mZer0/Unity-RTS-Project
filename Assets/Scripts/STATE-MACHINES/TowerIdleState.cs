using UnityEngine;

public class TowerIdleState : StateMachineBehaviour {
    
    TowerDefense towerDefense;


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
       towerDefense = animator.GetComponent<TowerDefense>();
    }

    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (towerDefense.enemyUnitsInRange.Count > 0) {
            animator.SetBool("enemyInRange", true);
        } 
    }

    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
       
    }

}
