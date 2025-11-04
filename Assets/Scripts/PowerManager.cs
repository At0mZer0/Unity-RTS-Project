using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PowerManager : MonoBehaviour {
    public static PowerManager Instance {get; set;} // This is a Singleton
    
    public int totalPower; // power produced
    public int powerUsage; // power consumed

    [SerializeField] private Image sliderFill;
    [SerializeField] private Slider powerSlider;
    [SerializeField] private TextMeshProUGUI powerText;

    public AudioClip powerAddedClip;
    public AudioClip powerInsufficientClip;

    private AudioSource powerAudioSource;


    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        powerAudioSource = gameObject.AddComponent<AudioSource>();
    }

    private void UpdatePowerUI() {
        int availablePower = CalculateAvailablePower();
        if (availablePower > 0) {
            sliderFill.gameObject.SetActive(true);
        } else {
            sliderFill.gameObject.SetActive(false);
        }

        if (powerSlider != null) {
            powerSlider.maxValue = totalPower;
            powerSlider.value = totalPower - powerUsage; // the total available power
        }
        if (powerText != null) {
            powerText.text = $"{totalPower - powerUsage}/{totalPower}";
        }
    }

    public void ConsumePower(int amount) {
        powerUsage += amount;
        UpdatePowerUI();

        if (IsInsufficientPower()) {
            PlayPowerInsufficientSound();
        }

    }

    public void AddPower(int amount) {
        PlayPowerAddedSound();
        totalPower += amount;
        UpdatePowerUI();
    }

    public void RemovePower(int amount) {
        totalPower -= amount;
        UpdatePowerUI();
        if (IsInsufficientPower()) {
            PlayPowerInsufficientSound();
        }

    }

    public void ReleasePower(int amount) {
        powerUsage -= amount;
        UpdatePowerUI();

    }

    private bool IsInsufficientPower() {
        return CalculateAvailablePower() < 0;
    }

    public int CalculateAvailablePower() {
        return totalPower - powerUsage;
    }

    public void PlayPowerAddedSound() {
        powerAudioSource.PlayOneShot(powerAddedClip);
    }

    public void PlayPowerInsufficientSound() {
        powerAudioSource.PlayOneShot(powerInsufficientClip);
    }

}
