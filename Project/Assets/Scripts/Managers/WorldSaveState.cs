using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

public class WorldSaveState : MonoBehaviour
{
    [SerializeField] private List<PlacedObjectData> placedObjects = new List<PlacedObjectData>();
    public string worldName = "Test";

    public void SaveWorld()
    {
        SaveData saveData = new SaveData()
        {
            placedObjects = placedObjects
        };
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        string filePath = Path.Combine(Application.persistentDataPath, "CozySaveData", worldName + ".json");
        File.WriteAllText(filePath, json);
        Console.WriteLine($"PlacedObjectData list saved to {filePath}");
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

[Serializable]
public struct PlacedObjectData
{
    public ulong buildingId;
    public float3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public int state;
    public int secondaryState;
}

[Serializable]
public struct ResourceObjectData
{
    public ResourceController resource;
}

[Serializable]
public struct SaveData
{
    public List<PlacedObjectData> placedObjects;
    public List<ResourceObjectData> resources;
}