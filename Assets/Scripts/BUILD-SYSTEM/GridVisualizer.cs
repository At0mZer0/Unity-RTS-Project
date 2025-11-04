using System.Collections.Generic;
using UnityEngine;

public class GridVisualizer : MonoBehaviour {
    public static GridVisualizer Instance {get; set;} // This is a Singleton

    // Makes sure this is the only one 
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    [SerializeField] private Grid grid;
    [SerializeField] private PlacementSystem placementSystem;
    [SerializeField] private bool showGrid = true;
    [SerializeField] private float yOffset = 0.05f;

    [Header("Visualization Area")]
    [SerializeField] private int gridWidth = 40;
    [SerializeField] private int gridLength = 40;
    [SerializeField] private Vector3 gridCenter = Vector3.zero;

    [Header("Colors")]
    [SerializeField] private Color buildableColor = new Color(0, 1, 0, 0.3f);
    [SerializeField] private Color occupiedColor = new Color(1, 0, 0, 0.3f);
    [SerializeField] private Color gridLineColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);

    public Material occupiedMaterial;
    public Material unOccupiedMaterial;
    public Material gridLineMaterial;
    
    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.5f; // How often to update grid colors
    
    private GridZoneManager gridZoneManager;
    private Transform gridParent;
    private Dictionary<Vector3Int, GameObject> gridCells = new Dictionary<Vector3Int, GameObject>();
    private List<LineRenderer> gridLines = new List<LineRenderer>();
    private float updateTimer = 0;
    private bool isInitialized = false;

    private HashSet<Vector3Int> previewCells = new HashSet<Vector3Int>();
    [SerializeField] private Color previewColor = new Color(1, 0.5f, 0, 0.5f);



    void Start() {
        if (grid == null) grid = FindAnyObjectByType<Grid>();
        if (placementSystem == null) placementSystem = FindAnyObjectByType<PlacementSystem>();
        gridZoneManager = GridZoneManager.Instance;
        
        if (showGrid) {
            CreateVisibleGrid();
        }
    }
    
    void Update() {
        // Toggle grid visibility
        if (showGrid && gridParent == null) {
            CreateVisibleGrid();
        } else if (!showGrid && gridParent != null) {
            DestroyGrid();
        }
        
        // Periodically update colors
        if (showGrid && isInitialized) {
            updateTimer -= Time.deltaTime;
            if (updateTimer <= 0) {
                UpdateGridColors();
                updateTimer = updateInterval;
            }
        }
    }
    
    public void SetPreviewCells(List<Vector3Int> cells) {
        previewCells.Clear();
        if (cells != null) {
            foreach (var cell in cells) {
                previewCells.Add(cell);
            }
        }
        if (isInitialized) {
            UpdateGridColors();
        }
    }   

    public void SetGridVisibility(bool visible) {
        showGrid = visible;
        if (gridParent != null) {
            gridParent.gameObject.SetActive(visible);
        }
    }
    
    public void CreateVisibleGrid() {
        // Create parent object
        gridParent = new GameObject("GridVisual").transform;
        gridParent.SetParent(transform);
        
        // Create material for cells
        Material cellMaterial = occupiedMaterial;
        
        Vector3 cellSize = grid.cellSize;
        Vector3Int centerCell = grid.WorldToCell(gridCenter);
        int halfWidth = gridWidth / 2;
        int halfLength = gridLength / 2;
        
        // Create cell objects
        for (int x = -halfWidth; x < halfWidth; x++) {
            for (int z = -halfLength; z < halfLength; z++) {
                Vector3Int cellPos = centerCell + new Vector3Int(x, 0, z);
                Vector3 worldPos = grid.CellToWorld(cellPos) + new Vector3(cellSize.x / 2, yOffset, cellSize.z / 2);
                
                // Create cell quad
                GameObject cellObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                cellObj.name = $"Cell_{x}_{z}";
                cellObj.transform.SetParent(gridParent);
                cellObj.transform.position = worldPos;
                cellObj.transform.rotation = Quaternion.Euler(90, 0, 0); // Face upward
                cellObj.transform.localScale = new Vector3(cellSize.x * 0.9f, cellSize.z * 0.9f, 1);
                
                // Remove collider (we don't need physics)
                Destroy(cellObj.GetComponent<Collider>());
                
                // Setup material
                Renderer renderer = cellObj.GetComponent<Renderer>();
                renderer.material = new Material(cellMaterial);
                
                gridCells[cellPos] = cellObj;
            }
        }
        
        // Create grid lines
        CreateGridLines(centerCell, halfWidth, halfLength, cellSize);
        
        // Set initial colors
        UpdateGridColors();
        isInitialized = true;
    }
    
    private void CreateGridLines(Vector3Int centerCell, int halfWidth, int halfLength, Vector3 cellSize) {
        Material lineMaterial = gridLineMaterial;
        lineMaterial.color = gridLineColor;
        
        // Create horizontal lines
        for (int x = -halfWidth; x <= halfWidth; x++) {
            GameObject lineObj = new GameObject($"LineH_{x}");
            lineObj.transform.SetParent(gridParent);
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.startWidth = line.endWidth = 0.02f;
            line.positionCount = 2;
            
            Vector3 start = grid.CellToWorld(centerCell + new Vector3Int(x, 0, -halfLength));
            Vector3 end = grid.CellToWorld(centerCell + new Vector3Int(x, 0, halfLength));
            start.y = yOffset;
            end.y = yOffset;
            
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            gridLines.Add(line);
        }
        
        // Create vertical lines
        for (int z = -halfLength; z <= halfLength; z++) {
            GameObject lineObj = new GameObject($"LineV_{z}");
            lineObj.transform.SetParent(gridParent);
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.startWidth = line.endWidth = 0.02f;
            line.positionCount = 2;
            
            Vector3 start = grid.CellToWorld(centerCell + new Vector3Int(-halfWidth, 0, z));
            Vector3 end = grid.CellToWorld(centerCell + new Vector3Int(halfWidth, 0, z));
            start.y = yOffset;
            end.y = yOffset;
            
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            gridLines.Add(line);
        }
    }
    
    private void UpdateGridColors() {
        Vector3Int centerCell = grid.WorldToCell(gridCenter);
        int halfWidth = gridWidth / 2;
        int halfLength = gridLength / 2;
        
        for (int x = -halfWidth; x < halfWidth; x++) {
            for (int z = -halfLength; z < halfLength; z++) {
                Vector3Int cellPos = centerCell + new Vector3Int(x, 0, z);
                
                if (gridCells.TryGetValue(cellPos, out GameObject cellObj)) {
                    bool isOccupied = IsCellOccupied(cellPos);
                    bool isBuildable = IsCellBuildable(cellPos);
                    bool isPreview = previewCells.Contains(cellPos);
                    
                    Renderer renderer = cellObj.GetComponent<Renderer>();
                    if (isPreview) {
                        renderer.material.color = previewColor;
                        cellObj.SetActive(true);
                    }
                    else if (isOccupied) {
                        renderer.material.color = occupiedColor;
                        cellObj.SetActive(true);
                    } else if (isBuildable) {
                        renderer.material.color = buildableColor;
                        cellObj.SetActive(true);
                    } else {
                        // Hide cells that are neither occupied nor buildable
                        cellObj.SetActive(false);
                    }
                }
            }
        }
    }
    
    private void DestroyGrid() {
        if (gridParent != null) {
            Destroy(gridParent.gameObject);
            gridParent = null;
        }
        
        gridCells.Clear();
        gridLines.Clear();
        isInitialized = false;
    }
    
    private bool IsCellBuildable(Vector3Int cellPos) {
        // First check if cell is occupied
        if (IsCellOccupied(cellPos)) {
            return false;
        }
        
        // If we have a zone manager, check if in the build zone
        if (gridZoneManager != null) {
            return gridZoneManager.IsPositionBuildable(cellPos);
        }
        
        // Without zone manager, all unoccupied cells are buildable
        return true;
    }
    
    private bool IsCellOccupied(Vector3Int cellPos) {
        if (placementSystem == null) return false;
        
        GridData floorData = placementSystem.GetFloorData();
        
        if (floorData != null) {
            return !floorData.CanPlaceObjectAt(cellPos, Vector2Int.one);
        }
        
        return false;
    }
    
    void OnDestroy() {
        DestroyGrid();
    }
}