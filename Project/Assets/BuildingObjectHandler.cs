using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingObjectHandler : MonoBehaviour
{
    public PlacedObjectData data;
    public Transform basePivot;
    public Transform[] snappingPoints;
    public Transform[] pivots;
    public bool requireBuildingBeacon = true;
    public bool grounded = false;

    public Vector3 GetClosestSnappingPoint(Vector3 position)
    {
        return GetClosestPoint(position, snappingPoints).position;
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
