using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class SpawnController : MonoBehaviour {

    public static SpawnController Instance { get; private set; } // Singleton instance

    public GameObject[] spawnPoints;  
    public GameObject[] enemyRomans;
    public GameObject[] enemyFae;
    
    private GridZoneManager gridZoneManager;

    [Header("Wave Settings")]
    public float intensityMod = 20;
    [SerializeField] private float timeBetweenWave = 60f;
    [SerializeField] private int minEnemies = 1;
    [SerializeField] private int maxEnemies = 5;
    [SerializeField] private float spawnOffset = 10f;
    [SerializeField] private int waveEnemyIncrease = 4;

    [Header("Spawned Enemy Lists")]
    // Spawned in Enemies for Buffing and Debuffing
    public List<GameObject> spawnedRomans = new List<GameObject>();
    public List<GameObject> spawnedFae = new List<GameObject>();
    public List<GameObject> allSpawnedEnemy = new List<GameObject>();
    
    [Header("Debug Timer Enemy Wave")]
    [SerializeField] private float timeUntilNextWave = 120f;

    [Header("ResourceNode Resource Qty to Spawn")]
    public int minResourceQty = 20;
    public int maxResourceQty = 32;

    public GameObject[] stoneResourceNodes;
    public GameObject[] woodResourceNodes;
    public GameObject[] foodResourceNodes; 
    public List<GameObject> spawnedResourceNodes = new List<GameObject>();

    [Header("ResourceNode Node Qty to Spawn")]
    [SerializeField] private int minStone = 5;
    [SerializeField] private int maxStone = 10;
    [SerializeField] private int minWood = 5;
    [SerializeField] private int maxWood = 10;
    [SerializeField] private int minFood = 5;
    [SerializeField] private int maxFood = 10;
    [SerializeField] private float nodeSpawnRadius = 200f;
    [SerializeField] private float minDistBetweenNodes = 5f;
    [SerializeField] private LayerMask nodeSpawnAvoidLayers;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }
    
    void Start() {
        if (gridZoneManager == null) {
                gridZoneManager = GridZoneManager.Instance;
        }
        StartCoroutine(BaseInitThenSpawnResources());

        timeUntilNextWave = timeBetweenWave;
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.L)) {
            SpawnEnemy();
        }
        timeUntilNextWave -= Time.deltaTime;

        // creates a new wave after the time has passed
        if (timeUntilNextWave <= 0) {
            GenerateWave();

            minEnemies += waveEnemyIncrease;
            maxEnemies += waveEnemyIncrease;

            // reset timer for next wave
            timeUntilNextWave = timeBetweenWave; // Reset the timer for the next wave
        }
    }

    private IEnumerator BaseInitThenSpawnResources() {
        yield return new WaitUntil(() => PlayerBaseCtrl.baseInit); // wait for base to initialize so resource nodes don't spawn in player base
        SpawnResourceNodes();
    }

    #region Spawn Enemy
    public void UpdateSpawnedEnemyList() {
        allSpawnedEnemy.Clear();
        allSpawnedEnemy.AddRange(spawnedRomans);
        allSpawnedEnemy.AddRange(spawnedFae);

    }
    
    public void SpawnEnemy() {
        bool spawnRomans = Random.value > 0.5f;
        string unitTag = spawnRomans ? "Roman" : "Fae";
        GameObject[] selectedEnemy = spawnRomans ? enemyRomans : enemyFae;

        foreach (GameObject spawnPoint in spawnPoints) {
            if (spawnPoint.CompareTag(unitTag)) {
                int qtyOfEnemies = 1;                                // +1 allows for the max value to happen otherwise it'll always be 1 less than the max specified0

                for (int i = 0; i < qtyOfEnemies; i++) {                                                    // Spawn enemies and pick a random prefab from the selected enemy type (will use when I have more than 1 type of Fae or Roman)
                    GameObject prefabToSpawn = selectedEnemy[Random.Range(0, selectedEnemy.Length)];
                    Vector3 spawnPosOffset = new Vector3(Random.Range(-spawnOffset, spawnOffset), 0, Random.Range(-spawnOffset, spawnOffset));  // Randomly generates a position offset to spawn the enemies around the spawn point
                    GameObject newEnemy = Instantiate(prefabToSpawn, spawnPoint.transform.position + spawnPosOffset, Quaternion.identity);
                    Enemy enemy = newEnemy.GetComponent<Enemy>();
                    enemy.enemyType = unitTag; // Set the enemy type based on the spawn point's tag

                    switch (unitTag) {
                        case "Roman":
                            spawnedRomans.Add(newEnemy);
                            break;
                        case "Fae":
                            spawnedFae.Add(newEnemy);
                            break;
                        default:
                            Debug.LogError("Unknown enemy type: " + unitTag);
                            break;
                    }
                }
            }
        }
        UpdateSpawnedEnemyList();
    }

    public void GenerateWave() {
        if (gridZoneManager == null) {
            Debug.LogError("GridZoneManager not found! Enemies won't have targets.");
            return;
        }
        PlayerBaseCtrl.Instance.ModifyIntensity(intensityMod);
        // Randomly gives a value between 0 & 1 to decide to spawn Romans or Fae
        bool spawnRomans = Random.value > 0.5f;           
        string unitTag = spawnRomans ? "Roman" : "Fae";                  
        GameObject[] selectedEnemy = spawnRomans ? enemyRomans : enemyFae; 

        foreach ( GameObject spawnPoint in spawnPoints) {
            if (spawnPoint.CompareTag(unitTag)) {
                int qtyOfEnemies = Random.Range(minEnemies, maxEnemies + 1);                                // +1 allows for the max value to happen otherwise it'll always be 1 less than the max specified0

                for (int i = 0; i < qtyOfEnemies; i++) {                                                    // Spawn enemies and pick a random prefab from the selected enemy type (will use when I have more than 1 type of Fae or Roman)
                    GameObject prefabToSpawn = selectedEnemy[Random.Range(0, selectedEnemy.Length)];
                    Vector3 spawnPosOffset = new Vector3(Random.Range(-spawnOffset, spawnOffset), 0, Random.Range(-spawnOffset, spawnOffset));  // Randomly generates a position offset to spawn the enemies around the spawn point
                    GameObject newEnemy = Instantiate(prefabToSpawn, spawnPoint.transform.position + spawnPosOffset, Quaternion.identity);
                    Enemy enemy = newEnemy.GetComponent<Enemy>();
                    enemy.enemyType = unitTag; // Set the enemy type based on the spawn point's tag

                    switch (unitTag) {
                        case "Roman":
                            spawnedRomans.Add(newEnemy);
                            break;
                        case "Fae":
                            spawnedFae.Add(newEnemy);
                            break;
                        default:
                            Debug.LogError("Unknown enemy type: " + unitTag);
                            break;
                    }
                }
            }
        }
    }
    #endregion

    #region Spawn Resource Nodes
    private void SpawnResourceNodes() {
        List<Vector3> usedPositions = new List<Vector3>();

        int stoneQty = Random.Range(minStone, maxStone + 1);
        SpawnNodesOfType(stoneResourceNodes, stoneQty, usedPositions);
        
        int woodQty = Random.Range(minWood, maxWood + 1);
        SpawnNodesOfType(woodResourceNodes, woodQty, usedPositions);

        int foodQty = Random.Range(minFood, maxFood + 1);
        SpawnNodesOfType(foodResourceNodes, foodQty, usedPositions);

        Debug.Log($"Spawned {usedPositions.Count} resource nodes on the map");
    }

    private void SpawnNodesOfType(GameObject[] nodePrefabs, int qty, List<Vector3> usedPositions) {
        if (nodePrefabs.Length == 0) return;

        for (int i = 0; i < qty; i++) {
            // find a valid position
            Vector3 pos = FindValidNodePosition(usedPositions);
            if (pos != Vector3.zero) {
                GameObject nodePrefab = nodePrefabs[Random.Range(0, nodePrefabs.Length)];
                Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                GameObject node = Instantiate(nodePrefab, pos, rot);
                usedPositions.Add(pos);
                spawnedResourceNodes.Add(node);
            }
        }    
    
       
    }

    private Vector3 FindValidNodePosition(List<Vector3> usedPositions) {
        // Random points
        Vector3 origin = transform.position;

        //try limited number of times to find valid position
        for(int attempts = 0; attempts < 30; attempts++) {
            // Random point in the circle
            Vector2 randomCircle = Random.insideUnitCircle * nodeSpawnRadius;
            Vector3 potentialPos = new Vector3( origin.x + randomCircle.x, origin.y, origin.z + randomCircle.y);

            // Raycast down to find terrain height
            if (Physics.Raycast(potentialPos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f)) {
                potentialPos.y = hit.point.y;

                bool tooClose = false;
                foreach (Vector3 usedPos in usedPositions) {
                    if (Vector3.Distance(potentialPos, usedPos) < minDistBetweenNodes) {
                        tooClose = true;
                        break;
                    }
                }

                bool overlapsInvalid = Physics.CheckSphere(potentialPos, 16f, nodeSpawnAvoidLayers);

                Vector3Int gridPos = gridZoneManager.GetGrid().WorldToCell(potentialPos);
                bool inBuildableZone = gridZoneManager.IsPositionBuildable(gridPos);



                if (!tooClose && !overlapsInvalid && !inBuildableZone) {
                    return potentialPos;
                }
            }
        }
        return Vector3.zero; // Return zero vector if no valid position found after attempts
    }
    #endregion
}
