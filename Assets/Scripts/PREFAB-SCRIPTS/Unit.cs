using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour, IDamageable {
    
    public float unitHealth;
    public float unitMaxHealth;
    public bool inPreviewMode = true; // for the object placer to know if the unit is in preview mode or not

    Animator animator;
    NavMeshAgent navAgent;
    public HealthTracker healthTracker;
    private AttackController atkCtrl;
    public bool isPlayer;
    public float defenseMod;  // default is 1, will multiply units damage taken by units defense mod
    public int databaseID;

    public float intensityMod = 5f;
    public float dmgIntensityMod = 1.2f; // player adds enemy subtracts
    public float xpGained = 15f;

    void Start() {
        if (isPlayer) {
            UnitSelectionManager.Instance.allUnitsList.Add(gameObject); //adds the unit instance to the list on creation
        } 
        
        unitHealth = unitMaxHealth;
        UpdateHealthUI();

        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        atkCtrl = GetComponent<AttackController>();
        isPlayer = atkCtrl.isPlayer;
        if (isPlayer) {
            PlayerBaseCtrl.Instance.UpdateUnitQty(1); // increase the unit quantity in the player base controller
        }
    }

    void Update() {
        // Agent has reached destination
        if (navAgent.remainingDistance > navAgent.stoppingDistance) {
            animator.SetBool("isMoving", true); // for actual walking animation

        } else {
            animator.SetBool("isMoving", false);

        }

    }

    private void OnDestroy() {
        UnitSelectionManager.Instance.allUnitsList.Remove(gameObject); //removes the unit instance from the list on destruction
        
        if (!isPlayer) {
            ItemSpawner.Instance.SpawnRune(transform.position); // spawn a rune at the unit's position
            PlayerBaseCtrl.Instance.UpdateEnemyKills(1);
        } else {
            UnitSelectionManager.Instance.unitSelected.Remove(gameObject);
            if (!inPreviewMode) {
                PlayerBaseCtrl.Instance.UpdateUnitQty(-1);
                PlayerBaseCtrl.Instance.fallenUnits.Add(databaseID);
            }

        }       
    }

    private void UpdateHealthUI(){
        healthTracker.UpdateSliderValue(unitHealth, unitMaxHealth);

        if (unitHealth <= 0) {
            Destroy(gameObject);
            PlayerBaseCtrl.Instance.ModifyIntensity(intensityMod);
        }
    }

    public void TakeDamage(float damageToInflict){
        unitHealth -= damageToInflict * defenseMod; // apply defense mod to reduce or increase damage taken

        UpdateHealthUI();
        if (isPlayer) {
            PlayerBaseCtrl.Instance.ModifyIntensity(dmgIntensityMod);
        } else {
            PlayerBaseCtrl.Instance.ModifyIntensity(-dmgIntensityMod);
        }
        Debug.Log($"Unit took {damageToInflict} damage. Remaining health: {unitHealth}");
    }

    public void UnitWasPlaced() {
        inPreviewMode = false;
        // Make health bar visible
        healthTracker.gameObject.SetActive(true);

        if (isPlayer) {
            PlayerBaseCtrl.Instance.GainXP(xpGained);
            PlayerBaseCtrl.Instance.ModifyIntensity(-intensityMod);
        }

        Debug.Log($"Unit placed at {transform.position}");
    }

    private void addToFallenUnits() {
        

    }
}
