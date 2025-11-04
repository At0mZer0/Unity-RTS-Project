using UnityEngine;

public class TowerAttackState : StateMachineBehaviour {
    TowerDefense towerDefense;
    Constructable constructable;
    Transform target;
    GameObject projectile;
    private float lastFireTime = 0f;
    private float fireRate; // shots per second
    Vector3 launchPos;



    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
       towerDefense = animator.GetComponent<TowerDefense>();
       constructable = animator.GetComponent<Constructable>();
       target = towerDefense.targetToAttack;
       projectile = towerDefense.projectile;
       launchPos = towerDefense.transform.position + new Vector3(5, 16, 3);
       fireRate = constructable.attackRate;
    }

    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (towerDefense.enemyUnitsInRange.Count == 0) {
            animator.SetBool("enemyInRange", false);
        }

        target = towerDefense.targetToAttack;

        if (target == null) {
            towerDefense.FindClosestEnemy();
            target = towerDefense.targetToAttack;

            if (target == null) {
                animator.SetBool("enemyInRange", false);
                return;
            }
        }
        
        if(Time.time > lastFireTime + fireRate) {
            FireProjectile(target, launchPos);
            lastFireTime = Time.time;
        }
    }

    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
       
    }

    private void FireProjectile(Transform target, Vector3 pos) {
        Vector3 targetAim = target.position + new Vector3(0, 2, 0);
        Vector3 direction = targetAim - pos;
        Quaternion projRot = Quaternion.LookRotation(direction);
        Vector3 launchPos = towerDefense.transform.position + new Vector3(10, 5, 10);
        GameObject newProjectile = Instantiate(projectile, pos, projRot);

        // Set damage based on constructable attack mod
        DamageDealer damageDealer = newProjectile.GetComponent<DamageDealer>();
        damageDealer.damageDelt = towerDefense.finalDmg;

        Debug.DrawRay(launchPos, direction * 1f, Color.red, 2f);

        Rigidbody rb = newProjectile.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = direction * 5f;
        }
    }
}
