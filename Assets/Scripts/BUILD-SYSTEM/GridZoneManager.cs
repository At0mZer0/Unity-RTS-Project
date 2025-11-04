using System;
using System.Collections.Generic;
using UnityEngine;

public class GridZoneManager : MonoBehaviour {

    public static GridZoneManager Instance { get; private set; }
    private HashSet<Vector3Int> buildableZone = new HashSet<Vector3Int>();
    
    // For mirroring the buildableZone HashSet for debugging purposes
    [SerializeField, Tooltip("Read-only display of buildable cells")]
    private List<Vector3Int> debugBuildableZoneList = new List<Vector3Int>();

    [SerializeField] private Grid grid;
    // [SerializeField] private bool showDebug = false;
    [SerializeField] private Color debugColor = Color.red;


    private void UpdateDebugList() {
        debugBuildableZoneList.Clear();
        debugBuildableZoneList.AddRange(buildableZone);
    }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }
    }

    void Start() {
        if (grid == null) grid = FindAnyObjectByType<Grid>();


    }

    public bool IsPositionBuildable(Vector3Int cellPos) {
        return buildableZone.Contains(cellPos);
    }

    // add the single Cell at this position to the buildable zone
    public void AddBuildablePosition(Vector3Int cellPos) {
        buildableZone.Add(cellPos);
        UpdateDebugList();
    }

    public void RemoveBuildablePosition(Vector3Int cellPos) {
        buildableZone.Remove(cellPos);
        UpdateDebugList();
    }

    public void ClearBuildableZone() {
        buildableZone.Clear();
        UpdateDebugList();
    }

    public Grid GetGrid() {
        return grid;
    }
    /// pass in the position of the center of the object and then round the radius up to the nearest int to snap to grid, then 
    public List<Vector3Int> CreateBuildableZoneCircle(Vector3 zoneCenter, float radius) {
        List<Vector3Int> addedCells = new List<Vector3Int>();
        Vector3Int centerCell = grid.WorldToCell(zoneCenter);
        int cellRadius = Mathf.CeilToInt(radius / grid.cellSize.x); // Rounds up to the nearest int to snap to grid cell

        for (int x = -cellRadius; x <= cellRadius; x++) {                           //----- The Loop is making a square search area of cells to loop through around the object in the grid using the radius 
            for (int z = -cellRadius; z <= cellRadius; z++) {
                if (x*x + z*z <= cellRadius*cellRadius) {                           //----- Uses good ole a2 + b2 = c2 to see if it's within the circle 
                    Vector3Int cellPos = centerCell + new Vector3Int(x, 0, z);
                    buildableZone.Add(cellPos);
                    addedCells.Add(cellPos);
                }
            }
        }
        UpdateDebugList();
        return addedCells;
    } 


    // create rectangular buildable zone
    public void CreateBuildableZoneRectangle(Vector3 zoneCenter, int width, int length) {
        Vector3Int centerCell = grid.WorldToCell(zoneCenter);
        int centerWidth = width / 2;
        int centerLength = length / 2;

        for (int x = -centerWidth; x <= centerWidth; x++) {
            for (int z = -centerLength; z <= centerLength; z++) {
                Vector3Int cellPos = centerCell + new Vector3Int(x, 0, z);
                buildableZone.Add(cellPos);
            }
        }
        UpdateDebugList();
    }

    // Expands the buildable zone when base is upgrade or other reasons
    public void ExpandBuildableZone(int expansionCells) {
        HashSet<Vector3Int> newPositions = new HashSet<Vector3Int>();
        
        foreach (Vector3Int cell in buildableZone) {
            for (int x = -expansionCells; x <= expansionCells; x++) {
                for (int z = -expansionCells; x <= expansionCells; z++) {
                    Vector3Int newPos = cell + new Vector3Int(x, 0, z);
                    newPositions.Add(newPos);
                }
            }
            buildableZone.UnionWith(newPositions);
        }
    }

    public int GetBuildableZoneCount() {
        return buildableZone.Count;
    }

}
