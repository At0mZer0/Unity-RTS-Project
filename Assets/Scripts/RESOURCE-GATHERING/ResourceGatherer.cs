using System.Collections.Generic;
using UnityEngine;

public enum ResourceState {
    Idle, // 0
    Gathering, // 1
    Depositing, // 2
    RuneGathering // 3
}

public class ResourceGatherer : MonoBehaviour {
    [Header("Gathering Properties")]
    public Transform targetResourceNode;
    public ResourceManager.ResourceType currentResourceType;
    public int currentLoad;
    public int maxLoad;
    public bool inGatheringRange = false;
    
    // for setting resource deposit / gather rate in Deposit and Gather States
    public float resourceRate = 1f;
    public int resourceQty = 5;

    [Header("Animation / State Change Properties")]
    public AttackController attackController;
    private Animator anim;
    public bool isUnloading;

    public Transform carryingRune;
    public bool hasRune = false;
    public bool runeCollision = false;
    public Transform runeTarget; // structure to deposit to
    public float runeDetectRadius = 5f;

    public float xpGained = 5f;
    public float spiritMod = -3f;

    // Helper for simplifying state machine logic
    public void SetResourceState(ResourceState state) {
        anim.SetInteger("ResourceState", (int)state);
    }

    private void Start() {
        attackController = GetComponent<AttackController>();
        anim = GetComponent<Animator>();
    }

    private void Update() {
        if (targetResourceNode == null) {
            inGatheringRange = false;
        }
        if (PlayerBaseCtrl.Instance.sacredTreeTransform != null) {
            runeTarget = PlayerBaseCtrl.Instance.sacredTreeTransform;
        } else {
            Debug.LogWarning("No Sacred Tree found for rune depositing.");
        }

        // Checks to see if a unit selected is this unit and that it's in idle state
        if (UnitSelectionManager.Instance.unitSelected.Contains(gameObject) && anim.GetInteger("ResourceState") == (int)ResourceState.Idle) {
            // look for nearby runes
            Collider[] colliders = Physics.OverlapSphere(transform.position, runeDetectRadius);
            foreach (var collider in colliders) {
                if (collider.CompareTag("Rune")) {
                    Rune rune = collider.GetComponent<Rune>();
                    if ( rune != null && rune.unitHoldingRune == null || rune.unitHoldingRune == gameObject) {
                        targetResourceNode = collider.transform;
                        SetResourceState(ResourceState.RuneGathering);

                        rune.unitHoldingRune = gameObject;
                        break;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (targetResourceNode != null && other.transform == targetResourceNode) {
            inGatheringRange = true;
        }

        if (attackController.isPlayer && other.CompareTag("Collection") && currentLoad > 0) {
            isUnloading = true;
        }

        Rune rune = other.GetComponent<Rune>();
        if (rune != null && !hasRune) {
            if (rune.unitHoldingRune == null || rune.unitHoldingRune == gameObject) {
                runeCollision = true;
                targetResourceNode = other.transform;

                rune.unitHoldingRune = gameObject;
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        if (attackController.isPlayer && other.CompareTag("Collection") && isUnloading) {

            if (currentLoad == 0) {
                isUnloading = false;
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.transform == targetResourceNode) {
            inGatheringRange = false;
        } 
    }

    public void PickupRune(Transform rune) {
        if (rune != null) {
            hasRune = true;
            carryingRune = rune;

            // Disable rube physics and parent in unit
            Rigidbody runeRb = rune.GetComponent<Rigidbody>();
            if (runeRb != null) {
                runeRb.isKinematic = true;
            }

            // Parent to unit and postion above unit
            rune.SetParent(transform);
            rune.localPosition = new Vector3(0, 1.5f, 0);

            SetResourceState(ResourceState.Depositing);
        }
    }

    public void DepositRune() {
        if (hasRune && carryingRune != null) {
            ResourceManager.Instance.IncreaseResource(ResourceManager.ResourceType.Runes, 1);
            hasRune = false;
            runeCollision = false;
            
            Rune rune = carryingRune.GetComponent<Rune>();
            if (rune != null) {
                rune.ShowRuneUI();
            }

            // Apply XP and Spirit Level mod
            PlayerBaseCtrl.Instance.GainXP(xpGained);
            PlayerBaseCtrl.Instance.AdjustSpiritLevel(spiritMod);

            Destroy(carryingRune.gameObject);
            carryingRune = null;
            SetResourceState(ResourceState.Idle);
        }
    }

}
