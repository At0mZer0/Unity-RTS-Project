using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour {
    // target lists for enemies
    public List<GameObject> playerStructures;
    public List<GameObject> playerUnits;

    private Transform directTarget; // the target to attack (for direct targeting))
    private AttackController attackController;
    private Vector3 targetPosition;

    private NavMeshAgent navAgent;
    private Animator animator;
    private Unit unit;
    public string enemyType; // Roman or Fae

    public int attackRange = 5; // distance to attack the base
    public int searchRange = 200;

    public float xpGained = 15f;
    public float intensityMod = -10f;



    private void Start() {
        // references for targeting player structures and units
        playerStructures = ObjectPlacer.Instance.placedGameObjects;
        playerUnits = UnitSelectionManager.Instance.allUnitsList;

        // remove enemy from unit list and mark as no player
        attackController = GetComponent<AttackController>();
        attackController.isPlayer = false;
        animator = GetComponent<Animator>();
        
        PlayerBaseCtrl.Instance.ModifyIntensity(intensityMod);

    }

    private void Update() {

    }

    private void OnDestroy() {
        PlayerBaseCtrl.Instance.GainXP(xpGained);
        PlayerBaseCtrl.Instance.ModifyIntensity(intensityMod);
        if (enemyType == "Roman")
        {
            PlayerBaseCtrl.Instance.romansKilled++;
            SpawnController.Instance.spawnedRomans.Remove(gameObject);
        }
        else if (enemyType == "Fae")
        {
            PlayerBaseCtrl.Instance.faeKilled++;
            SpawnController.Instance.spawnedFae.Remove(gameObject);
        }
        SpawnController.Instance.UpdateSpawnedEnemyList();
    }
}
