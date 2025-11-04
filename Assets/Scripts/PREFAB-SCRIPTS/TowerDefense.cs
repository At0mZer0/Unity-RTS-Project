using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerDefense : MonoBehaviour {

    [Serializable]
    public struct EnemyData {
        public GameObject enemyObj;
        public Transform enemyTransform;
        public Unit enemyUnit;
        public float enemyHealth;

        public EnemyData(GameObject obj) {
            enemyObj = obj;
            enemyTransform = obj.transform;
            enemyUnit = obj.GetComponent<Unit>();
            enemyHealth = enemyUnit.unitHealth;
        }
    }
    
    public List<EnemyData> enemyUnitsInRange = new List<EnemyData>();
    public Transform targetToAttack;
    public GameObject projectile; // set this to the projectile prefab in the inspector
    Constructable constructable;
    float atkMod;
    public float projectileDmg;
    public float finalDmg;
    
    
    void Start() {
        constructable = GetComponent<Constructable>();
        atkMod = constructable.attackMod;
        finalDmg = atkMod * projectileDmg;
        Debug.Log("Final Damage: " + finalDmg);
        FindClosestEnemy();
    }

    void Update() {
        if(enemyUnitsInRange.Count > 0) {
            ConfirmTargetKill();
            FindClosestEnemy();
        }
    }

    private void OnTriggerEnter(Collider other) { 
        if (other.CompareTag("Enemy") || other.CompareTag("Roman") || other.CompareTag("Fae")) {
            foreach (var enemy in enemyUnitsInRange) {
                if (enemy.enemyObj == other.gameObject) {
                    return;
                }
            }
            EnemyData newEnemy = new EnemyData(other.gameObject);
            enemyUnitsInRange.Add(newEnemy);
            FindClosestEnemy();
        }
    }


    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Enemy") || other.CompareTag("Roman") || other.CompareTag("Fae")) {
           foreach (var enemy in enemyUnitsInRange) {
                if (enemy.enemyObj == other.gameObject) {
                    enemyUnitsInRange.Remove(enemy);
                    break;
                }
           }
        }
    }

    private void ConfirmTargetKill() {
        if (enemyUnitsInRange.Count == 0) {
            targetToAttack = null;
            return;
        }
        foreach (var enemy in enemyUnitsInRange) {
           if (enemy.enemyHealth <= 0 || enemy.enemyTransform == null) {
                enemyUnitsInRange.Remove(enemy);
                break;
            }
        }
    }

    public void FindClosestEnemy() {
        float closestDist = Mathf.Infinity;
        float enemyDist;
        if (enemyUnitsInRange.Count == 0) {
            targetToAttack = null;
            return;
        } else {
            foreach (var enemy in enemyUnitsInRange) {
                enemyDist = Vector3.Distance(enemy.enemyTransform.position, transform.position);
                if (enemyDist < closestDist) {
                    closestDist = enemyDist;
                    targetToAttack = enemy.enemyTransform;
                }
            }
        }
    }

}
