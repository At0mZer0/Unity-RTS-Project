using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class ChainPlacementState : PlacementState {
    
    private Vector3Int startPosition;
    private bool isFirstClick = true;
    private List<Vector3Int> chainPositions = new List<Vector3Int>();
    private ObjectData objectData;
    private float spacing;

    // make a constructor like in PlacementState, uses base: to inherit the parent base constructor parameters
    public ChainPlacementState(int iD, Grid grid, PreviewSystem previewSystem, ObjectsDatabseSO database, GridData floorData, GridData furnitureData, ObjectPlacer objectPlacer, GridZoneManager gridZoneManager) 
        : base (iD, grid, previewSystem, database, floorData, furnitureData, objectPlacer, gridZoneManager) {

            // add in the parameters that are unique to Chain Placement 
            objectData = database.GetObjectByID(iD);
            spacing = objectData.chainSpacing; // will set this to the Length of the object set by X
        }

    public override void EndState() {
        base.EndState();
        isFirstClick = true;
        chainPositions.Clear();
    }

    public override GameObject OnAction(Vector3Int gridPosition) {
        if (isFirstClick) {
            startPosition = gridPosition;
            isFirstClick = false;
            Debug.Log($"Chain start point set at {startPosition}");

        } else {
            bool canAffordAll = CheckResourcesForChain();

            if (canAffordAll) {
                foreach (var pos in chainPositions) {
                    // Place object at each position in the chain
                    int index = objectPlacer.PlaceObject(objectData.Prefab, grid.CellToWorld(pos));

                    // Update the grid data 
                    GridData gridData = floorData;
                    gridData.AddObjectAt(pos, objectData.Size, objectData.ID, index);

                    // Remove resources used in placement
                    ResourceManager.Instance.DecreaseResourcesBasedOnRequirements(objectData);
                } 
            ResourceManager.Instance.UpdateBuildingChanged(objectData.thisBuildingType, true, Vector3.zero);
            } else {
                Debug.Log("Not enough resources to place all objects in the chain.");   
            }

            // Reset the state for next placement
            isFirstClick = true;
            chainPositions.Clear(); // clear the list of positions used
        }
        return null;
    }

    private void CalculateChainPositions(Vector3Int start, Vector3Int end) {
        chainPositions.Clear();

        // Get the direction from start to end
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        Vector3 normalized = direction.normalized;

        //calculate number of objects in chain
        int objectCount = Mathf.CeilToInt(distance / spacing);

        // Add start position
        chainPositions.Add(start);

        // Calculate next positions in chain
        for (int i = 1; i < objectCount; i++) {
            // get world position
            Vector3 worldPos = grid.CellToWorld(start) + (normalized * spacing * i);

            // Convert to grid cell position
            Vector3Int gridPos = grid.WorldToCell(worldPos);

            // Only add if it's a new position
            if (!chainPositions.Contains(gridPos)) {
                chainPositions.Add(gridPos);
            }
        }

        // Add end position if not in list
        if (!chainPositions.Contains(end)) {
            chainPositions.Add(end);
        }

    }

    private bool CheckResourcesForChain() {
        int count = chainPositions.Count;

        // loop through the Build Requirements to check amount required

        foreach (BuildRequirement req in objectData.resourceRequirements) {
            int totalRequired = req.amount * count;
            int available = ResourceManager.Instance.GetResourceAmount(req.resource);

            if (available < totalRequired) {
                return false;
            }
        }
        return true;
    }

    protected override bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex) {
        GridData selectedData = floorData;

        if (!selectedData.CanPlaceObjectAt(gridPosition, objectData.Size)){
            return false;
        }

        if (gridZoneManager != null) {
            List<Vector3Int> positionsToOccupy = CalculatePositions(gridPosition, objectData.Size);
            foreach (var pos in positionsToOccupy) {
                if (!gridZoneManager.IsPositionBuildable(pos)) {
                    return false;
                }
            }
        }
        return true;

    }
    
    private void ShowChainPreview(bool isValid) {
        List<Vector3> worldPositions = new List<Vector3>();

        foreach (var pos in chainPositions) {
            Vector3 worldPos = grid.CellToWorld(pos);
            previewSystem.UpdatePosition(worldPos, isValid);
        }

        previewSystem.UpdateMultiplePositions(worldPositions, isValid);
    }



    private void UpdateGridVisualizerPreview() {
        // create a list of all cells for gird visual
        List<Vector3Int> allCells = new List<Vector3Int>();

        foreach (var pos in chainPositions) {
            allCells.AddRange(CalculatePositions(pos, objectData.Size));
        }

        if (GridVisualizer.Instance != null) {
            GridVisualizer.Instance.SetPreviewCells(allCells);;
        }
    }

    public override void UpdateState(Vector3Int gridPosition) {
       if (isFirstClick) {
            base.UpdateState(gridPosition);
       } else {
            CalculateChainPositions(startPosition, gridPosition);

            bool allValid = true;
            foreach (var pos in chainPositions) {
                if (!CheckPlacementValidity(pos, objectData.ID )) {
                    allValid = false;
                    break;
                }
            }

            bool canAfford = CheckResourcesForChain();
            ShowChainPreview(allValid && canAfford);
            UpdateGridVisualizerPreview();
       }
    }

    public override void UpdateRotation(int dir, Vector3Int gridPosition) {
    base.UpdateRotation(dir, gridPosition);

    //if in the second click phase recalculate chain positions
    if (!isFirstClick && startPosition != Vector3Int.zero) {
        CalculateChainPositions(startPosition, gridPosition);
    }
}

}
