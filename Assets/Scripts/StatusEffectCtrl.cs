using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public enum EffectType {
    AttackMod,
    AttackRate,
    DefenseMod,
    SpeedMod,
    MaxHealth,
    HealthRegen,
    DamageBonus,
    MaxLoad,
    ResourceRate,
    ResourceQty,
    ProjectileDamage,
    ResourceCap,
    UnitCap,
    BountifulResources,
    
    RaiseFallenUnits,
    SpiritBoost, // Boost in Spirit gained overtime or decreased
    FaeFavor, // Lowers percentage needed for Fae to be friendly or enemies
    SpiritPotential, // changes the scale of the spirit level meter by decreasing the spiritMax 
    FreezeSpirit // stops the timers on spirit level
}

public enum ObjectType {
    Unit,
    Building,
    Base,
    Enemy
}


/// base class for all effects, new instances will be added to the Unit Building or Base StatusEffect Lists and ran in the Update()
#region Effect (Parent Class)
public abstract class Effect {
    public float mod;
    public float duration;
    public float timeLeft;
    public EffectType effectType;
    public ObjectType objectType;
    public GameObject sourceObj; // tracks the object that applied the effect
    public bool isItem;          // consumable items give effects when they are destroyed, make True to ensure the effect is not removed

    public Effect(float mod, float duration, ObjectType objectType = ObjectType.Unit, GameObject sourceObj = null, bool isItem = false) {
        this.mod = mod;
        this.duration = duration;
        this.timeLeft = duration;
        this.objectType = objectType;
        this.sourceObj = sourceObj;  
        this.isItem = isItem;
    }

    public abstract void Apply(GameObject target);
    public abstract void Remove(GameObject target);
    public abstract EffectType GetEffectType();
    
    public virtual bool Update(float deltaTime) {
        if (isItem) {
            if (duration <= 0) return true; // Permenent effect
            timeLeft -= deltaTime;
            return timeLeft > 0;
        }

        // Check only if the sourceObj is an item that will be destroyed
        if ((sourceObj == null || !sourceObj) && !isItem) {
            return false;
        }
        
        bool sourceDestroyed = false;

        // Check for a constructable
        Constructable construct = sourceObj.GetComponent<Constructable>();
        if (construct != null) {
            if (construct.constHealth <= 0) {
                sourceDestroyed = true;
            }
        }

        // Also check for unit / shortcircut
        if (!sourceDestroyed) {
            Unit unit = sourceObj.GetComponent<Unit>();
            if (unit != null && unit.unitHealth <= 0) {
                sourceDestroyed = true;
            }
        }

        if (sourceDestroyed) {
            return false;
        }
    

        if (duration <= 0) return true; // Permenent effect
        timeLeft -= deltaTime;
        return timeLeft > 0;
    }
}
#endregion

#region DefenseMod
public class DefenseMod : Effect {
    public DefenseMod(float mod, float duration, ObjectType objectType = ObjectType.Unit, GameObject sourceObj = null, bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.DefenseMod;
    }

    public override void Apply(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                Unit unit = target.GetComponent<Unit>();
                unit.defenseMod += mod;
                break;

            case ObjectType.Building:
                Constructable constructable = target.GetComponent<Constructable>();
                constructable.defenseMod += mod;
                break;
        }
    }
    
    public override void Remove(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                Unit unit = target.GetComponent<Unit>();
                unit.defenseMod -= mod;
                break;

            case ObjectType.Building:
                Constructable constructable = target.GetComponent<Constructable>();
                constructable.defenseMod -= mod;
                break;
        } 
    }

    public override EffectType GetEffectType() => EffectType.DefenseMod; 
}
#endregion

#region Attack Mod
public class AttackMod : Effect {
    public AttackMod(float mod, float duration, ObjectType objectType = ObjectType.Unit, GameObject sourceObj = null, bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.AttackMod;
    }

    public override void Apply(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                AttackController atkCtrl = target.GetComponent<AttackController>();
                atkCtrl.attackMod += mod;
                break;

            case ObjectType.Building:
                Constructable constructable = target.GetComponent<Constructable>();
                constructable.attackMod += mod;
                break;
        }
    }
    
    public override void Remove(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                AttackController atkCtrl = target.GetComponent<AttackController>();
                atkCtrl.attackMod -= mod;
                break;

            case ObjectType.Building:
                Constructable constructable = target.GetComponent<Constructable>();
                constructable.attackMod -= mod;
                break;
        } 
    }

    public override EffectType GetEffectType() => EffectType.AttackMod; 
}

#endregion

#region Attack Rate
public class AttackRate : Effect {
    public AttackRate(float mod, float duration, ObjectType objectType = ObjectType.Unit, GameObject sourceObj = null,  bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.AttackRate;
    }

    public override void Apply(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                AttackController atkCtrl = target.GetComponent<AttackController>();
                atkCtrl.attackRate += mod;
                break;

            case ObjectType.Building:
                Constructable constructable = target.GetComponent<Constructable>();
                constructable.attackRate += mod;
                break;
        }
    }
    
    public override void Remove(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                AttackController atkCtrl = target.GetComponent<AttackController>();
                atkCtrl.attackRate -= mod;
                break;

            case ObjectType.Building:
                Constructable constructable = target.GetComponent<Constructable>();
                constructable.attackRate -= mod;
                break;
        } 
    }

    public override EffectType GetEffectType() => EffectType.AttackRate; 
}

#endregion

#region SpeedMod 
public class SpeedMod : Effect {
    public SpeedMod(float mod, float duration, ObjectType objectType = ObjectType.Unit, GameObject sourceObj = null, bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.SpeedMod;
    }

    public override void Apply(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                NavMeshAgent navMesh = target.GetComponent<NavMeshAgent>();
                navMesh.speed += mod;
                break;
        }
    }
    
    public override void Remove(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                NavMeshAgent navMesh = target.GetComponent<NavMeshAgent>();
                navMesh.speed -= mod;
                break;
        }
    }

    public override EffectType GetEffectType() => EffectType.SpeedMod; 
}
#endregion

#region Max Health 
public class MaxHealth : Effect {
    public MaxHealth(float mod, float duration, ObjectType objectType = ObjectType.Unit, GameObject sourceObj = null,  bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.MaxHealth;
    }

    public override void Apply(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                Unit unit = target.GetComponent<Unit>();
                unit.unitMaxHealth += mod;
                break;

            case ObjectType.Building:
                Constructable constructable = target.GetComponent<Constructable>();
                constructable.constMaxHealth += mod;
                break;
        }
    }

    public override void Remove(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                Unit unit = target.GetComponent<Unit>();
                unit.unitMaxHealth -= mod;
                break;

            case ObjectType.Building:
                Constructable constructable = target.GetComponent<Constructable>();
                constructable.constMaxHealth -= mod;
                break;
        } 
    }

    public override EffectType GetEffectType() => EffectType.MaxHealth; 
}
#endregion

#region Damage Bonus 
public class DamageBonus : Effect {
    public DamageBonus(float mod, float duration, ObjectType objectType = ObjectType.Unit, GameObject sourceObj = null, bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.DamageBonus;
    }

    public override void Apply(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                Unit unit = target.GetComponent<Unit>();
                unit.unitMaxHealth += mod;
                break;

            case ObjectType.Building:
                if (target.TryGetComponent<TowerDefense>(out var towerDefense)) {
                    towerDefense.projectileDmg += mod;
                }
                break;
        }
    }

    public override void Remove(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                Unit unit = target.GetComponent<Unit>();
                unit.unitMaxHealth -= mod;
                break;

            case ObjectType.Building:
                if (target.TryGetComponent<TowerDefense> (out TowerDefense towerDefense)) {
                    towerDefense.projectileDmg -= mod;
                }
                break;
        } 
    }

    public override EffectType GetEffectType() => EffectType.DamageBonus; 
}
#endregion

#region Max Resource Load per Unit 
public class MaxLoad : Effect {
    public MaxLoad(float mod, float duration, ObjectType objectType = ObjectType.Unit, GameObject sourceObj = null,  bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.MaxLoad;
    }

    public override void Apply(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                ResourceGatherer resource = target.GetComponent<ResourceGatherer>();
                resource.maxLoad += (int)mod;
                break;
        }
    }
    
    public override void Remove(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                ResourceGatherer resource = target.GetComponent<ResourceGatherer>();
                resource.maxLoad -= (int)mod;
                break;
        }
    }

    public override EffectType GetEffectType() => EffectType.MaxLoad; 
}
#endregion

#region Resource Gather / Deposit (Rate) 
public class ResourceRate : Effect {
    public ResourceRate(float mod, float duration, ObjectType objectType = ObjectType.Unit, GameObject sourceObj = null, bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.ResourceRate;
    }

    public override void Apply(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                ResourceGatherer resource = target.GetComponent<ResourceGatherer>();
                resource.resourceRate += mod;
                break;
        }
    }
    
    public override void Remove(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                ResourceGatherer resource = target.GetComponent<ResourceGatherer>();
                resource.resourceRate -= mod;
                break;
        }
    }

    public override EffectType GetEffectType() => EffectType.ResourceRate; 
}
#endregion

#region Bountiful Resources 
public class BountifulResources : Effect {
    public BountifulResources(float mod, float duration, ObjectType objectType = ObjectType.Base, GameObject sourceObj = null, bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.BountifulResources;
    }

    public override void Apply(GameObject target) {
        switch (objectType) {
            case ObjectType.Base:
                SpawnController spawnCtrl = SpawnController.Instance;
                spawnCtrl.minResourceQty += (int)mod;
                spawnCtrl.maxResourceQty += (int)mod;
                List<GameObject> nodeList = SpawnController.Instance.spawnedResourceNodes;

                // add a bump in existing nodes resource qty
                foreach (GameObject node in nodeList) {
                    ResourceNode resourceNode = node.GetComponent<ResourceNode>();
                    if (resourceNode != null) {
                        resourceNode.resourceQty += (int)mod;
                    }
                }
                break;
        }
    }
    
    public override void Remove(GameObject target) {
        switch (objectType) {
            case ObjectType.Base:
                SpawnController spawnCtrl = SpawnController.Instance;
                spawnCtrl.minResourceQty -= (int)mod;
                spawnCtrl.maxResourceQty -= (int)mod;
                List<GameObject> nodeList = SpawnController.Instance.spawnedResourceNodes;

                // remove bump in existing nodes resource qty
                foreach (GameObject node in nodeList) {
                    ResourceNode resourceNode = node.GetComponent<ResourceNode>();
                    if (resourceNode != null) {
                        resourceNode.resourceQty -= (int)mod;
                    }
                }
                break;
        }
    }

    public override EffectType GetEffectType() => EffectType.BountifulResources; 
}
#endregion

#region Resource Gather / Deposit (Qty)
public class ResourceQty : Effect {
    public ResourceQty(float mod, float duration, ObjectType objectType = ObjectType.Unit, GameObject sourceObj = null, bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.ResourceQty;
    }

    public override void Apply(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                ResourceGatherer resource = target.GetComponent<ResourceGatherer>();
                resource.resourceQty += (int)mod;
                break;
        }
    }
    
    public override void Remove(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                ResourceGatherer resource = target.GetComponent<ResourceGatherer>();
                resource.resourceQty -= (int)mod;
                break;
        }
    }

    public override EffectType GetEffectType() => EffectType.ResourceQty; 
}
#endregion

#region Projectile Damage 
public class ProjectileDamage : Effect {
    public ProjectileDamage(float mod, float duration, ObjectType objectType = ObjectType.Unit, GameObject sourceObj = null,  bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.ProjectileDamage;
    }

    public override void Apply(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                AttackController atkCtrl = target.GetComponent<AttackController>();
                atkCtrl.projectileDmg += mod;
                break;

            case ObjectType.Building:
                if (target.TryGetComponent<TowerDefense>(out var towerDefense)) {
                    towerDefense.projectileDmg += mod;
                }
                break;
        }
    }

    public override void Remove(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                Unit unit = target.GetComponent<Unit>();
                unit.unitMaxHealth -= mod;
                break;

            case ObjectType.Building:
                if (target.TryGetComponent<TowerDefense> (out var towerDefense)) {
                    towerDefense.projectileDmg -= mod;
                }
                break;
        } 
    }

    public override EffectType GetEffectType() => EffectType.ProjectileDamage; 
}
#endregion

#region Resource Capacity 
public class ResourceCap : Effect {
    public ResourceCap(float mod, float duration, ObjectType objectType = ObjectType.Base, GameObject sourceObj = null,  bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.ResourceCap;
    }

    public override void Apply(GameObject target) {
        switch (objectType) {
            case ObjectType.Base:
                PlayerBaseCtrl.Instance.resourceCapacity += (int)mod;
                PlayerBaseCtrl.Instance.UpdateUI();
                break;
        }
    }
    public override void Remove(GameObject target) {
        switch (objectType) {
            case ObjectType.Base:
                PlayerBaseCtrl.Instance.resourceCapacity -= (int)mod;
                PlayerBaseCtrl.Instance.UpdateUI();
                break;
        }
    }
    public override EffectType GetEffectType() => EffectType.ResourceCap; 
}
#endregion

#region Unit Capacity 
public class UnitCap : Effect {
    public UnitCap(float mod, float duration, ObjectType objectType = ObjectType.Base, GameObject sourceObj = null,  bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.UnitCap;
    }

    public override void Apply(GameObject target) {
        PlayerBaseCtrl.Instance.unitCapacity += (int)mod;
        PlayerBaseCtrl.Instance.UpdateUI();
                
    }
    public override void Remove(GameObject target) {
        PlayerBaseCtrl.Instance.unitCapacity -= (int)mod;
        PlayerBaseCtrl.Instance.UpdateUI();    
    }
    public override EffectType GetEffectType() => EffectType.UnitCap; 
}
#endregion

#region Health Regen
public class HealthRegen : Effect {
    private GameObject storedTarget;
    public HealthRegen(float mod, float duration, ObjectType objectType = ObjectType.Unit, GameObject sourceObj = null, bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.HealthRegen;
    }

    public override void Apply(GameObject target) {
        storedTarget = target;
        switch (objectType) {
            case ObjectType.Unit:
                Unit unit = target.GetComponent<Unit>();
                unit.unitHealth += mod;
                break;

            case ObjectType.Building:
                Constructable constructable = target.GetComponent<Constructable>();
                constructable.constHealth += mod;
                break;

            case ObjectType.Base:
                PlayerBaseCtrl.Instance.baseHealth += mod;
                break;
        }
    }
    
    public override void Remove(GameObject target) {
        switch (objectType) {
            case ObjectType.Unit:
                Unit unit = target.GetComponent<Unit>();
                unit.unitHealth -= mod;
                break;

            case ObjectType.Building:
                Constructable constructable = target.GetComponent<Constructable>();
                constructable.constHealth -= mod;
                break;

            case ObjectType.Base:
                PlayerBaseCtrl.Instance.baseHealth -= mod;
                break;
        } 
    }

    public override EffectType GetEffectType() => EffectType.HealthRegen; 

    public override bool Update(float deltaTime) {
        if (duration <= 0) return true; // Permenent effect
        float healPerFrame = mod * deltaTime;

        switch (objectType) {
            case ObjectType.Unit:
                Unit unit = storedTarget.GetComponent<Unit>();
                unit.unitHealth += healPerFrame;
                break;

            case ObjectType.Building:
                Constructable constructable = storedTarget.GetComponent<Constructable>();
                constructable.constHealth += healPerFrame;
                break;

            case ObjectType.Base:
                PlayerBaseCtrl.Instance.baseHealth += healPerFrame;
                break;
        }
        
        timeLeft -= deltaTime;
        return timeLeft > 0;
    }
}
#endregion

#region Fallen Units Raise
public class RaiseFallenUnits : Effect {
    // Properties
    private Material spectorGlow;
    private List<Vector3> spawnedPos = new List<Vector3>();
    private List<GameObject> spectors = new List<GameObject>();

    // Constructor
    public RaiseFallenUnits(float mod, float duration, ObjectType objectType = ObjectType.Base, GameObject sourceObj = null, bool isItem = false) : base(mod, duration, objectType, sourceObj, isItem) {
        effectType = EffectType.RaiseFallenUnits;
        spectorGlow = PlayerBaseCtrl.Instance.spectorMaterial;
    }

    // Apply Effect
    public override void Apply(GameObject target) {
        PlayerBaseCtrl baseCtrl = PlayerBaseCtrl.Instance;
        if (baseCtrl.fallenUnits.Count == 0) return;

        int unitsToRaise = Mathf.Min((int)mod, baseCtrl.fallenUnits.Count); // ensures we don't raise more units than have been killed Mathf.Min gets the lower of 2 values
        List<int> raisedUnitIDs = new List<int>();

        for (int i = 0; i < unitsToRaise; i++) {
            // get fallen unit ID's from list
            int unitID = baseCtrl.fallenUnits[i];

            // get unit data from database using the ID set in the Database
            ObjectData unitData = DatabaseManager.Instance.databaseSO.GetObjectByID(unitID);
            if (unitData == null || !unitData.isUnit) continue;

            // find valid spawn position
            Vector3? spawnPos = FindValidSpawnPosition(baseCtrl.sacredTreeTransform.position);
            if (!spawnPos.HasValue) {
                Debug.Log("Couldn't find a valid spawn position for raised unit.");
                continue;
            }

            // Place the unit
            GameObject prefab = unitData.Prefab;
            int index = ObjectPlacer.Instance.PlaceObject(prefab, spawnPos.Value, 0f, unitID);

            // Apply spector ghosty guy material
            if (spectorGlow != null) {
                GameObject placedUnit = ObjectPlacer.Instance.placedGameObjects[index];
                ApplySpectorMaterial(placedUnit);

                spectors.Add(placedUnit);
                raisedUnitIDs.Add(unitID);
            } 

        }
    }

    public override void Remove(GameObject target) {
        // Destroy spectors when effect ends
        foreach (GameObject unit in spectors) {
            if (unit != null) {
                GameObject.Destroy(unit);
            }
        }

        // clear list when effect ends
        spectors.Clear();
        
    }
    public override EffectType GetEffectType() => EffectType.RaiseFallenUnits; 


    #region Helper Methods (Find Position, Apply Material)

    private Vector3? FindValidSpawnPosition(Vector3 centerPos) {
        PlacementSystem placementSystem = ResourceManager.Instance.placementSystem;
        GridData floorData = placementSystem.GetFloorData();
        Grid grid = GridZoneManager.Instance.GetGrid();

        float radius = 30f; // spawn radius around center of base
        int maxAttempts = 30; // max attempts before it gives up findins a position

        for (int i = 0; i < maxAttempts; i++) {
            // Get Random position in circle
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * radius;
            Vector3 potentialPos = new Vector3(centerPos.x + randomPoint.x, centerPos.y, centerPos.z + randomPoint.y);

            // Raycast to check terrain height
            if (Physics.Raycast(potentialPos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f)) {
                potentialPos.y = hit.point.y;

                // check if to close to other spawned units
                bool tooClose = false;
                foreach (Vector3 usedPos in spawnedPos) {
                    if (Vector3.Distance(potentialPos, usedPos) < 2f) {
                        tooClose = true;
                        break;
                    }
                }

                // check if position overlaps with obstacles
                bool overlapsObstacle = Physics.CheckSphere(potentialPos, 1f, LayerMask.GetMask("Constructable", "Unit", "Enemy"));

                // check grid position
                Vector3Int gridPos = grid.WorldToCell(potentialPos);
                bool cellOccupied = !floorData.CanPlaceObjectAt(gridPos, Vector2Int.one);

                // Check if position is on Nav Mesh (out _ is a way of ignoring the out, tells C# that we know there's output but we don't need it, avoids creating a variable I don't need) 
                bool onNavMesh = NavMesh.SamplePosition(potentialPos, out _, 1.0f, NavMesh.AllAreas);

                if (!tooClose && !overlapsObstacle && !cellOccupied && onNavMesh) {
                    return potentialPos;
                }
            }
        }
        return null; // not valid position found
    }

    private void ApplySpectorMaterial(GameObject unit) {
        Renderer[] renderers = unit.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers) {
            Material[] currentMaterials = renderer.materials;
            Material[] newMaterials = new Material[currentMaterials.Length];
            for (int i = 0; i < newMaterials.Length; i++) {
                // Create instance of the spector material
                newMaterials[i] = new Material(spectorGlow);
            }
            // Apply materials
            renderer.materials = newMaterials;
        }

        // Add a component to track this as a spector unit
        unit.AddComponent<Spector>();
    }
    #endregion

}
#endregion

