using UnityEngine;

public class Rune : MonoBehaviour {
    
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float floatSpeed = 1.0f;
    [SerializeField] private Vector3 startPos;
    public Rigidbody rb;
    [SerializeField] public GameObject unitHoldingRune;

    [SerializeField] private RuneData runeData;

    void Start() {
        startPos = transform.position;

        rb = GetComponent<Rigidbody>();
        SelectRandomRuneType();

        // add small random force for natural drop effect
        rb.AddForce(new Vector3(Random.Range(-1f, 1f), 2f, Random.Range(-1f, 1f)), ForceMode.Impulse);
    }

    private void SelectRandomRuneType() {
        var runeTypes = RuneManager.Instance.runeTypes;

        if (runeTypes.Count > 0) {
            runeData = runeTypes [Random.Range(0, runeTypes.Count)];
        }
    }

    void Update() {
        // make rune float off ground 
        if (rb.linearVelocity.magnitude < 0.1f) {
            rb.isKinematic = true;

            // float up and down slightly
            float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnDestroy() {
        if (runeData != null) {
            if (unitHoldingRune != null) {
                RuneManager.Instance.ApplyRunePowerUp(runeData, gameObject);
            }
        }
    }

    public void ShowRuneUI() {
        RuneManager.Instance.ShowRuneUI(runeData);
    }
}
