using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu] // allows me to display in the Asset Menu and store in .asset file 
public class ObjectsDatabseSO : ScriptableObject
{
    public List<ObjectData> objectsData;

    public ObjectData GetObjectByID(int id){
        foreach (ObjectData obj in objectsData) {
            if (obj.ID == id) {
                return obj;
            }
        }
        return new(); // This cannot happen
    }

}

public enum BuildingType {
    None,
    SacredTree,
    ResourceCollector,
    ArcherTower,
    WardingTower,
    Walls,
    Huts,
    DruidHut,
	Granary,
	ResourceCollector2,
	RitualSite01,
	RitualSite02,
	RitualSite03,
	RitualSite04,
	RitualSite05,
	StoneCircle,
	Roundhouse,
	Sanctuary,
	WeaponForge,
	WheatField,
    WoodenFence,
    Level2,
    Level3,
    Has_3_Roundhouse,
    Druid,
    Warrior
}

[System.Serializable]
public class ObjectData {
    [field: SerializeField]
    public string Name { get; private set; }

    [field: SerializeField]
    public int ID { get; private set; }

    [field: SerializeField]
    public BuildingType thisBuildingType { get; private set; } /// added the enum BuildingType

    [field: SerializeField]
    [TextArea(3, 10)]
    public string description;

    [field: SerializeField]
    public int numVariants {get; private set;} = 1;
    
    [field: SerializeField]
    public GameObject[] PrefabVariants {get; private set;}
    
    [field: SerializeField]
    public Vector2Int[] SizeVariants {get; private set;}

    public Vector2Int Size { 
        get { return SizeVariants != null && SizeVariants.Length > 0 ? SizeVariants[0] : Vector2Int.one;}
    }

    public GameObject Prefab { 
        get { return PrefabVariants != null && PrefabVariants.Length > 0 ? PrefabVariants[0] : null; }
    }

    [field: SerializeField]
    public int baseExpandWidth {get; private set;} = 2; 

    [field: SerializeField]
    public int baseExpandLength {get; private set;} = 2; 

    [field: SerializeField]
    public List<BuildRequirement> resourceRequirements { get; private set; }

    [field: SerializeField]
    public List<BuildingType> buildDependency { get; private set; } // For the Build Tree

    [field: SerializeField]
    public List<Benefits> benefits { get; private set; }

    [field: SerializeField]
    public bool isUnit {get; private set;} = false;

    [field: SerializeField]  
    public bool isChainable { get; private set; } = false;

    [field: SerializeField]
    public float chainSpacing {get; private set;} = 1.0f;

}

[System.Serializable]
public class BuildRequirement {
    public ResourceManager.ResourceType resource;
    public int amount;
}

// used in EffectConfig to set which list of objects to apply effect to
public enum EffectTarget {
    Base,
    AllPlayerUnits,
    AllEnemyUnits,
    EnemyRomans,
    EnemyFae,
    Buildings
}

// Used to create new Status Effects in the inspector
[System.Serializable]
public class EffectConfig {
    public EffectType effectType;
    public EffectTarget effectTarget;
    public float mod;
    public float duration;
    public ObjectType objectType;
    public bool isItem = false;

}

// Updated to allow for Benefits to include multiple status effects
[System.Serializable]
public class Benefits {
    public string name;
    public string description;
    // public Sprite benefitIcon; // will implement later for UI
    public List<EffectConfig> effects = new List<EffectConfig>();
}


