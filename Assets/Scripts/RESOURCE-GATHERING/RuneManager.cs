using UnityEngine;
using TMPro;
using System.Collections.Generic;
using NUnit.Framework.Internal;

[System.Serializable]
public class RuneData {
    public string name;
    public string bonusDescription;
    public List<EffectConfig> effects = new List<EffectConfig>();
}

public class RuneManager : MonoBehaviour {
    public static RuneManager Instance {get; private set;}

    [Header("UI References")]
    public GameObject UiRune;
    public TextMeshProUGUI runeType;
    public TextMeshProUGUI powerUpDescription;
    public TextMeshProUGUI runeText;
    public InputManager inputManager;

    [Header("Rune Data")]
    public List<RuneData> runeTypes = new List<RuneData>();

    [Header("Rune Text")]
    public List<string> runeTextList = new List<string>();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        if (UiRune != null) {
            UiRune.SetActive(false);
        } 
    }

    public void ApplyRunePowerUp(RuneData runeFx, GameObject sourceObj) {
        PlayerBaseCtrl plyrBase = PlayerBaseCtrl.Instance;
        foreach (EffectConfig effect in runeFx.effects) {
            List<GameObject> targetList = new List<GameObject>();

            if (effect.objectType != ObjectType.Base) {
                targetList = plyrBase.GetEffectTarget(effect.effectTarget);
            }

            plyrBase.ApplyEffect(effect.effectType, 
                        effect.mod, 
                        effect.duration, 
                        effect.objectType, 
                        sourceObj,
                        effect.isItem, 
                        targetList);            
        }

    } 
    
    // Overload that takes RuneData directly
    public void ShowRuneUI(RuneData selectedRune) {
        inputManager.OnExit += CloseRuneUI;

        runeType.text = selectedRune.name;
        powerUpDescription.text = selectedRune.bonusDescription;
        runeText.text = runeTextList[Random.Range(0, runeTextList.Count)];

        UiRune.SetActive(true);             // show the UI  

    }


    public void ShowRuneUI(string runeName = "") {
    RuneData selectedRune;
    
    // Find the rune by name or select random
    if (string.IsNullOrEmpty(runeName)) {
        selectedRune = runeTypes[Random.Range(0, runeTypes.Count)];
    } else {
        selectedRune = runeTypes.Find(r => r.name == runeName);
        if (selectedRune == null) {
            selectedRune = runeTypes[0];
        }
    }
    
    ShowRuneUI(selectedRune);
}

    

    public void CloseRuneUI() {
        UiRune.SetActive(false); // hide the UI
        inputManager.OnExit -= CloseRuneUI;
    }


   




}
