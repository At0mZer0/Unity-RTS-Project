using System;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.AI;

public class UnitGatherState : StateMachineBehaviour {
    NavMeshAgent agent;
    ResourceGatherer resourceGatherer;

    public float gatherRate; // 1 resource collect per seconf
    public int qtyGathered; // amount per gather rate set
    private float gatherTimer;
    public float gatherDist = 2f;
  


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        agent = animator.GetComponent<NavMeshAgent>();
        resourceGatherer = animator.GetComponent<ResourceGatherer>();
        gatherRate = resourceGatherer.resourceRate;
        qtyGathered = resourceGatherer.resourceQty;
        
        gatherTimer = gatherRate;

        Debug.Log($"Entered Gather State. Target node: {(resourceGatherer.targetResourceNode != null ? resourceGatherer.targetResourceNode.name : "null")}");
    }


    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {   
        if (resourceGatherer.targetResourceNode == null) {
            Debug.Log("Resource node is null during gathering");

            if (resourceGatherer.currentLoad > 0) {
                // if we have a load go deposit it
                resourceGatherer.SetResourceState(ResourceState.Depositing);
            }
            else {
                resourceGatherer.SetResourceState(ResourceState.Idle);
            }
            return;
        }

        if (!animator.transform.GetComponent<UnitMovement>().isCommandedToMove) {
            if (resourceGatherer.inGatheringRange) {
                agent.isStopped = true;

                if (gatherTimer <= 0) {
                    GatherResource();
                    gatherTimer = gatherRate; // Reset the timer
                } else {
                    gatherTimer -= Time.deltaTime; // Decrease the timer
                }
            }
            else {
                agent.isStopped = false;
                agent.SetDestination(resourceGatherer.targetResourceNode.position);
            }
            
            if (resourceGatherer.currentLoad >= resourceGatherer.maxLoad) {
                resourceGatherer.currentLoad = resourceGatherer.maxLoad;

                resourceGatherer.SetResourceState(ResourceState.Depositing);
            }
        }
       
    }

    public void GatherResource() {
        var resourceType = resourceGatherer.targetResourceNode.GetComponent<ResourceNode>().resourceType;
        var resourceNode = resourceGatherer.targetResourceNode.GetComponent<ResourceNode>(); 
  
        if (resourceGatherer.currentResourceType == ResourceManager.ResourceType.None) {
            resourceGatherer.currentResourceType = resourceType;
        }
        if (resourceNode.resourceQty >= qtyGathered) {
            resourceGatherer.currentLoad += qtyGathered;
            resourceNode.resourceQty -= qtyGathered;
        } else {
            int remainingQty = resourceNode.resourceQty;
            resourceGatherer.currentLoad += remainingQty;
            resourceNode.resourceQty -= remainingQty;
        }
    }

}
