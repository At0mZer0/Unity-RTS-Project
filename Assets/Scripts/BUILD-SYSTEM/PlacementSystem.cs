using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour {

    [SerializeField] private InputManager inputManager;
    [SerializeField] private Grid grid;
    [SerializeField] private ObjectsDatabseSO database;
    [SerializeField] private GridData floorData, furnitureData; // floor things like roads, furniture change to "buildings"
    [SerializeField] private PreviewSystem previewSystem;
    private Vector3Int lastDetectedPosition = Vector3Int.zero;
    [SerializeField] private ObjectPlacer objectPlacer;
    [SerializeField] private GridZoneManager gridZoneManager;

    int selectedID;
    IBuildingState buildingState;
    public bool inSellMode;
    PlayerBaseCtrl playerBase;

    private void Start() {
        floorData = new();
        furnitureData = new();
        playerBase = PlayerBaseCtrl.Instance;
    }

    public void StartPlacement(int ID) {
        Debug.Log("Should Start Placement");
        selectedID = ID;
        Debug.Log("Placement ID: " + ID);

        StopPlacement();

        // ---- Check if the object being placed is chainable -----
        ObjectData objectData = database.GetObjectByID(ID);

        if (objectData.isChainable) {
            buildingState = new ChainPlacementState(ID, grid, previewSystem, database, floorData, furnitureData, objectPlacer, gridZoneManager);
        } else {
            buildingState = new PlacementState(ID, grid, previewSystem, database, floorData, furnitureData, objectPlacer, gridZoneManager);
        }

        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
        // inputManager.OnRotated += RotateStructure;
        inputManager.OnVariantCycled += CycleVariant; // Added to cycle through variants
    }

    // allows us to remove 
    public void RemovePlacementData(Vector3 position) {
        Vector3Int cellPosition = grid.WorldToCell(position);

        if (floorData.HasObjectAt(cellPosition)){
            floorData.RemoveObjectAt(grid.WorldToCell(position));
        }
    }



    public void StartRemoving() {
        StopPlacement();

        buildingState = new RemovingState(grid, previewSystem, floorData, furnitureData, objectPlacer);

        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;

        inputManager.OnClicked += EndSelling;
        inputManager.OnExit += EndSelling;
    }

    /// REVIEW
    private void EndSelling() {
        inSellMode = false;
        // CursorManager.Instance.SetMarkerType(CursorManager.CursorType.None);
    }

    private void PlaceStructure() {
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        // buildingState.OnAction(gridPosition);
        

        // Get the actual object that was just placed
        GameObject actualPlacedObject = buildingState.OnAction(gridPosition);

        // Only proceed if we're not in sell mode (selectedID will be valid)
        if (buildingState is not RemovingState && actualPlacedObject != null) {
            ObjectData ob = database.GetObjectByID(selectedID);

            foreach (Benefits bf in ob.benefits) {
                CalculateAndAddBenefit(bf, actualPlacedObject);
            }
        }
        StopPlacement();
    }

    ///----------------------------------------------------- need to implement eventually, ended up going with a Prefab and Size array to index to Prefabs in different orientations
    private void RotateStructure(int dir) {
        if (buildingState == null)
            return;
            
        // Only toggle between wall types if current selected ID is 4 or 5
        if (selectedID == 4 || selectedID == 5) {
            // Toggle between IDs 4 and 5
            selectedID = (selectedID == 4) ? 5 : 4;
            
            // Store current mouse position
            Vector3 mousePosition = inputManager.GetSelectedMapPosition();
            Vector3Int gridPosition = grid.WorldToCell(mousePosition);
            
            // Restart placement with the new ID
            StopPlacement();
            
            // Check if the new object is chainable
            ObjectData objectData = database.GetObjectByID(selectedID);
            
            if (objectData.isChainable) {
                buildingState = new ChainPlacementState(selectedID, grid, previewSystem, database, 
                    floorData, furnitureData, objectPlacer, gridZoneManager);
            } else {
                buildingState = new PlacementState(selectedID, grid, previewSystem, database, 
                    floorData, furnitureData, objectPlacer, gridZoneManager);
            }
            
            // Re-register events
            inputManager.OnClicked += PlaceStructure;
            inputManager.OnExit += StopPlacement;
            // inputManager.OnRotated += RotateStructure;
            
            // Update preview at current position
            buildingState.UpdateState(gridPosition);
        }
    }

    private void CycleVariant(Vector3Int gridPosition) {
        if (buildingState is PlacementState placementState) {
            placementState.CycleVariant(gridPosition);
        }
    }

    private void CalculateAndAddBenefit(Benefits bf, GameObject sourceObj) {
        PlayerBaseCtrl.Instance.ApplyBenefits(bf, sourceObj);
    }

    private void StopPlacement() {
        if (buildingState == null)
            return;
       
        buildingState.EndState();

        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
        inputManager.OnVariantCycled -= CycleVariant; 

        inputManager.OnClicked -= EndSelling;
        inputManager.OnExit -= EndSelling;

        lastDetectedPosition = Vector3Int.zero;

        buildingState = null;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            inSellMode = true;
            StartRemoving();
        }
        
        
        // We return because we did not selected an item to place (not in placement mode)
        // So there is no need to show cell indicator
        if (buildingState == null)
            return;
      
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        if (lastDetectedPosition != gridPosition){
            buildingState.UpdateState(gridPosition);
            lastDetectedPosition = gridPosition;
        }

    }

    // Getters for Grid debugger

    public GridData GetFloorData() {
        return floorData;
    }

    public GridData GetFurnitureData() {
        return furnitureData;
    }


}
