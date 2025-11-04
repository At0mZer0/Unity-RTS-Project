using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour {

    // This is a Singleton
    public static ObjectPlacer Instance { get; private set; }

    [SerializeField]
    public List<GameObject> placedGameObjects = new();
    // for enemies to see player structures directly 
    // public IReadOnlyList<GameObject> placedObjects => placedGameObjects;
    public GameObject currentPlacedObj { get; private set;}


    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }


    public int PlaceObject(GameObject prefab, Vector3 position, float rotation = 0f, int databaseID = -1) {
       
        // We instantiate the prefab into the cell
        GameObject newObject = Instantiate(prefab);
        currentPlacedObj = newObject;
        newObject.transform.position = position;
        newObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
        
        // Enable different things for example activate the obstacle
        if ( newObject.CompareTag("PlayerBase")) {
            newObject.GetComponent<Constructable>().ConstructableWasPlaced(position);
            // Storing the positions that are now occupied
            placedGameObjects.Add(newObject);
        } else if (newObject.CompareTag("Unit")) {
            Unit unit = newObject.GetComponent<Unit>();
            unit.databaseID = databaseID;
            unit.UnitWasPlaced();
        }
        return placedGameObjects.Count - 1;
    }

    internal void RemoveObjectAt(int gameObjectIndex) {
        if(placedGameObjects.Count <= gameObjectIndex || placedGameObjects[gameObjectIndex] == null)
             return;
        Destroy(placedGameObjects[gameObjectIndex]);
        placedGameObjects[gameObjectIndex] = null;
    }

    public GameObject GetObjectAt(int gameObjectIndex) {
        if (gameObjectIndex < 0 || gameObjectIndex >= placedGameObjects.Count)
            return null;
        return placedGameObjects[gameObjectIndex];
    }
}
