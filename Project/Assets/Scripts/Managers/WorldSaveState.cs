using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSaveState : MonoBehaviour
{
    [SerializeField] private List<PlacedObjectData> placedObjects = new List<PlacedObjectData>();

    public void SaveWorld()
    {

    }

    public void LoadWorld()
    {

    }

    public void AddPlacedObject(PlacedObjectData data)
    {
        placedObjects.Add(data);
    }

    public void RemovePlacedObject(PlacedObjectData data)
    {
        placedObjects.Remove(data);
    }
}

public struct PlacedObjectData
{
    public BuildingObject buildingObject;
    public GameObject prefab;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public int state;
    public int secondaryState;
}