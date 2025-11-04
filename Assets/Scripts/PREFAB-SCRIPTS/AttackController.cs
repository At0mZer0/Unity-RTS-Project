using Unity.VisualScripting;
using UnityEngine;

public class AttackController : MonoBehaviour {

    public Transform targetToAttack;
    public Vector3 gridTarget;

    public Material idleStateMaterial;
    public Material followStateMaterial;
    public Material attackStateMaterial;

    public bool isPlayer;
    public int unitDamage; // melee damage
    public float projectileDmg; // ranged weapon damage
    
    public float attackMod = 1.0f;
    public float attackRate = 2f;


    private void OnTriggerEnter(Collider other) {
        if (isPlayer && (other.CompareTag("Enemy") || other.CompareTag("Roman") || other.CompareTag("Fae")) && targetToAttack == null) {
            targetToAttack = other.transform;
        } else if (!isPlayer && (other.CompareTag("Unit") || other.CompareTag("PlayerBase")) && targetToAttack == null){
            targetToAttack = other.transform;

            // Force state change to follow this target
            Animator anim = GetComponent<Animator>();
            if (anim != null) {
                anim.SetBool("isFollowing", true);
                anim.SetBool("usingGridTarget", false);

                gridTarget = Vector3.zero;
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        if (isPlayer && (other.CompareTag("Enemy") || other.CompareTag("Roman") || other.CompareTag("Fae")) && targetToAttack == null) {
            targetToAttack = other.transform;
        } else if (!isPlayer && (other.CompareTag("Unit") || other.CompareTag("PlayerBase")) && targetToAttack == null){
            targetToAttack = other.transform;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (isPlayer && (other.CompareTag("Enemy") || other.CompareTag("Roman") || other.CompareTag("Fae")) && targetToAttack != null) {
            targetToAttack = null;
        } else if (!isPlayer && (other.CompareTag("Unit") || other.CompareTag("PlayerBase")) && targetToAttack == null){
            targetToAttack = null;
        }
    }

    public void SetIdleMaterial() {
        GetComponent<Renderer>().material = idleStateMaterial;
    }

    public void SetFollowMaterial() {
        GetComponent<Renderer>().material = followStateMaterial;

    }

    public void SetAttackMaterial() {
         GetComponent<Renderer>().material = attackStateMaterial;

    }

    private void OnDrawGizmos() {
        // follow distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 10f*0.2f);

        // attack distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // stop attack distance / area
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1.2f);
    }

}

