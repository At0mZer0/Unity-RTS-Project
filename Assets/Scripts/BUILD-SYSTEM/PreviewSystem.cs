using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewSystem : MonoBehaviour {
    [SerializeField] 
    private float previewYOffset = 0.06f;
    private float currentRotation = 0f;

    private GameObject previewObject;
    private List<GameObject> chainPreviewObjects = new List<GameObject>();

    private GameObject currentPrefab;

    [SerializeField] 
    private Material previewMaterialPrefab;
    private Material previewMaterialInstance;


    private void Start() {
        previewMaterialInstance = new Material(previewMaterialPrefab);
    }

    public void UpdateRotation(float rotationDeg) {
        currentRotation = rotationDeg;
        if (previewObject != null) {
            previewObject.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
        }
    }

    public void StartShowingPlacementPreview(GameObject prefab, Vector2Int size) {
        currentPrefab = prefab;
        previewObject = Instantiate(prefab);
        PreparePreview(previewObject);
        previewObject.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
    }

    internal void StartShowingRemovePreview() {
        ApplyFeedbackToCursor(false);
    }

    private void PreparePreview(GameObject previewObject) {
        // Change the materials of the prefab (and its children) to semi-transparent

        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            Material[] materials = renderer.materials;
            
            for (int i = 0; i < materials.Length; i++){
                // Getting the current material color
                Color color = materials[i].color;
     
                // changing its alpha
                color.a = 0.5f;

                // setting the modified color
                materials[i].color = color;
                materials[i] = previewMaterialInstance;
            }
            renderer.materials = materials;
        }
    }

    public void StopShowingPreview() {
        if (previewObject != null){
            Destroy(previewObject);
        }
    }

    public void UpdatePosition(Vector3 position, bool validity){
        if (previewObject != null) {
            MovePreview(position);
            ApplyFeedbackToPreview(validity);
            previewObject.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
        }
        ApplyFeedbackToCursor(validity);
    }

    // Added in for Chainable Objects
    public void UpdateMultiplePositions(List<Vector3> positions, bool validity) {
        ClearChainPreviews();

        foreach (var pos in positions) {
            // Create preview for each position in the list
            GameObject previewObject = Instantiate(currentPrefab);
            PreparePreview(previewObject);
            previewObject.transform.position = pos;
            previewObject.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
            chainPreviewObjects.Add(previewObject);

            // Make Green or Red
            ApplyFeedbackToPreview(validity);
        }
    }

    private void ClearChainPreviews() {
        // Destroy the model in the scene
        foreach (var obj in chainPreviewObjects) {
            Destroy(obj);
        }
        // clear the reference list
        chainPreviewObjects.Clear();
    }

    private void ApplyFeedbackToPreview(bool validity) {
        Color c = validity ? Color.green : Color.red;
        c.a = 0.5f;
        previewMaterialInstance.color = c;

        previewMaterialInstance.EnableKeyword("_EMISSION");

        Color finalColor = c * Mathf.LinearToGammaSpace(1); //  boost to the Gamma (adjusts for monitor brightness curves) standard gamma correction is 2.2
        previewMaterialInstance.SetColor("_EmissionColor", finalColor);

    }

    private void ApplyFeedbackToCursor(bool validity){
        Color c = validity ? Color.green : Color.red;
        c.a = 1f;
    }

    private void MovePreview(Vector3 position) {
        previewObject.transform.position = new Vector3(position.x, position.y + previewYOffset, position.z);
    }

}
