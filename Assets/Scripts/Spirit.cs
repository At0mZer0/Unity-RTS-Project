using UnityEngine;

public class Spirit : MonoBehaviour {
    
    public float spiritGained = 1f;
    public float spiritRateMod = -0.5f;

    void Start() {
      ModifySpiritRateAndLevel();

    }


    void Update() {
        
    }

    public void ModifySpiritRateAndLevel() {
        var baseCtrl = PlayerBaseCtrl.Instance;
        baseCtrl.AdjustSpiritLevel(spiritGained);
        baseCtrl.ModifySpiritRate(spiritRateMod);
    }
}
