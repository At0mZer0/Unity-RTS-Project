using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{

    [SerializeField] private Camera sceneCamera;
    [SerializeField] private LayerMask placementLayerMask;
    [SerializeField] private LayerMask constructableLayerMask;
    public GameObject uiBuildMenu;
    public GameObject uiDruidHutMenu;

    [SerializeField]  private Vector3 lastPosition;

    public event Action OnClicked, OnExit;
    // public event Action<int> OnRotated; // <int> allows me to pass data in with the event instead of writing 2 functions for clockwise and counterclockwise
    public event Action<Vector3Int> OnVariantCycled;

    private void Update() {
        if (Input.GetMouseButtonDown(0)) // Left Click
             OnClicked?.Invoke();

        if (Input.GetMouseButtonDown(1) ) {
            bool isInBuildMenu = HandleBuildingMenus();

            if (!isInBuildMenu) {
                OnExit?.Invoke();
            }
        }
             
        if (Input.GetKeyDown(KeyCode.Escape)) 
             OnExit?.Invoke();

        if(Input.GetKeyDown(KeyCode.Q)) {
            Vector3 pos = GetSelectedMapPosition();
            Vector3Int gridPos = GridZoneManager.Instance.GetGrid().WorldToCell(pos);
            OnVariantCycled?.Invoke(gridPos);
        }
        if(Input.GetKeyDown(KeyCode.U)) {
            // Toggle Build menu on / off
            uiBuildMenu.SetActive(!uiBuildMenu.activeSelf);
        }
        
        // Mouse Wheel direction detection lol
        // float scroll = Input.mouseScrollDelta.y;
        // if (scroll != 0) {
        //     int rotateDir = scroll > 0 ? 1 : -1; // +1 for up (90 deg), etc..
        //     OnRotated?.Invoke(rotateDir);
        // }
    }

    public bool IsPointerOverUI() => EventSystem.current.IsPointerOverGameObject();


    public Vector3 GetSelectedMapPosition() {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        
        Ray ray = sceneCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, placementLayerMask)) {
            lastPosition = hit.point;
        }
        return lastPosition;
    }

    private bool HandleBuildingMenus() {
        if (Input.GetMouseButtonDown(1)) {
            if (ResourceManager.Instance.placementSystem.inSellMode == false) {
                Ray ray = sceneCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, constructableLayerMask)) {
                    string prefabName = hit.collider.gameObject.name;
                    if (prefabName.Contains("(Clone)")) {
                        // Remove "(Clone)" from the prefab name
                        prefabName = prefabName.Replace("(Clone)", "").Trim(); // Remove "(Clone)" and trim whitespace
                    }
                    Debug.Log($"Hit building: {prefabName}");

                    switch (prefabName) {
                        case "DruidHut":
                            uiDruidHutMenu.SetActive(!uiDruidHutMenu.activeSelf);
                            break;
         
                    }
                    return true;
                }
            } 
        }
        return false;
    }
    

}
