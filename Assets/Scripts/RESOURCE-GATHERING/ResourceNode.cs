using UnityEngine;
using UnityEngine.AI;

public class ResourceNode : MonoBehaviour {

    public ResourceManager.ResourceType resourceType;
    [SerializeField] public int resourceQty;
    NavMeshObstacle obstacle;
    private int minQty;
    private int maxQty;
    public float xpGained = 15f;


    private void Start() {
        minQty = SpawnController.Instance.minResourceQty;
        maxQty = SpawnController.Instance.maxResourceQty;
        resourceQty = Random.Range(minQty, maxQty);
    }

    private void Update() {
        if (resourceQty <= 0) {
            ItemSpawner.Instance.SpawnRune(transform.position, true);
            PlayerBaseCtrl.Instance.GainXP(xpGained);
            Destroy(gameObject);
        }
    }

    // will use if I start to notice issues with Units moving around Buildings in PreviewState
    private void ActivateObstacle() {
        obstacle = GetComponent<NavMeshObstacle>();
        obstacle.enabled = true;
    }
}
