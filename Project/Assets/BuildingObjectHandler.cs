using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Item;

public class BuildingObjectHandler : MonoBehaviour
{
    public PlacedObjectData data;
    public Transform basePivot;
    public Transform[] snappingPoints;
    public Transform[] pivots;
    public bool requireBuildingBeacon = true;
    public bool grounded = false;
    public Container itemContainer1;
    public Container itemContainer2;

    public Vector3 GetClosestSnappingPoint(Vector3 position)
    {
        return GetClosestPoint(position, snappingPoints).position;
    }

    public void UpdateItemSaveData()
    {
        if(itemContainer1 != null)
        {
            List<ulong> ids = new List<ulong>();
            List<int> count = new List<int>();
            foreach (ItemData item in itemContainer1.items)
            {
                ids.Add(item.itemId);
                count.Add(item.stackSize);
            }
            data.itemContainer1 = ids.ToArray();
            data.itemContainer1Amounts = count.ToArray();
        }

        if (itemContainer1 != null)
        {
            List<ulong> ids = new List<ulong>();
            List<int> count = new List<int>();
            foreach (ItemData item in itemContainer2.items)
            {
                ids.Add(item.itemId);
                count.Add(item.stackSize);
            }
            data.itemContainer2 = ids.ToArray();
            data.itemContainer2Amounts = count.ToArray();
        }
    }


    private Transform GetClosestPoint(Vector3 position, Transform[] points)
    {
        Transform closestPoint = transform;
        float distance = float.MaxValue;
        foreach (Transform point in points)
        {
            float newDistance = Vector3.Distance(point.position, position);
            if (newDistance < distance)
            {
                distance = newDistance;
                closestPoint = point;
            }
        }
        return closestPoint;
    }

    public void ChangePivot(int i)
    {
        basePivot.transform.position = pivots[i].transform.position;    
    }
}
