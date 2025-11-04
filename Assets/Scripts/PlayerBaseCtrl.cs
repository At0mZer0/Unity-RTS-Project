using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerBaseCtrl : MonoBehaviour {
    public static PlayerBaseCtrl Instance {get; set;} 
    [SerializeField] private int sacredTreeID = 0;
    public Transform sacredTreeTransform;
    public Constructable treeConstructable;
    [SerializeField] private PlacementSystem  placementSystem;  
    [SerializeField] private GridZoneManager  gridZoneManager;
    [SerializeField] private ObjectPlacer  objectPlacer;
    [SerializeField] private SpawnController spawnCtrl;

    [Header("Player Base UI")]
    public TextMeshProUGUI UiWave;
    public int waveNum = 0;
    public TextMeshProUGUI UiLevel;
    public float level = 1;
    public TextMeshProUGUI UiGroveSize;
    public int groveSize;                  // The amount of cells that are considered buildable
    public TextMeshProUGUI UiEnemyKills;
    public int enemyKills = 0;
    public TextMeshProUGUI UiUnitQty;
    public int unitQty = 0;

    // Struct used for triggering UpdateIntensity against PlayerBaseCtrl values
    private struct IntensityValues {
        public int WaveNum;
        public float BaseHealth;
        public int UnitCapacity;
        public int UnitQty;
        public int RunesCollected;
        public int EnemyKills;
        public int ResourceCapacity;
        public int ResourcesGathered;
        public int GroveSize;
        public int BuildCount;
        public int CurrentEnemyCount;
    }

    [Header("Grove Level Properties")]
    public int faeKilled; // tracking for factoring in to spirit level
    public int romansKilled; // Both updated in Enemy.cs
    public int runesCollected;
    public int resourcesGathered;

    public TextMeshProUGUI UiXP;
    public TextMeshProUGUI UiXPGained;
    public float levelXP;
    public float xpToLevelUp; // XP needed to level up

    [Header("Spirit System")]
    public float spiritLevel = 60;
    public float baseSpirit;
    public float accumulatedSpirit;
    public float spiritRate = -3f; // rate of spirit gain per second
    public float spiritMax = 100f;
    public bool pauseSpiritTimer = false;
    public TextMeshProUGUI UiSpirit;

    [Header("Intensity Timer Settings")]
    public TextMeshProUGUI UiIntensity;
    public float intensityLevel;
    public float intensityMax = 100f;
    // will be hidden from player but used to determine Enemy waves (lower score = more events)
    public IntensityFocus intensityFocus = IntensityFocus.Balanced;
    public bool activeIntensityTimer = true;
    public float intensityInterval = 30f; // secons between updates
    public float addIntensity = 2f; // percentage added per interval
    public float effectIntensityMod = -1f;
    public float intensityTimer = 0f;
    public float accumulatedIntensity;
    public float baseIntensity;
    public bool pauseIntensityTimer = false;

    private IntensityValues _previousValues;
    private bool _isFirstCheck = true;

    // Props that are used for power ups and upgrades
    [Header("Base Properties")]
    public float baseHealth; // get from Sacred Tree

    public TextMeshProUGUI UiUnitCap;
    public int unitCapacity;
    public TextMeshProUGUI UiResourceCap;
    public int resourceCapacity;
    public int buildCount;
    public int currentEnemyCount;


    [Header("Player Units / Enemies / Assets Reference Lists")]
    public UnitSelectionManager unitSelectManager;
    public List<GameObject> playerUnits;
    public List<GameObject> enemyRomans;
    public List<GameObject> enemyFae;
    public List<GameObject> playerStructures;
    public List<int> fallenUnits = new List<int>(); /// for bringing back to life during rune power up stores the databaseID for a unit OnDestroy
    public Material spectorMaterial;

    public static bool baseInit = false;

    private delegate ValueType ValueGetter<ValueType>();
    private List<(ValueGetter<object> current, ValueGetter<object> previous)> _valueComparisons;

    [SerializeField] private const float BASE_XP_TO_LVL = 100f;
    [SerializeField] private const float XP_SCALER = 1.5f;
    [SerializeField] private List<Benefits> levelBenefits = new List<Benefits>();

    #region Start / Awake / Updates
    

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        // Start the coroutine instead of calling PlaceSacredTree directly
        StartCoroutine(WaitForDatabaseThenPlaceSacredTree());
        // get reference lists for applying buffs / debuffs / level props
        playerUnits = unitSelectManager.allUnitsList;
        enemyRomans = SpawnController.Instance.spawnedRomans;
        enemyFae = SpawnController.Instance.spawnedFae;
        playerStructures = ObjectPlacer.Instance.placedGameObjects;

        InitValueComparisons();
        UpdateUI();

    }
    
    private IEnumerator WaitForDatabaseThenPlaceSacredTree() {
        // Wait until DatabaseManager exists and has a valid databaseSO
        yield return new WaitUntil(() => 
            DatabaseManager.Instance != null && 
            DatabaseManager.Instance.databaseSO != null);
        
        // Optional: add a small delay to ensure everything is fully loaded
        yield return new WaitForSeconds(0.1f);
        
        // Now safely place the sacred tree
        PlaceSacredTree();
    }

    private void PlaceSacredTree() {
        if (placementSystem == null || gridZoneManager == null || objectPlacer == null) {
            Debug.LogError("One or more components are not assigned in the inspector.");
            return;
        }

        ObjectData treeData = DatabaseManager.Instance.databaseSO.GetObjectByID(sacredTreeID);
        if (treeData == null) {
            Debug.LogError($"Could not find Sacred Tree with ID {sacredTreeID} in database!");
            return;
        }
        Vector2Int treeSize = treeData.Size;

        Grid grid = gridZoneManager.GetGrid();
        Vector3Int basePosition = grid.WorldToCell(transform.position);

        // Calculate offset to center while conforming to 1 x 1 grid (update later to handle more sizes)
        Vector3Int offset = new Vector3Int(Mathf.FloorToInt(treeSize.x / 2), 0, Mathf.FloorToInt(treeSize.y / 2));

        // apply offset to place center of tree at base center
        Vector3Int treePosition = basePosition - offset;
        Vector3 worldPos = grid.CellToWorld(treePosition);
        int index = objectPlacer.PlaceObject(treeData.Prefab, worldPos);

        GameObject placedTree = objectPlacer.placedGameObjects[index];
        sacredTreeTransform = placedTree.transform; // Store the transform of the placed Sacred Tree for access rune gathering

        // Update grid 
        GridData floorData = placementSystem.GetFloorData();
        floorData.AddObjectAt(treePosition, treeData.Size, treeData.ID, index);

        // Add to allExistingBuildings and play sound
        ResourceManager.Instance.UpdateBuildingChanged(treeData.thisBuildingType, true, worldPos);
        treeConstructable = placedTree.GetComponent<Constructable>();

        // Create the Buildable zone around the Sacred Tree
        List<Vector3Int> sacredTreeCells = gridZoneManager.CreateBuildableZoneCircle(transform.position, 30);
        treeConstructable.addedCells = sacredTreeCells;
        
        baseInit = true;
        UpdateGroveSize();
    }
    #endregion

    #region Update UI
    public void UpdateGroveSize() {
        groveSize = gridZoneManager.GetBuildableZoneCount();
        UpdateUI();
    }

    public void UpdateWaveNum(int wave) {
        waveNum += wave;
        UpdateUI();
    }

    public void UpdateUnitQty(int qty) {
        unitQty += qty;
        UpdateUI();
    }

    public void UpdateEnemyKills(int kills) {
        enemyKills += kills;
        UpdateUI();
    }

    public void UpdateUI() {
        UiWave.text = $"{waveNum}";
        UiLevel.text = $"{level}";
        UiGroveSize.text = $"{groveSize}";
        UiEnemyKills.text = $"{enemyKills}";
        UiUnitQty.text = $"{unitQty}";
        UiUnitCap.text = $"{unitCapacity}";
        UiResourceCap.text = $"{resourceCapacity}";
    }
    #endregion


    #region Benefits Apply Status Effect

    public void ApplyBenefits(Benefits benefits, GameObject sourceObj) {
        // AddStatusEffect(benefits.effect);

        foreach (EffectConfig effect in benefits.effects) {
            List<GameObject> targetList = new List<GameObject>();

            if (effect.objectType != ObjectType.Base) {
                targetList = GetEffectTarget(effect.effectTarget);
            }

            ApplyEffect(effect.effectType, 
                        effect.mod, 
                        effect.duration, 
                        effect.objectType, 
                        sourceObj,
                        effect.isItem, 
                        targetList);            
        }
    }

    public void ApplyEffect(EffectType type, float mod, float duration, ObjectType objectType, GameObject sourceObj = null, bool isItem = false, List<GameObject> targetList = null) {
        Effect newEffect;
        PlayerBaseCtrl baseTarget = PlayerBaseCtrl.Instance;
        StatusEffect baseStatus = baseTarget.GetComponent<StatusEffect>();

        switch (type) {
            #region Unit / Building Effects
            case EffectType.AttackMod:
                newEffect = new AttackMod(mod, duration, objectType, sourceObj, isItem);
                foreach (GameObject targetObj in targetList) {
                    StatusEffect targetStatus = targetObj.GetComponent<StatusEffect>();
                    targetStatus.AddStatusEffect(newEffect);
                }
                break;
            case EffectType.AttackRate:
                newEffect = new AttackRate(mod, duration, objectType, sourceObj, isItem);
                foreach (GameObject targetObj in targetList) {
                    StatusEffect targetStatus = targetObj.GetComponent<StatusEffect>();
                    targetStatus.AddStatusEffect(newEffect);
                }
                break;
            case EffectType.DefenseMod:
                newEffect = new DefenseMod(mod, duration, objectType, sourceObj, isItem);
                foreach (GameObject targetObj in targetList) {
                    StatusEffect targetStatus = targetObj.GetComponent<StatusEffect>();
                    targetStatus.AddStatusEffect(newEffect);
                }
                break;
            case EffectType.SpeedMod:
                newEffect = new SpeedMod(mod, duration, objectType, sourceObj, isItem);
                foreach (GameObject targetObj in targetList) {
                    StatusEffect targetStatus = targetObj.GetComponent<StatusEffect>();
                    targetStatus.AddStatusEffect(newEffect);
                }
                break;
            case EffectType.MaxHealth:
                newEffect = new MaxHealth(mod, duration, objectType, sourceObj, isItem);
                foreach (GameObject targetObj in targetList) {
                    StatusEffect targetStatus = targetObj.GetComponent<StatusEffect>();
                    targetStatus.AddStatusEffect(newEffect);
                }
                break;
            case EffectType.HealthRegen:
                newEffect = new HealthRegen(mod, duration, objectType, sourceObj, isItem);
                foreach (GameObject targetObj in targetList) {
                    StatusEffect targetStatus = targetObj.GetComponent<StatusEffect>();
                    targetStatus.AddStatusEffect(newEffect);
                }
                break;
            case EffectType.DamageBonus:
                newEffect = new DamageBonus(mod, duration, objectType, sourceObj, isItem);
                foreach (GameObject targetObj in targetList) {
                    StatusEffect targetStatus = targetObj.GetComponent<StatusEffect>();
                    targetStatus.AddStatusEffect(newEffect);
                }   
                break;
            case EffectType.MaxLoad:
                newEffect = new MaxLoad(mod, duration, objectType, sourceObj, isItem);
                foreach (GameObject targetObj in targetList) {
                    StatusEffect targetStatus = targetObj.GetComponent<StatusEffect>();
                    targetStatus.AddStatusEffect(newEffect);
                }
                break;
            case EffectType.ResourceRate:
                newEffect = new ResourceRate(mod, duration, objectType, sourceObj, isItem);
                foreach (GameObject targetObj in targetList) {
                    StatusEffect targetStatus = targetObj.GetComponent<StatusEffect>();
                    targetStatus.AddStatusEffect(newEffect);
                }
                break;
            case EffectType.ResourceQty:
                newEffect = new ResourceQty(mod, duration, objectType, sourceObj, isItem);
                foreach (GameObject targetObj in targetList) {
                    StatusEffect targetStatus = targetObj.GetComponent<StatusEffect>();
                    targetStatus.AddStatusEffect(newEffect);
                }
                break;
            case EffectType.ProjectileDamage:
                newEffect = new ProjectileDamage(mod, duration, objectType, sourceObj, isItem);
                foreach (GameObject targetObj in targetList) {
                    StatusEffect targetStatus = targetObj.GetComponent<StatusEffect>();
                    targetStatus.AddStatusEffect(newEffect);
                }
                break;
            #endregion
            // Bass Effects are applied to PlayerBaseCtrl
            #region Base Effects
            case EffectType.ResourceCap:
                newEffect = new ResourceCap(mod, duration, objectType, sourceObj, isItem);
                baseStatus.AddStatusEffect(newEffect);
                break;
            case EffectType.UnitCap:
                newEffect = new UnitCap(mod, duration, objectType, sourceObj, isItem);
                baseStatus.AddStatusEffect(newEffect);
                break; 

            case EffectType.RaiseFallenUnits:
                newEffect = new RaiseFallenUnits(mod, duration, objectType, sourceObj, isItem);
                baseStatus.AddStatusEffect(newEffect);
                break;
            case EffectType.BountifulResources:
                newEffect = new BountifulResources(mod, duration, objectType, sourceObj, isItem);
                baseStatus.AddStatusEffect(newEffect);
                break;
            #endregion
        }
        ModifyIntensity(effectIntensityMod);

    }

    public List<GameObject> GetEffectTarget(EffectTarget target) {
        List<GameObject> targetList = new List<GameObject>();;

        switch(target) {

            case EffectTarget.AllPlayerUnits:
                targetList = playerUnits;
                break;
            case EffectTarget.AllEnemyUnits:
                targetList.AddRange(enemyRomans);
                targetList.AddRange(enemyFae);
                break;
            case EffectTarget.EnemyRomans:
                targetList = enemyRomans;
                break;
            case EffectTarget.EnemyFae:
                targetList = enemyFae;
                break;
            case EffectTarget.Buildings:
                targetList = playerStructures;
                break;
        }
        return targetList;
    }

    #endregion

    #region Leveling System


    
    public void GainXP(float xp) {
        levelXP += xp;
        UpdateXpUI();
        while (levelXP >= xpToLevelUp) {
            float overflow = levelXP - xpToLevelUp;
            levelXP = 0;
            LevelUp();
            levelXP = overflow;
        }
        UpdateXpUI();
    }

    private void UpdateXpRequired() {
        xpToLevelUp = BASE_XP_TO_LVL * Mathf.Pow(XP_SCALER, level - 1);
        #region xp scaling example
        /* Example          xp requred TotalXp
            1	100 × (1.5^0)	100	    100
            2	100 × (1.5^1)	150	    250
            3	100 × (1.5^2)	225	    475
            4	100 × (1.5^3)	337.5	812.5
            5	100 × (1.5^4)	506.25	1318.75
        */
        #endregion
    }

    private void LevelUp() {
        level++;
        UpdateXpRequired();
        
        foreach (Benefits benefit in levelBenefits) {
            ApplyBenefits(benefit, playerStructures[0]);
        }
        // Add Benefits in PlayerBaseManager in Unity inspector
        // eventually make more benefits that index to sets for levels to change benefits
    }

    public void UpdateXpUI() {
        UiXP.text = $"{xpToLevelUp:F1}";
        UiXPGained.text = $"{levelXP:F1}";
    }
    #endregion


    #region Spirit System
    public void ProcessSpiritTimer(float deltaTime, float spiritMod) {
        if(pauseSpiritTimer)
            return;
        // Calculate spirit change (can be positive or negative)
        float spiritChange = spiritMod * deltaTime;
        accumulatedSpirit += spiritChange;
        UpdateSpiritLevel();
    }
  
    public void UpdateSpiritLevel() {
        spiritLevel = baseSpirit + accumulatedSpirit;
        spiritLevel = Mathf.Clamp(spiritLevel, 0f, spiritMax);
        UpdateSpiritUI();
    }

    public void AdjustSpiritLevel(float mod) {
        accumulatedSpirit += mod;
        UpdateSpiritLevel();
    }

    public void ModifySpiritRate(float multiplier) {
        spiritRate *= multiplier;
    }

    public void UpdateSpiritUI() {
        float spiritPercentage = (spiritLevel / spiritMax) * 100f; // Calculate percentage
        UiSpirit.text = $"{spiritPercentage:F1}%";
    }

    // // Calculate and factor in properties that effect level of Otherworld threats (Fae)
    // private float CalculateSpiritFavor() {
    //     // You can implement your own formula here based on:
    //     // - Druids paying respects
    //     // - Ritual sites created/destroyed
    //     // - Fae creatures killed
    //     // - Sacred sites protected
    //     // - Nature vs destruction balance

    //     float calculatedSpirit = 50f; // default midpoint

    //     //example factors
    //     float ritualSites= 0f;
    //     float faeKilled = 0f;
    //     float groveSize;
    // }

    public void SetSpiritMax(float newMax) {
        spiritMax = newMax;
        UpdateSpiritLevel();
    }


    #endregion

    #region Intensity System
    public enum IntensityFocus {
        Balanced, // default
        Aggressive, // combat stat focused
        Defensive, // defense focused
        Resource, // resource gathering focused
        Building, // building / base expansion focused
        Chaotic  // random weighted event focuses
    }

    public void ProcessIntensityTimer(float deltaTime) {
        if(!activeIntensityTimer || pauseIntensityTimer) 
            return;
        intensityTimer += deltaTime;
        if (intensityTimer >= intensityInterval) {
            // reset timer
            intensityTimer = 0f;

            ModifyIntensity();
            DetermineEventSpawns();
        }
    } 

    void Update() {
        UpdateCounts(); // updates current enemy count and build count
        ProcessIntensityTimer(Time.deltaTime);
        ProcessSpiritTimer(Time.deltaTime, spiritRate);
        if (CheckIntensityValueChange()) {
            UpdateIntensity(intensityFocus);
            DetermineEventSpawns();
        }
    }
    
    public void SetIntensityMax(float newMax) {
        intensityMax = newMax;
        UpdateIntensity();
    }

    public void ModifyIntensity(float customIncrement = -1f) {
        float incrementQty = customIncrement >= 0f ? customIncrement : addIntensity;

        // adds the intensity over time value if none is passed in
        accumulatedIntensity += incrementQty;
        accumulatedIntensity = Mathf.Clamp(accumulatedIntensity, 0f, intensityMax);

        UpdateIntensity();
    }

    // Pause Timer
    public void PauseIntensityTimer(bool paused) {
        pauseIntensityTimer = paused;
    
    }

    // Reset Timer
    public void ResetIntensityTimer() {
        intensityTimer = 0f;
    }

    public void UpdateIntensity(IntensityFocus focus = IntensityFocus.Balanced) {

        baseIntensity = GetIntensityPercent(focus);
        // Combine intensity over time with stat based intensity rating
        intensityLevel = baseIntensity + accumulatedIntensity;
        intensityLevel = Mathf.Clamp(intensityLevel, 0f, intensityMax);

        if (UiIntensity != null) {
            float intensityPercent = (intensityLevel / intensityMax) * 100f;
            UiIntensity.text = $"{intensityPercent:F1}%";
        }
    }

    private float GetIntensityPercent(IntensityFocus focus = IntensityFocus.Balanced) {
        // Get base intensity value (0-100)
        float intensity = GetBaseIntensity(focus);

        // Add random factor based on focus
        intensity = ApplyRandomFactor(intensity, focus);

        // return %
        return Mathf.Clamp(intensity, 0f, 100f);
    }

    private float GetBaseIntensity(IntensityFocus focus) {
        // weight modifiers
        float waveWeight = 1f;
        float healthWeight = 1f;
        float unitWeight = 1f;
        float runesWeight = 1f;
        float resourceWeight = 1f;
        float buildingWeight = 1f;
        float enemyWeight = 1f;
        float killWeight = 0f;


        switch (focus) {
            case IntensityFocus.Aggressive:
                unitWeight = 1.5f;
                enemyWeight = 1.8f;
                killWeight = 1.5f;
                waveWeight = 1.3f;
                healthWeight = 0.6f;
                resourceWeight = 0.7f;
                break;
            case IntensityFocus.Defensive:
                healthWeight = 1.8f;
                buildingWeight = 1.5f;
                unitWeight = 0.8f;
                enemyWeight = 0.7f;
                break;
            case IntensityFocus.Resource:
                resourceWeight = 1.8f;
                unitWeight = 0.7f;
                enemyWeight = 0.8f;
                break;
            case IntensityFocus.Building:
                buildingWeight = 2.0f;
                runesWeight = 1.3f;
                waveWeight = 0.8f;
                enemyWeight = 0.7f;
                break;
            case IntensityFocus.Chaotic:
                // Chaotic uses default weights but will have randomness added later
                break;
            // Balanced keeps default weights
        }
        // Wave factor (MORE waves now DECREASES intensity)
        float maxWaveFactor = 20f * waveWeight;
        float waveFactor = maxWaveFactor - Mathf.Min(waveNum * 2f, 20f) * waveWeight;

        // Health factor (MORE health now DECREASES intensity)
        float maxHealthFactor = 10f * healthWeight;
        float healthFactor = maxHealthFactor - (baseHealth / 100f) * 10f * healthWeight;

        // Unit factor (MORE units now DECREASES intensity)
        float capacityRatio = unitCapacity > 0 ? (float)unitQty / unitCapacity : 0f;
        float maxUnitFactor = 50f * unitWeight;
        float unitFactor = maxUnitFactor - (capacityRatio * 50f) * unitWeight;

        // Enemy factor (MORE enemies now INCREASES intensity)
        float enemyRatio = unitCapacity > 0 ? (float)currentEnemyCount / unitCapacity : 0f;
        float enemyFactor = Mathf.Min(enemyRatio * 60f, 10f) * enemyWeight;

        // Resource factor (MORE resources now DECREASES intensity)
        float resourceRatio = resourceCapacity > 0 ? (float)resourcesGathered / resourceCapacity : 0f;
        float maxResourceFactor = 30f * resourceWeight;
        float resourceFactor = maxResourceFactor - (resourceRatio * 30f) * resourceWeight;

        // Building factor (MORE buildings now DECREASES intensity)
        float maxBuildFactor = (30f * 3f) * buildingWeight;                                 //  max ~30 buildings
        float buildFactor = maxBuildFactor - (buildCount * 3f) * buildingWeight;

        // Grove size factor (LARGER grove now DECREASES intensity)
        float maxGroveFactor = 10f * buildingWeight;
        float groveFactor = maxGroveFactor - Mathf.Min(groveSize / 50f, 10f) * buildingWeight;

        // Runes factor (MORE runes now DECREASES intensity)
        float maxRunesFactor = (25f * 4f) * runesWeight;                                    //  max ~25 runes
        float runeFactor = maxRunesFactor - (runesCollected * 4f) * runesWeight;

        // Kills factor (MORE kills now DECREASES intensity)
        float maxKillFactor = (50f * 2f) * killWeight;                                      //  max ~50 kills
        float killFactor = maxKillFactor - (enemyKills * 2f) * killWeight;

        // Calculate base intensity with reversed factors
        float calculatedIntensity = (
            waveFactor + healthFactor + unitFactor + enemyFactor + 
            resourceFactor + buildFactor + groveFactor + runeFactor + killFactor
        );

        baseIntensity = calculatedIntensity;

        // Normalize to 0 / 100 range
        return Mathf.Clamp(baseIntensity / 15f, 0f, 100f);
    }

    private float ApplyRandomFactor(float intensity, IntensityFocus focus) {
        float randomMagnitude = 5f; // default variabce

        if (focus == IntensityFocus.Chaotic) {
            randomMagnitude = 20f; // extreme
            intensity *= 1.5f * (UnityEngine.Random.value + 0.5f); // Multiply by 0.75 - 1.5
        }

        // Apply random adjustment
        float randomFactor = UnityEngine.Random.Range(-randomMagnitude, randomMagnitude);

        // add penalty for higher wave levels
        float waveProgress = Mathf.Min(waveNum * 2f, 20f);
        float wavePenalty = UnityEngine.Random.Range(0f, waveProgress);

        // Apply combined factor (random variance + potential penalty)
        return intensity + randomFactor - wavePenalty;
    }

    public void DetermineEventSpawns() {
        ThreatLevel threatLevel = DetermineThreatLevel(intensityLevel);
        switch (threatLevel) {
            case ThreatLevel.Critical:
                // Spawn high level enemies
                break;
            case ThreatLevel.High:
                // Spawn medium level enemies
                break;
            case ThreatLevel.Medium:
                // Spawn low level enemies
                break;
            case ThreatLevel.Low:
                // Spawn low level enemies
                break;
            case ThreatLevel.Peace:
                // No spawns
                break;
        }
        
    }

    public enum ThreatLevel {
        Critical,
        High,
        Medium,
        Low,
        Peace
    }

    public ThreatLevel DetermineThreatLevel(float intensity) {
        if (intensity >= 90f) return ThreatLevel.Critical;  // 90-100% = Critical (most dangerous)
        if (intensity >= 70f) return ThreatLevel.High;      // 70-90% = High
        if (intensity >= 50f) return ThreatLevel.Medium;    // 50-70% = Medium
        if (intensity >= 30f) return ThreatLevel.Low;       // 30-50% = Low
        if (intensity >= 10f) return ThreatLevel.Peace;     // 10-30% = Peace
        return ThreatLevel.Peace;                          // 0-10% = Peace (safest) 
    }

    private void UpdateCounts() {
        if (buildCount > playerStructures.Count || buildCount < playerStructures.Count) {
            buildCount = playerStructures.Count;
        }

        if (currentEnemyCount > enemyRomans.Count + enemyFae.Count || currentEnemyCount < enemyRomans.Count + enemyFae.Count) {
            currentEnemyCount = enemyRomans.Count + enemyFae.Count;
        }
    }

    // loop through Touple list and compare values 
    public bool CheckIntensityValueChange() {
        // always return true on first check
        if (_isFirstCheck) {
            UpdatePreviousValues();
            _isFirstCheck = false;
            return true;
        } 
        foreach (var (current, previous) in _valueComparisons) {    
            if (!Equals(current(), previous())) {
                // values have changed store new values
                UpdatePreviousValues();
                return true;
            }
        }
        return false;
    }

    // Helper method to store previous values in struct
    private void UpdatePreviousValues() {
        _previousValues.WaveNum = waveNum;
        _previousValues.BaseHealth = baseHealth;
        _previousValues.UnitCapacity = unitCapacity;
        _previousValues.UnitQty = unitQty;
        _previousValues.RunesCollected = runesCollected;
        _previousValues.EnemyKills = enemyKills;
        _previousValues.ResourceCapacity = resourceCapacity;
        _previousValues.ResourcesGathered = resourcesGathered;
        _previousValues.GroveSize = groveSize;
        _previousValues.BuildCount = buildCount;
        _previousValues.CurrentEnemyCount = currentEnemyCount;
    }

    // initialize list of tuples with my generic value getters
    private void InitValueComparisons() {
        _valueComparisons = new List<(ValueGetter<object>, ValueGetter<object>)> {
            (() => waveNum, () => _previousValues.WaveNum),
            (() => baseHealth, () => _previousValues.BaseHealth),
            (() => unitCapacity, () => _previousValues.UnitCapacity),
            (() => unitQty, () => _previousValues.UnitQty),
            (() => runesCollected, () => _previousValues.RunesCollected),
            (() => enemyKills, () => _previousValues.EnemyKills),
            (() => resourceCapacity, () => _previousValues.ResourceCapacity),
            (() => resourcesGathered, () => _previousValues.ResourcesGathered),
            (() => groveSize, () => _previousValues.GroveSize),
            (() => buildCount, () => _previousValues.BuildCount),
            (() => currentEnemyCount, () => _previousValues.CurrentEnemyCount)
        };
    }

    #endregion

}