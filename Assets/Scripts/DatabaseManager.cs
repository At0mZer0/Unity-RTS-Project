using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static DatabaseManager Instance {get; set;} // This is a Singleton

    // Makes sure this is the only one 
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
}

    // Objects Database
    public ObjectsDatabseSO databaseSO;

}