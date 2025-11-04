using UnityEngine;

public class DamageDealer : MonoBehaviour {
    public float damageDelt;

    void Start() {
        
    }

    void Update() {
        
    }

    private void OnTriggerEnter(Collider other) {
        Unit unit = other.GetComponent<Unit>();
        if (unit != null) {
            unit.TakeDamage(damageDelt);
        } 
        
    }

}
