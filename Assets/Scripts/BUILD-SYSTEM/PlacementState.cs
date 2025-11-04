using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class PlacementState : IBuildingState
{
    protected private int selectedObjectIndex = -1;
    protected int ID;
    protected Grid grid;
    protected PreviewSystem previewSystem;
    protected ObjectsDatabseSO database;
    protected GridData floorData;
    protected GridData furnitureData;
    protected ObjectPlacer objectPlacer;
    protected GridZoneManager gridZoneManager;
    protected GridVisualizer gridVisualizer;
    protected int rotIndex = 0;
    protected int variantIndex = 0;
    

    public PlacementState(int iD, Grid grid, PreviewSystem previewSystem, ObjectsDatabseSO database, GridData floorData, GridData furnitureData, ObjectPlacer objectPlacer, GridZoneManager gridZoneManager) {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.database = database;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;
        this.gridVisualizer = UnityEngine.Object.FindAnyObjectByType<GridVisualizer>();

        this.gridZoneManager = gridZoneManager;

        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex > -1) {
            previewSystem.StartShowingPlacementPreview(database.objectsData[selectedObjectIndex].Prefab,
                database.objectsData[selectedObjectIndex].Size);
        }
        else{
            throw new System.Exception($"No object with ID {iD}");
        }
    }

    public virtual void EndState() {
        previewSystem.StopShowingPreview();
        if (gridVisualizer != null) {
            gridVisualizer.SetPreviewCells(null);
        }
    }




    public virtual GameObject OnAction(Vector3Int gridPosition) {
        // Checking if we can place this item (position not occupied)
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        if (placementValidity == false) {
            return null;
        }

        ObjectData objectData = database.objectsData[selectedObjectIndex];
        GameObject prefabToPlace = objectData.PrefabVariants[variantIndex];
        Vector2Int sizeToUse = objectData.SizeVariants[variantIndex];

        int index = objectPlacer.PlaceObject(prefabToPlace, grid.CellToWorld(gridPosition), rotIndex * 90f, objectData.ID);

        // Create ref to track placed object (source for Effects)
        GameObject placedObject = null;

        ResourceManager.Instance.DecreaseResourcesBasedOnRequirements(database.objectsData[selectedObjectIndex]);
        
        // Get building type from the building that was just placed
        BuildingType buildingType = database.objectsData[selectedObjectIndex].thisBuildingType;
        ResourceManager.Instance.UpdateBuildingChanged(buildingType, true, new Vector3()); // passes in an empty vector for position so it won't cause an error       
        
        if (!objectData.isUnit) {
            GridData selectedData = floorData;
            selectedData.AddObjectAt(gridPosition,
                sizeToUse,
                objectData.ID,
                index);

            Vector3 buildingOrigin = grid.CellToWorld(gridPosition);

            Vector3 buildingCenter = buildingOrigin;
            buildingCenter.x += sizeToUse.x * grid.cellSize.x / 2;
            buildingCenter.z += sizeToUse.y * grid.cellSize.z / 2;

            int expandWidth = sizeToUse.x + objectData.baseExpandWidth * 2;
            int expandLength = sizeToUse.y + objectData.baseExpandLength * 2;

            // Get the cells that were added to the buildable zone by the new building
            List<Vector3Int> newBuildableCells = GetCellsAddedToBuildZone(buildingCenter, expandWidth, expandLength);

            // Get reference to placed object
            placedObject = objectPlacer.placedGameObjects[index];
            Constructable constructable = placedObject.GetComponent<Constructable>();

            // update the constructable added cells list with the new buildable cells
            if (constructable != null) {
                constructable.addedCells = newBuildableCells;
            }
            gridZoneManager.CreateBuildableZoneRectangle(buildingCenter, expandWidth, expandLength);
            PlayerBaseCtrl.Instance.UpdateGroveSize();
        } else {
            placedObject = objectPlacer.currentPlacedObj;
        }
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false);

        return placedObject;
    }






    // When you have more floor objects add their id here
    private List<int> GetAllFloorIDs() {
        return new List<int> { 11 }; // These are all the ids of floor items - For now its only the grass
    }

    // protected allows for overriding in child classes but keeps it private to other classes
    protected virtual bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex) {
        GridData selectedData = floorData;
        ObjectData objectData = database.objectsData[selectedObjectIndex];
        Vector2Int sizeToUse = objectData.SizeVariants[variantIndex]; 

        Vector2Int rotatedSize = sizeToUse;
        if (rotIndex == 1 || rotIndex == 3) {
            rotatedSize = new Vector2Int(sizeToUse.y, sizeToUse.x);
        }

        // Check to see if gid cell is already ocupied by another object from the GridData
        if (!selectedData.CanPlaceObjectAt(gridPosition, rotatedSize)) {
            return false;
        }

        if (gridZoneManager != null) {
            List<Vector3Int> positionsToOccupy = CalculatePositions(gridPosition, sizeToUse);
            foreach (var pos in positionsToOccupy) {
                if (!gridZoneManager.IsPositionBuildable(pos)) {
                    return false;
                }
            }
        }
        // Additional check for obstacles (trees, units, environment)
        Vector3 worldPosition = grid.CellToWorld(gridPosition);
        Collider[] colliders = Physics.OverlapBox(worldPosition, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity);

        foreach (var collider in colliders) {
            if (  collider.CompareTag("Resource") || collider.CompareTag("Obstacle")) {
                return false;
            }
        }
        return true;
    }

        // First method: Calculate which cells are occupied based on rotation
    protected List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int size) {
        List<Vector3Int> positionsToOccupy = new List<Vector3Int>();
        
        // Use the correct size based on rotation
        Vector2Int rotatedSize = size;
        if (rotIndex == 1 || rotIndex == 3) {
            rotatedSize = new Vector2Int(size.y, size.x);
        }
        
        for (int x = 0; x < rotatedSize.x; x++) {
            for (int y = 0; y < rotatedSize.y; y++) {
                Vector3Int offset;
                
                switch (rotIndex) {
                    case 0: // 0°
                        offset = new Vector3Int(x, 0, y);
                        break;
                    case 1: // 90°
                        offset = new Vector3Int(y, 0, -x);
                        break;
                    case 2: // 180°
                        offset = new Vector3Int(-x, 0, -y);
                        break;
                    case 3: // 270°
                        offset = new Vector3Int(-y, 0, x);
                        break;
                    default:
                        offset = new Vector3Int(x, 0, y);
                        break;
                }
                
                positionsToOccupy.Add(gridPosition + offset);
            }
        }
        
        return positionsToOccupy;
    }

    // Second method: Adjust the cursor/preview position based on rotation
    public virtual void UpdateState(Vector3Int gridPosition) {
        // Calculate adjusted position based on rotation
        Vector3Int adjustedPosition = gridPosition;
        ObjectData objectData = database.objectsData[selectedObjectIndex];
        Vector2Int sizeToUse = objectData.SizeVariants[variantIndex];
        
        // Apply position offset based on rotation
        switch (rotIndex) {
            case 1: // 90°
                adjustedPosition = new Vector3Int(gridPosition.x, gridPosition.y, gridPosition.z);
                break;
            case 2: // 180°
                adjustedPosition = new Vector3Int(gridPosition.x + sizeToUse.x - 1, gridPosition.y, gridPosition.z + sizeToUse.y - 1);
                break;
            case 3: // 270°
                adjustedPosition = new Vector3Int(gridPosition.x, gridPosition.y, gridPosition.z + sizeToUse.x - 1);
                break;
        }
        
        // Check validity with the adjusted position
        bool placementValidity = CheckPlacementValidity(adjustedPosition, selectedObjectIndex);
        
        // Update preview
        previewSystem.UpdatePosition(grid.CellToWorld(adjustedPosition), placementValidity);
        
        // Update grid visualizer
        if (gridVisualizer != null) {
            List<Vector3Int> positionsToOccupy = CalculatePositions(adjustedPosition, sizeToUse);
            gridVisualizer.SetPreviewCells(positionsToOccupy);
        }
    }

    // Update rotation method // abandoned for now in favor of using variants set up at different rotations
    public virtual void UpdateRotation(int dir, Vector3Int gridPosition) {
        // Update rotation index (0-3)
        rotIndex = (rotIndex + dir) % 4;
        if (rotIndex < 0) rotIndex += 4;
        
        // Update preview rotation
        previewSystem.UpdateRotation(rotIndex * 90f);
        
        // Update state to refresh preview and grid cells
        UpdateState(gridPosition);
    }

    public virtual void CycleVariant(Vector3Int gridPosition) {
        ObjectData objectData = database.objectsData[selectedObjectIndex];
        if (objectData.numVariants <= 1) return;

        variantIndex = (variantIndex + 1) % objectData.numVariants;

        //Update the preview with the new variant
        GameObject preFab = objectData.PrefabVariants[variantIndex];
        Vector2Int varSize = objectData.SizeVariants[variantIndex];

        previewSystem.StopShowingPreview();
        previewSystem.StartShowingPlacementPreview(preFab, varSize);

        UpdateState(gridPosition);
    }

    private List<Vector3Int> GetCellsAddedToBuildZone(Vector3 zoneCenter, int width, int length) {
        List<Vector3Int> cells = new List<Vector3Int>();
        Vector3Int centerCell = grid.WorldToCell(zoneCenter);
        int halfWidth = width / 2;
        int halfLength = length / 2;

        for (int x = -halfWidth; x <= halfWidth; x++) {
            for (int z = -halfLength; z <= halfLength; z++) {
                Vector3Int cellPos = centerCell + new Vector3Int(x, 0, z);
                cells.Add(cellPos);
            }
        }
        return cells;
    }

}
