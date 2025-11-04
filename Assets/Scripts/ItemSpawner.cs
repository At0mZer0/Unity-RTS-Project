using UnityEngine;

// Spawn any power ups / items from here
public class ItemSpawner : MonoBehaviour {
    public static ItemSpawner Instance { get; private set; }

    [SerializeField] private GameObject runePrefab;
    [SerializeField] public float dropChance;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }


    void Start() {
        
    }

    void Update() {
        
    }

    public void SpawnRune(Vector3 position, bool guaranteedDrop = true) {
        if (!guaranteedDrop && Random.value > dropChance) 
            return;

        if (runePrefab == null) {
            Debug.LogError("Rune prefab is not assigned in the ItemSpawner script.");
            return;
        }
        // offset spawn above ground
        Vector3 spawnPos = new Vector3(position.x, position.y + .5f, position.z);
        Instantiate(runePrefab, spawnPos, Quaternion.identity);        
    }



}
