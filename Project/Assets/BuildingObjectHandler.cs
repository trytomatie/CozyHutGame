using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingObjectHandler : MonoBehaviour
{
    public PlacedObjectData data;
    public Transform[] snappingPoints;
    public bool wallSnapping = false;
    public bool floorSnapping = false;

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
}
