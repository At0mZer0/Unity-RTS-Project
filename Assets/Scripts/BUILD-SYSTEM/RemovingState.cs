using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovingState : IBuildingState {
    private int gameObjectIndex = -1;
    Grid grid;
    PreviewSystem previewSystem;
    GridData floorData;
    GridData furnitureData;
    ObjectPlacer objectPlacer;

    public RemovingState(Grid grid, PreviewSystem previewSystem, GridData floorData, GridData furnitureData, ObjectPlacer objectPlacer) {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;

        previewSystem.StartShowingRemovePreview();
    }

    public void EndState() {
        previewSystem.StopShowingPreview();
    }

    public GameObject OnAction(Vector3Int gridPosition) {
        GridData selectedData = null;
        if (furnitureData.CanPlaceObjectAt(gridPosition, Vector2Int.one) == false) {
            selectedData = furnitureData;
        }
        else if (floorData.CanPlaceObjectAt(gridPosition, Vector2Int.one) == false){
            selectedData = floorData;
        }

        if (selectedData == null){
            // Nothing to remove
        }
        else {
            gameObjectIndex = selectedData.GetRepresentationIndex(gridPosition);
            if (gameObjectIndex == -1)
                return null;

            GameObject buildingToSell = objectPlacer.GetObjectAt(gameObjectIndex);

            if (buildingToSell != null && buildingToSell.GetComponent<Constructable>() != null) {
                ResourceManager.Instance.SellBuilding(buildingToSell.GetComponent<Constructable>().buildingType, buildingToSell);
            } else {
                selectedData.RemoveObjectAt(gridPosition);
                objectPlacer.RemoveObjectAt(gameObjectIndex);
            }
        }
        Vector3 cellposition = grid.CellToWorld(gridPosition);
        previewSystem.UpdatePosition(cellposition, CheckIfSelectionIsValid(gridPosition));

        return null;
    } 

    private bool CheckIfSelectionIsValid(Vector3Int gridPosition) {
        return !(furnitureData.CanPlaceObjectAt(gridPosition, Vector2Int.one) &&
            floorData.CanPlaceObjectAt(gridPosition, Vector2Int.one));
    }

    public void UpdateState(Vector3Int gridPosition){
        bool validity = CheckIfSelectionIsValid(gridPosition);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), validity);
    }

    public void UpdateRotation(int direction, Vector3Int gridPosition) {
        // No rotation needed for removal state, but we need to satisfy the interface
        // Just update the preview position
        UpdateState(gridPosition);
    }
}
