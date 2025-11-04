using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{

    public LayerMask clickable;
    public LayerMask ground;
    public LayerMask attackable;
    public LayerMask constructable;
    public LayerMask resourceNode;

    public GameObject groundMarker;

    private Camera cam;

    public bool attackCursorVisible;

    public bool playedDuringDrag = false;

    // This is a Singleton
    public static UnitSelectionManager Instance {get; set;}

    public List<GameObject> allUnitsList = new List<GameObject>();
    public List<GameObject> unitSelected = new List<GameObject>();

    // Makes sure this is the only one 
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    private void Start() {
        cam = Camera.main;
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // If we are hitting a clickable object
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable)) {
                if (Input.GetKey(KeyCode.LeftShift)) {
                    MultiSelect(hit.collider.gameObject);
                } else {
                   SelectByClicking(hit.collider.gameObject);
                }
            } else {
                if (!Input.GetKey(KeyCode.LeftShift)) {
                    DeselectAll();
                } 
            }
            

        }

        if (Input.GetMouseButtonDown(1) && unitSelected.Count > 0) {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // If we are hitting a clickable object
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground)) {
                groundMarker.transform.position = hit.point;
                // Makes sure the animation plays only once if clicking several times
                groundMarker.SetActive(false);
                groundMarker.SetActive(true);
            }
        }

        // Interact with Attackable and Resource Objects
        if (unitSelected.Count > 0 && AtLeastOneOffensiveUnitSelected(unitSelected)) {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // If we are hitting a clickable object
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, attackable)) {
                Debug.Log("Enemy Hovered with Mouse");

                attackCursorVisible = true;

                if (Input.GetMouseButtonDown(1)) {
                    Transform target = hit.transform;
                    foreach (GameObject unit in unitSelected) {
                        if (unit.GetComponent<AttackController>()) {
                        unit.GetComponent<AttackController>().targetToAttack = target;
                        }
                    }
                } else {
                    attackCursorVisible = false;
                }
            }
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, resourceNode)) {
                    // Debug.Log($"Hit object: Resource Node");

                    if (Input.GetMouseButtonDown(1)) {
                        Transform target = hit.transform; // get the transform of the hit object
                        foreach (GameObject unit in unitSelected) {
                            if (unit.GetComponent<ResourceGatherer>()) {
                                var gatherer = unit.GetComponent<ResourceGatherer>();
                                gatherer.targetResourceNode = target;
                                gatherer.SetResourceState(ResourceState.Gathering);
                            }
                        }
                    } 
                } 
                 
        }
        CursorSelector();
    }

    private void CursorSelector() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // use Raycast to check to see if hitting a specific layer mask
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable)) {
            CursorManager.Instance.SetMarkerType(CursorManager.CursorType.Selectable);
        }
        else if (ResourceManager.Instance.placementSystem.inSellMode) {
            // Debug.Log("Selling Cursor");
            CursorManager.Instance.SetMarkerType(CursorManager.CursorType.SellCursor);
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, attackable) && unitSelected.Count > 0 && AtLeastOneOffensiveUnitSelected(unitSelected)) {
            CursorManager.Instance.SetMarkerType(CursorManager.CursorType.Attackable);

        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, constructable) && unitSelected.Count > 0) {
            CursorManager.Instance.SetMarkerType(CursorManager.CursorType.UnAvailable);
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground) && unitSelected.Count > 0) {
            CursorManager.Instance.SetMarkerType(CursorManager.CursorType.Walkable);
        }
        else {
            CursorManager.Instance.SetMarkerType(CursorManager.CursorType.None);
            // Debug.Log("No Cursor");
        }


    }
        

    private bool AtLeastOneOffensiveUnitSelected(List<GameObject> unitSelected)
    {
        foreach (GameObject unit in unitSelected) {
            if (unit != null && unit.GetComponent<AttackController>()) {
                return true;
            }
        }    
        return false;
    }

    private void MultiSelect(GameObject unit) {
        if (unitSelected.Contains(unit) == false) {
            unitSelected.Add(unit);
            SelectUnit(unit, true);
        } else {
            SelectUnit(unit, false);
            unitSelected.Remove(unit);
        }
    }

    public void DeselectAll() {
        foreach (var unit in unitSelected) {
            SelectUnit(unit, false);
        }
        groundMarker.SetActive(false);
        unitSelected.Clear();
    }

    internal void DragSelect(GameObject unit) {
        if (unitSelected.Contains(unit) == false) {
            unitSelected.Add(unit);
            SelectUnit(unit, true);
        }
    }

    private void SelectByClicking(GameObject unit) {
        DeselectAll();
        unitSelected.Add(unit); //adds unit that was hit with Raycast to the list

        SelectUnit(unit, true);
    }

    // Turns on or off indicator and disables or enables the movement of the unit
    private void SelectUnit(GameObject unit, bool isSelected) {
        
        TriggerSelectionIndicator(unit, isSelected);
        EnableUnitMovement(unit, isSelected);
    }

    /// This is where the script is enabled for a unit so that it's listening to commands
    private void EnableUnitMovement(GameObject unit, bool shouldMove) { 
        unit.GetComponent<UnitMovement>().enabled = shouldMove;
        
    }

    private void TriggerSelectionIndicator(GameObject unit, bool isVisible) {
        // unit.transform.GetChild(0).gameObject.SetActive(isVisible); //--------------- looked for the first child before;
        GameObject indicator = unit.transform.Find("Indicator").gameObject;
        
        if (!indicator.activeInHierarchy && playedDuringDrag == false){
            SoundManager.Instance.PlayUnitSelectionSound();
            playedDuringDrag = true;
        }
        indicator.SetActive(isVisible);
    }


}
