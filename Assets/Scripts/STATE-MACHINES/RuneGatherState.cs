using UnityEngine;
using UnityEngine.AI;

public class RuneGatherState : StateMachineBehaviour {

    NavMeshAgent agent;
    ResourceGatherer resourceGatherer;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        agent = animator.GetComponent<NavMeshAgent>();
        resourceGatherer = animator.GetComponent<ResourceGatherer>();

        agent.isStopped = false; // Ensure the agent is not stopped when entering this state
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if(resourceGatherer.targetResourceNode == null) {
            resourceGatherer.SetResourceState(ResourceState.Idle);
            return;
        }

        // Move to rune
        agent.SetDestination(resourceGatherer.targetResourceNode.position);

        // check distance or trigger enter
        if (resourceGatherer.runeCollision || Vector3.Distance(animator.transform.position, resourceGatherer.targetResourceNode.position) < 1f) {
            //pickup rune
            resourceGatherer.PickupRune(resourceGatherer.targetResourceNode);

            // switch to depositing state
            resourceGatherer.SetResourceState(ResourceState.Depositing);
        }
    }

}
