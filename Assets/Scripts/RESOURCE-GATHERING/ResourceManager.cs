using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceManager : MonoBehaviour {
    public static ResourceManager Instance {get; set;} // This is a Singleton

    // Makes sure this is the only one 
    private void Awake() {
        if (Instance != null && Instance != this) {
            // Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    // public int credits = 300;
    public int wood = 100;
    public int stone = 100;
    public int food = 0;
    public int runes = 0;

    public event Action OnResourceChanged;
    public event Action OnBuildingsChanged;
    // public TextMeshProUGUI creditsUI;
    public TextMeshProUGUI stoneUI;
    public TextMeshProUGUI woodUI;
    public TextMeshProUGUI foodUI;
    public TextMeshProUGUI runesUI;

    public List<BuildingType> allExistingBuildings;

    public PlacementSystem placementSystem;

    public enum ResourceType {
        Credits,
        None,
        Wood,
        Stone,
        Food,
        Runes

    }

    private void Start() {
        
        UpdateUI();

    }


    public void UpdateBuildingChanged(BuildingType buildingType, bool isNew, Vector3 position) {
        if (isNew) {
            allExistingBuildings.Add(buildingType);
            SoundManager.Instance.PlayBuidlingConstructionSound();
        } 
        else {
            placementSystem.RemovePlacementData(position);
            allExistingBuildings.Remove(buildingType);
        }

        OnBuildingsChanged?.Invoke();
    }

    public void SellBuilding(BuildingType buildingType, GameObject building) {
        var constructable = building.GetComponent<Constructable>();
        if (constructable != null) {
            ObjectPlacer.Instance.placedGameObjects.Remove(building);

            UpdateBuildingChanged(constructable.buildingType, false, building.transform.position);
            SoundManager.Instance.PlayBuildingSellingSound();
            RefundResources(constructable.buildingType);
            Destroy(building);
        }
    }

    private void RefundResources(BuildingType buildingType) {
        foreach (ObjectData obj in DatabaseManager.Instance.databaseSO.objectsData) {
            if (obj.thisBuildingType == buildingType) {
                foreach (BuildRequirement req in obj.resourceRequirements) {
                    int refundAmount = req.amount;
                    IncreaseResource(req.resource, refundAmount);
                }
            }
        }
    }


    public void IncreaseResource(ResourceType resource, int amountToIncrease) {
        switch(resource) {
            // case ResourceType.Credits:
            //     credits += amountToIncrease;
            //     break;
            case ResourceType.Wood:
                wood += amountToIncrease;
                break;
            case ResourceType.Stone:
                stone += amountToIncrease;
                break;
            case ResourceType.Food:
                food += amountToIncrease;
                break;
            case ResourceType.Runes:
                runes += amountToIncrease;
                break;
            default:
                break;
        }

        OnResourceChanged?.Invoke();   /// invoke the event Action (all methods subscribed to it will run) (subscribe and unsubscribe using the -= and += operators)

    }

    public void DecreaseResource(ResourceType resource, int amountToDecrease) {
        switch(resource) {
            // case ResourceType.Credits:
            //     credits -= amountToDecrease;
            //     break;
            case ResourceType.Wood:
                wood -= amountToDecrease;
                break;
            case ResourceType.Stone:
                stone -= amountToDecrease;
                break;
            case ResourceType.Food:
                food -= amountToDecrease;
                break;
            case ResourceType.Runes:
                runes -= amountToDecrease;
                break;
            default:
                break;
        }
        OnResourceChanged?.Invoke();
    }



    private void UpdateUI() {
        // String interpolation (converts either floats or integers to strings, can also use formating like {credits:F2} to show 2 decimal places)
        // creditsUI.text = $"{credits}"; 
        stoneUI.text = $"{stone}"; 
        woodUI.text = $"{wood}";
        foodUI.text = $"{food}";  
        runesUI.text = $"{runes}";
    }

    // public int GetCredits() {
    //     return credits;
    // }

    internal int GetResourceAmount(ResourceType resource)
    {
        switch(resource) {
            // case ResourceType.Credits:
            //     return credits;
            case ResourceType.Wood:
                return wood;
            case ResourceType.Stone:
                return stone;
            case ResourceType.Food:
                return food;
            case ResourceType.Runes:
                return runes;
            default:
                break;
        }
        
        return 0;
    }



    internal void DecreaseResourcesBasedOnRequirements(ObjectData objectData) {
        foreach (BuildRequirement req in objectData.resourceRequirements) {
            DecreaseResource(req.resource, req.amount); 

        }

    }

    private void OnEnable() {
        OnResourceChanged += UpdateUI;   
    }

    private void OnDisable() {
        OnResourceChanged -= UpdateUI;   
    }

}




