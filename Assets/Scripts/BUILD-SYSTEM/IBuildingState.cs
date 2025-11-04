using UnityEngine;

public interface IBuildingState {
    void EndState();
    GameObject OnAction(Vector3Int gridPosition);
    void UpdateState(Vector3Int gridPosition);
    void UpdateRotation(int direction, Vector3Int gridPosition); 
}