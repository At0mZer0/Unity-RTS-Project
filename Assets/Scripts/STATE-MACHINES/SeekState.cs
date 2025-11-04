using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SeekState : StateMachineBehaviour {
    private Enemy enemy;
    private AttackController attackController;
    private Animator anim; // so i don't need to pass animator in to every new method that uses it
    private Transform transform;

    private GridZoneManager gridZoneManager;
    private Grid grid;
    private Vector3 gridTargetPos;

    private int searchRange;


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
       // Get components
        anim = animator;
        attackController = animator.GetComponent<AttackController>();
        enemy = animator.GetComponent<Enemy>();
        transform = animator.transform;

        searchRange = enemy.searchRange;

        gridZoneManager = GridZoneManager.Instance;
        grid = GridZoneManager.Instance.GetGrid();

        if (attackController.targetToAttack != null) {
            anim.SetBool("isFollowing", true);
            anim.SetBool("usingGridTarget", false);
            return; // enemy has a target and doesn't need to find player base grid Cell
        }
        
        if (InRangeOfBase()) {
            FindClosestTarget();

            // if we found a target already transition to follow state
            if(attackController.targetToAttack != null) {
                anim.SetBool("isFollowing", true);
                anim.SetBool("usingGridTarget", false);
                return;
            }
        }
        FindClosestTargetCell();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (attackController.targetToAttack != null || attackController.gridTarget != Vector3.zero) {
            animator.SetBool("isFollowing", true);
            return; // enemy has a target and doesn't need to find player base grid Cell
        }
    }




// ------------- Methods for state changes ---------------------------------------------

    private bool InRangeOfBase() {
        Vector3Int enemyCell = grid.WorldToCell(transform.position);
        int rangeOfBase = 10; //

        for (int x = -rangeOfBase; x <= rangeOfBase; x++) {
            for (int z = -rangeOfBase; z <= rangeOfBase; z++) {
                Vector3Int checkCell = enemyCell + new Vector3Int(x, 0, z);

                if (gridZoneManager.IsPositionBuildable(checkCell)) {
                    return true;
                }
            }
        }
        return false;
    }

    private void FindClosestTargetCell() {        
        Vector3Int enemyPos = grid.WorldToCell(transform.position);
        Vector3Int closestCell = Vector3Int.zero;
        float closestDist = float.MaxValue;
        bool foundAnyCell = false;
     // Debug   
        Debug.Log($"Searching for cells around {enemyPos} with range {searchRange}");
        int cellsChecked = 0;
        int buildableCellsFound = 0;

        // Loop through grid cells to find closest buildable cell (Player Base Zone, 
        for (int x = -searchRange; x <= searchRange; x++) {
            for (int z = -searchRange; z <= searchRange; z++) {
                Vector3Int cellToCheck = enemyPos + new Vector3Int(x, 0, z);
                cellsChecked++;
                
                // Skip the current cell the enemy is in!
                if (cellToCheck == enemyPos) continue;

                if (gridZoneManager.IsPositionBuildable(cellToCheck)) {
                    buildableCellsFound++;
                    Vector3 worldPos = grid.CellToWorld(cellToCheck);
                    float dist = Vector3.Distance(transform.position, worldPos);

                    if (dist < closestDist) {
                        closestDist = dist;
                        closestCell = cellToCheck;
                        foundAnyCell = true;
                    }
                }
            }
        }

        if (foundAnyCell) {
            gridTargetPos = grid.CellToWorld(closestCell) + new Vector3(0.5f, 0, 0.5f);
            anim.SetBool("usingGridTarget", true);
            attackController.gridTarget = gridTargetPos;

            // navAgent.SetDestination(gridTargetPos);
    
            Debug.Log($"Enemy {enemy.enemyType} found target at {gridTargetPos}, distance: {closestDist}");
        } else {
            Debug.LogWarning($"Enemy {enemy.enemyType} couldn't find any buildable cells!");
        }
    }

    private void FindClosestTarget() {
        Transform closestTarget = null;
        float closestDist = float.MaxValue;

        // Check Structures distance
        if (enemy.playerStructures != null) {
            foreach (GameObject structure in enemy.playerStructures) {
                if (structure == null) continue;

                float dist = Vector3.Distance(transform.position, structure.transform.position);
                if (dist < closestDist) {
                    closestDist = dist;
                    closestTarget = structure.transform;
                } 
            }
        }

        if (enemy.playerUnits != null) {
            foreach (GameObject unit in enemy.playerUnits) {
                if (unit == null) continue;

                float dist = Vector3.Distance(transform.position, unit.transform.position);
                if (dist < closestDist) {
                    closestDist = dist;
                    closestTarget = unit.transform;
                }
            }   
        }

        if (closestTarget != null) {
            attackController.targetToAttack = closestTarget;
            Debug.Log($"Enemy {enemy.enemyType} found target at {closestTarget.position}, distance: {closestDist}");
        } 
    }

}
