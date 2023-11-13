using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    [HideInInspector]
    public float gridSize = 0.25f;
    private float[] gridSizes = new float[] { 0, 0.25f};
    private int gridSizeIndex = 1;
    public float raycastMaxDistance = 7;
    public LayerMask layerMask;
    public LayerMask layerMaskDelete;
    public ProjectionHandler projectionInstance;
    public BuildingObjectHandler projectionBuildingObjectHandler;
    private Transform cameraMainTransform;
    private bool canUpdateProjectionPosition = false;
    private float sphereCastRadius = 0.1f;
    private static BuildManager instance;
    public ulong currentBuildingId;
    public BuildingObject.BuildingType currentBuildingType;

    private float heightOffset;
    private Vector3 lastSavedProjectionPosition;
    public bool flip = false;
    public bool snapping = true;

    public bool CanUpdateProjectionPosition { get => canUpdateProjectionPosition; set => canUpdateProjectionPosition = value; }
    public static BuildManager Instance { get => instance; }

    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
        {
            cameraMainTransform = Camera.main.transform;
            instance = this;
            CallProjection(0);
            gridSize = gridSizes[gridSizeIndex];
            GameUI.Instance.UpdateGridSizeText();
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void ChangeGridSize()
    {
        gridSizeIndex++;
        if(gridSizeIndex >= gridSizes.Length)
        {
            gridSizeIndex = 0;
        }
        gridSize = gridSizes[gridSizeIndex];
        GameUI.Instance.UpdateGridSizeText();
    }



    private void FixedUpdate()
    {
        if(CanUpdateProjectionPosition && heightOffset == 0)
        {
            UpdateProjectionPosition();
        }
    }

    public virtual void SetHeightOffset(bool value)
    {
        if(value)
        {
            heightOffset += InputManager.Instance.cameraMovement.y * Options.Instance.mouseRotationSpeed * 0.25f;
            Vector3 orignPoint = CheckGridPlacement(lastSavedProjectionPosition);
            projectionInstance.transform.position = RoundVector(lastSavedProjectionPosition + new Vector3(0, heightOffset, 0), gridSize, orignPoint);
        }
    }

    private void Update()
    {
        if(CanUpdateProjectionPosition && Mathf.Abs(InputManager.Instance.CameraZoomDelta) > 0.1f)
        {
            if(InputManager.Instance.CameraZoomDelta > 0)
            {
                projectionInstance.transform.eulerAngles += new Vector3(0, 22.5f, 0);
            }
            else
            {
                projectionInstance.transform.eulerAngles += new Vector3(0, -22.5f, 0);
            }
        }
    }

    public virtual void FlipModel(bool value)
    {
        flip = value;
        if(flip)
        {
            projectionInstance.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            projectionInstance.transform.localScale = new Vector3(1, 1, 1);
        }

    }

    public virtual void EnableSnapping(bool value)
    {
        snapping = value;
    }

    /* Not used?
    public virtual void CallProjection(GameObject prefab)
    {
        DismissProjection();
        projectionInstance.SpawnProjection(prefab);
        UpdateProjectionPosition();
    }
    */

    public virtual void CallProjection(int id)
    {
        DismissProjection();
        BuildingObject buildingObject = BuildingObjectManager.GenerateBuildingObject((ulong)id);
        GameObject projectionPrefab = buildingObject.buildingPrefab;
        projectionBuildingObjectHandler = projectionInstance.SpawnProjection(projectionPrefab).GetComponent<BuildingObjectHandler>();
        currentBuildingType = buildingObject.buildingType;
        currentBuildingId = (ulong)id;
        UpdateProjectionPosition();
    }

    public virtual void PlaceBuildingObject()
    {
        heightOffset = 0;
        GameManager.Instance.PlaceBuildingServerRpc(currentBuildingId, projectionBuildingObjectHandler.basePivot.transform.position, projectionInstance.transform.rotation, flip);
    }

    public void DestroyBuilding()
    {
        RaycastHit raycastHit;
        if (Physics.SphereCast(cameraMainTransform.position, sphereCastRadius, cameraMainTransform.forward, out raycastHit, raycastMaxDistance, layerMaskDelete))
        {
            BuildingObjectHandler boh = raycastHit.collider.transform.root.GetComponent<BuildingObjectHandler>() ?? null;
            if (boh != null)
            {
                GameManager.Instance.DespawnBuildingServerRpc(boh.GetComponent<NetworkObject>().NetworkObjectId,NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    public virtual void PlaceBuildingObject(int id)
    {
        GameManager.Instance.PlaceBuildingServerRpc((ulong)id, projectionInstance.transform.position, projectionInstance.transform.rotation, flip);
    }

    public void UpdateProjectionPosition()
    {
        Vector3 projectionPosition = GetRaycastPosition(cameraMainTransform.position, cameraMainTransform.forward);
        Vector3 orignPoint = CheckGridPlacement(projectionPosition);

        projectionInstance.transform.position = RoundVector(projectionPosition, gridSize, orignPoint);
    }

    //Replace this Method maybe?
    private Vector3 CheckGridPlacement(Vector3 pos)
    {
        return Vector3.zero;
        foreach(BuildingBeacon beacon in GameManager.Instance.buildingBeacons)
        {
            if (IsObjectWithinCube(pos, beacon.transform.position, beacon.beaconSize))
            {
                print("Is in Becon Range");
                return beacon.transform.position;
            }
        }
        return Vector3.zero;
    }

    bool IsObjectWithinCube(Vector3 objectPosition, Vector3 cubeCenter, Vector3 cubeSize)
    {
        Bounds cubeBounds = new Bounds(cubeCenter, cubeSize);

        return cubeBounds.Contains(objectPosition);
    }

    public void DismissProjection()
    {
        projectionInstance.DismissProjection();
    }
    
    public Vector3 GetRaycastPosition(Vector3 startPoint, Vector3 direction)
    {
        RaycastHit[] raycastHits = Physics.SphereCastAll(startPoint, sphereCastRadius, direction, raycastMaxDistance, layerMask);
        if(raycastHits.Length > 0)
        {
            raycastHits = raycastHits.OrderBy(hit =>
            {
                float distanceToPoint = Vector3.Distance(startPoint, hit.point);
                float distanceToCollider = Vector3.Distance(startPoint, hit.collider.transform.position);
                return Tuple.Create(distanceToPoint, distanceToCollider);
            }, new RaycastComparer()).ToArray();
            bool closestPointWithoutSnappingFound = false;
            foreach (RaycastHit raycastHit in raycastHits)
            {
                if(raycastHit.distance < 3) // Skip everything too close to the camera
                {
                    continue;
                }
                SnapHitboxHandler snapHitbox = raycastHit.collider.GetComponent<SnapHitboxHandler>() ?? null;
                if (snapHitbox != null && snapping)
                {
                    int snapperIndex = 0;
                    foreach(BuildingObject.BuildingType permittedSnapper in snapHitbox.snapBuildingTypes)
                    {
                        if(permittedSnapper == currentBuildingType)
                        {
                            Transform snappingPoint = raycastHit.collider.transform;
                            BuildingObjectHandler buildingObjectHandler = raycastHit.collider.transform.root.GetComponent<BuildingObjectHandler>() ?? null;
                            if(buildingObjectHandler == null)
                            {
                                Debug.LogWarning("No Building Object Handler found on the building? Add it please");
                            }
                            else
                            {
                                projectionBuildingObjectHandler.ChangePivot(snappingPoint.GetComponent<SnapHitboxHandler>().pivotIndex[snapperIndex]);
                                lastSavedProjectionPosition = snappingPoint.position;
                                return lastSavedProjectionPosition;
                            }
                        }
                        snapperIndex++;
                    }
                    continue;
                }
                if(!closestPointWithoutSnappingFound && snapHitbox == null)
                {
                    projectionBuildingObjectHandler.ChangePivot(0);
                    lastSavedProjectionPosition = raycastHit.point + new Vector3(0, heightOffset, 0);
                    closestPointWithoutSnappingFound = true;
                }
                else
                {
                    continue;
                }
                break;
            }
        }
        else
        {
            lastSavedProjectionPosition = new Ray(startPoint, direction).GetPoint(raycastMaxDistance);
        }
        return lastSavedProjectionPosition;
    }

    // Helper class to store RaycastHit and corresponding distance
    private class HitDistance
    {
        public RaycastHit hit;
        public float distance;

        public HitDistance(RaycastHit hit, float distance)
        {
            this.hit = hit;
            this.distance = distance;
        }
    }

    // Custom method to round Vector3 components to a specified precision
    Vector3 RoundVector(Vector3 vector, float precision, Vector3 gridOrigin)
    {
        Vector3 originOffset = new Vector3(gridOrigin.x % 1, gridOrigin.y % 1, gridOrigin.z % 1);
        float x = RoundToPrecision(vector.x, precision);
        float y = RoundToPrecision(vector.y, precision);
        float z = RoundToPrecision(vector.z, precision);

        return new Vector3(x, y, z) + originOffset;
    }

    float RoundToPrecision(float value, float precision)
    {
        if(precision == 0)
        {
            return value;
        }
        return Mathf.Round(value / precision) * precision;
    }
}

public class RaycastComparer : IComparer<Tuple<float, float>>
{
    public int Compare(Tuple<float, float> x, Tuple<float, float> y)
    {
        // Compare distances manually
        int distanceToPointComparison = x.Item1.CompareTo(y.Item1);
        if (distanceToPointComparison != 0)
        {
            return distanceToPointComparison;
        }
        else
        {
            return x.Item2.CompareTo(y.Item2);
        }
    }
}






