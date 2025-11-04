using System.Collections.Generic;
using UnityEngine;

// Created so that it's viewable in Unity Inspector for Debugging
[System.Serializable]
public class EffectDebug {
    public string type;
    public float timeLeft;
    public float mod;
}


public class StatusEffect : MonoBehaviour {
    
    public List<Effect> statusEffects = new List<Effect>();
    
    [SerializeField] private List<EffectDebug> debugEffects = new List<EffectDebug>(); // So I can see effects in Unity's Inspector 

    void Update() {
        if (statusEffects.Count == 0) return;
        CycleStatusEffects();
    }

    // Call and pass in an class that's child of the Effect class
    public void AddStatusEffect(Effect effect) {
        statusEffects.Add(effect);
        effect.Apply(gameObject);
        UpdateDebugList();

        Debug.Log($"Effect {effect.GetEffectType()} applied to {gameObject.name} for {effect.duration}s");
    }

    private void CycleStatusEffects() {
        List<Effect> expiredEffects = new List<Effect>();

        // Update all effects and if Update returns false, instance is added to expiredEffects list for removal
        foreach (Effect effect in statusEffects) {
            // will run all update code before evalutating true/false so HealthRegen will work
            if (!effect.Update(Time.deltaTime)) {
                expiredEffects.Add(effect); 
            }
        }
        // Remove expired effects after looping through the list
        foreach (Effect expiredEffect in expiredEffects) {
            // don't confuse List remove with Effect remove, this is reverting changes made from apply
            expiredEffect.Remove(gameObject);
        
            // remove the effect from the list
            statusEffects.Remove(expiredEffect);

            Debug.Log($"Effect {expiredEffect.GetEffectType()} expired on {gameObject.name}");
        }

        UpdateDebugList();
    }

    // Check to see if an effect is active passing in the Enum (for if I don't want an effect to stack)
    public bool HasEffect(EffectType effectType) {
        return statusEffects.Exists(effect => effect.GetEffectType() == effectType);
    }

    private void UpdateDebugList() {
        debugEffects.Clear();
        
        foreach (Effect effect in statusEffects) {
            debugEffects.Add(new EffectDebug {
                type = effect.GetEffectType().ToString(),
                timeLeft = effect.timeLeft,
                mod = effect.mod
            });
        }
    }

    
}

