using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Constructable : MonoBehaviour, IDamageable {

    public float constHealth;
    public float constMaxHealth;
    public HealthTracker healthTracker;

    public bool isEnemy = false;

    public BuildingType buildingType;
    public Vector3 buildPosition;
    NavMeshObstacle obstacle;

    public bool inPreviewMode;

    public float defenseMod = 1.0f;
    public float attackMod = 1.0f;
    public float attackRate = 1f;

    public float xpGained = 5f;
    public float dmgIntensityMod = 1.2f;
    public float intensityMod = 10f;

    /// This keeps track of the cells added to the base so I can remove them when the building is destroyed
    // [HideInInspector]
    public List<Vector3Int> addedCells = new List<Vector3Int>();            

    private void Start() {
        constHealth = constMaxHealth;
        UpdateHealthUI();
        
    }

    private void UpdateHealthUI() {
        healthTracker.UpdateSliderValue(constHealth, constMaxHealth);
        
        if (constHealth <= 0) {
            ObjectPlacer.Instance.placedGameObjects.Remove(gameObject);
            ResourceManager.Instance.UpdateBuildingChanged(buildingType, false, buildPosition);

            SoundManager.Instance.PlayBuildingDestructionSound();
            PlayerBaseCtrl.Instance.ModifyIntensity(intensityMod); // increase intensity
            
            Destroy(gameObject);
        }
    }

    private void OnDestroy() {
        if (inPreviewMode == false) {
            // Remove this buildings buildable cells from the grid
            if (buildingType != BuildingType.SacredTree && addedCells.Count > 0) {
                RemoveBuildableCells();
            }
        }
    }

    public void TakeDamage(float damage) {
        constHealth -= damage * defenseMod; // apply defense mod to reduce or increase damage taken
        
        PlayerBaseCtrl.Instance.ModifyIntensity(dmgIntensityMod); // increase intensity
        UpdateHealthUI();
        
        Debug.Log($"Constructable took {damage} damage. Remaining health: {constHealth}");
    }

    public void ConstructableWasPlaced(Vector3 position) {
        buildPosition = position;

        inPreviewMode = false;
        // Make health bar visible
        healthTracker.gameObject.SetActive(true);
        
        ActivateObstacle();
        
        if (isEnemy) {
            gameObject.tag = "Enemy";
        }

        if (GetComponent<ManaUser>() != null) {
            GetComponent<ManaUser>().PowerOn();
        }

        PlayerBaseCtrl.Instance.GainXP(xpGained);
        PlayerBaseCtrl.Instance.ModifyIntensity(-intensityMod);
        
            
    }

    private void RemoveBuildableCells() {
        PlacementSystem placementSystem = FindFirstObjectByType<PlacementSystem>();
        
        GridData occupiedGridCells = placementSystem.GetFloorData();

        // create a hashset to ensure no duplicates are added
        HashSet<Vector3Int> cellsUsedByOtherBuildings = new HashSet<Vector3Int>();

        foreach (GameObject obj in ObjectPlacer.Instance.placedGameObjects) {
            // skip null objects or this object
            if (obj == null || obj == gameObject) continue;

            // Get constructable component
            Constructable otherConstructable = obj.GetComponent<Constructable>();
            if (otherConstructable != null) {
                // add all cells from the other building in the don't remove set
                foreach (Vector3Int cell in otherConstructable.addedCells) {
                    cellsUsedByOtherBuildings.Add(cell);
                }
            }
        }

        // now check each cell this building added
        foreach (Vector3Int cell in addedCells) {
            // 1. Only remove if cell isn't occupied by another building
            // 2. Vector2Int.one is used to check that one specific cell
            if (occupiedGridCells.CanPlaceObjectAt(cell, Vector2Int.one) && !cellsUsedByOtherBuildings.Contains(cell)) {
                GridZoneManager.Instance.RemoveBuildablePosition(cell);
            }
        }            
    }

    private void ActivateObstacle() {
        obstacle = GetComponentInChildren<NavMeshObstacle>();
        obstacle.enabled = true;
    }
}
