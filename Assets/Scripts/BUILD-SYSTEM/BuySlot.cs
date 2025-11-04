using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public class BuySlot : MonoBehaviour {

    public Sprite availableSprite;
    public Sprite unavailableSprite;

    public bool isAvailable;

    public BuySystem buySystem;

    public int databaseItemID;

    private void Start() {

        ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
        HandleResourceChanged();

        ResourceManager.Instance.OnBuildingsChanged += HandleBuildingChanged;
        HandleBuildingChanged();

    }


    public void ClickedOnSlot() {
        if (isAvailable) {
            buySystem.placementSystem.StartPlacement(databaseItemID);
        }

    }

    private void UpdateAvailabilityUI() {
        if (!isAvailable) {
            GetComponent<Image>().sprite = unavailableSprite;
            GetComponent<Button>().interactable = false;
        }
        else {
            GetComponent<Image>().sprite = availableSprite;
            GetComponent<Button>().interactable = true;
            // GetComponent<Image>().color = Color.green;       
        }
    }



    private void HandleResourceChanged() {
        // Gets the index from the database 
        ObjectData objectData = DatabaseManager.Instance.databaseSO.objectsData[databaseItemID];

        bool requirementMet = true;

        // loops through the requirements and checks to see if they're met
        foreach (BuildRequirement req in objectData.resourceRequirements) {
            
            if (ResourceManager.Instance.GetResourceAmount(req.resource) < req.amount) {
                requirementMet = false;
                break; // breaks if even on of the requirements aren't met and stays false set above
            }
        }

        isAvailable = requirementMet;
        UpdateAvailabilityUI();
    }

    private void HandleBuildingChanged() {
        ObjectData objectData = DatabaseManager.Instance.databaseSO.objectsData[databaseItemID];

        foreach (BuildingType dependency in objectData.buildDependency) {
            // if no dependency
            if (dependency == BuildingType.None) {
                gameObject.SetActive(true);
                return;
            }

            // Check for dependency
            if (!ResourceManager.Instance.allExistingBuildings.Contains(dependency)) {
                gameObject.SetActive(false);
                return;
            }
        }
        
        // If all dependencies are met
        gameObject.SetActive(true);

    }



}
