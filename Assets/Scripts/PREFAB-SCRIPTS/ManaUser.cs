using UnityEngine;

public class ManaUser : MonoBehaviour {

    public int producingPower;
    public int consumingPower;

    public bool isProducer;

    public void PowerOn() {
        if (isProducer) {
            PowerManager.Instance.AddPower(producingPower);
        } else {
            PowerManager.Instance.ConsumePower(consumingPower);
        }
    }

    public void OnDestroy() {
        if (GetComponent<Constructable>().inPreviewMode == false) {
            if (isProducer) {
                PowerManager.Instance.RemovePower(producingPower);
                PowerManager.Instance.PlayPowerInsufficientSound();
            } else {
                PowerManager.Instance.ReleasePower(consumingPower);
                PowerManager.Instance.PlayPowerInsufficientSound();
            }
        }
    }


}
