using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingObjectManager : MonoBehaviour
{
    [SerializeField] private BuildingObject[] buildingObjects;

    private static BuildingObjectManager instance;

    public static BuildingObjectManager Instance { get => instance; }

    private void Start()
    {
        if(Instance == null)
        {
            instance = this;
            AssignItemIds();
        }
        else
        {
            Destroy(this);
        }
    }

    private void AssignItemIds()
    {
        ulong i = 0;
        foreach (BuildingObject buildingObject in buildingObjects)
        {
            buildingObject.buildingId = i;
            i++;
        }
    }

    public static BuildingObject GenerateBuildingObject(ulong i)
    {
        return Instantiate(Instance.buildingObjects[i]);
    }
    public static BuildingObject GenerateBuildingObject(string name)
    {
        int i = 0;
        foreach(BuildingObject buildingObject in Instance.buildingObjects)
        {
            if(buildingObject.buildingName == name)
            {
                return Instantiate(Instance.buildingObjects[i]);
            }
            i++;
        }
        return null;

    }

    public static ulong GetBuildingId(string buildingName)
    {
        ulong i = 0;
        foreach (BuildingObject buildingObject in Instance.buildingObjects)
        {
            if (buildingObject.buildingName == buildingName)
            {
                return i;
            }
            i++;
        }
        return 0;
    }

    public static BuildingObject AccessStaticBuildingObjectData(ulong id)
    {
        return Instance.buildingObjects[id];
    }

}
