using Mono.Cecil;
using UnityEngine;
using UnityEngine.AI;

public class UnitDepositState : StateMachineBehaviour {
    NavMeshAgent agent;
    ResourceGatherer resourceGatherer;
    // Transform depositSite;

    private float depositTimer;
    public float depositRate = 1f; // 1 resource collect per seconf
    public int qtyDeposited = 5; // amount per deposit rate set
    private bool depositComplete = false;

    public Transform depositSite;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        agent = animator.GetComponent<NavMeshAgent>();
        resourceGatherer = animator.GetComponent<ResourceGatherer>();
        depositRate = resourceGatherer.resourceRate;
        qtyDeposited = resourceGatherer.resourceQty;

        Debug.Log($"Starting current Load: .................................{resourceGatherer.currentLoad}");

        depositTimer = depositRate; // Reset the deposit timer
        depositComplete = false;
        agent.isStopped = false; 

        if (resourceGatherer.hasRune) {
            depositSite = resourceGatherer.runeTarget; // Use the sacred tree as the deposit site
        } else {
            depositSite = FindClosestDepositSite(); // Find the closest deposit site
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        if (depositComplete) {
            return;
        }

        if (depositSite == null && !resourceGatherer.hasRune) {
            depositSite = FindClosestDepositSite(); // Find the closest deposit site
            return;
        }
        agent.isStopped = false;
        agent.SetDestination(depositSite.position);

        float distanceToDeposit = Vector3.Distance(resourceGatherer.transform.position, depositSite.position);
        bool isCloseToDeposit = distanceToDeposit < 3.0f; 

        if (resourceGatherer.hasRune && isCloseToDeposit) {
            resourceGatherer.DepositRune();
            depositComplete = true;
            return;
        }


        if (resourceGatherer.currentLoad > 0 && (isCloseToDeposit || resourceGatherer.isUnloading)) {
            if (depositTimer <= 0) {
                bool finishedUnload = false;
                if (resourceGatherer.currentLoad >= qtyDeposited) {
                    // Full deposit
                    ResourceManager.Instance.IncreaseResource(resourceGatherer.currentResourceType, qtyDeposited);
                    resourceGatherer.currentLoad -= qtyDeposited;

                    if (resourceGatherer.currentLoad == 0) {
                        finishedUnload = true;
                    }
                } else {
                    // Partial deposit (remaining resources)
                    int remainingResources = resourceGatherer.currentLoad;
                    ResourceManager.Instance.IncreaseResource(resourceGatherer.currentResourceType, remainingResources);
                    resourceGatherer.currentLoad = 0;
                    finishedUnload = true;

                }

                if (finishedUnload) {
                    depositComplete = true;
                     CheckStateAfterDeposit(animator);
                }

                depositTimer = depositRate;
            } else {
                depositTimer -= Time.deltaTime;
            }

        }
    }

    private void CheckStateAfterDeposit(Animator animator) {
        resourceGatherer.currentResourceType = ResourceManager.ResourceType.None; // Reset resource type
        resourceGatherer.isUnloading = false;

        bool hasValidNode = resourceGatherer.targetResourceNode != null;
        if (hasValidNode) {
            // Try to check if object is destroyed
            try {
                var test = resourceGatherer.targetResourceNode.gameObject;
            } catch {
                hasValidNode = false;
                Debug.LogWarning("Target resource node reference exists but is destroyed");
            }
        }
        
        if (hasValidNode) {
            Debug.Log("Deposit complete - returning to gather more");
            resourceGatherer.SetResourceState(ResourceState.Gathering);
        }
        else {
            Debug.Log("Deposit complete - no valid node, going idle");
            resourceGatherer.SetResourceState(ResourceState.Idle);
        }
    }

    public Transform FindClosestDepositSite() {
        GameObject[] depositSites = GameObject.FindGameObjectsWithTag("Collection");
        Debug.Log($"Found {depositSites.Length} deposit sites.");

        Transform nearestNode = null; // 
        float nearestDistance = Mathf.Infinity;
        Vector3 currentPosition = resourceGatherer.transform.position;

        foreach (GameObject node in depositSites) {
            
            float distance = Vector3.Distance(currentPosition, node.transform.position);
            if (distance < nearestDistance) {
                nearestNode = node.transform;
                nearestDistance = distance;
            }
            
        }

        return nearestNode;

    }


}