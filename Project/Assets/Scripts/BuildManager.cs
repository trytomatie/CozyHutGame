using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static Item;

public class BuildManager : MonoBehaviour
{
    [HideInInspector]
    public float gridSize = 0.25f;

    public float raycastMaxDistance = 7;
    public LayerMask layerMask;
    public LayerMask layerMaskDelete;
    public ProjectionHandler projectionInstance;
    public BuildingObjectHandler projectionBuildingObjectHandler;

    public ulong currentBuildingId;
    public BuildingObject.BuildingType currentBuildingType;

    private float heightOffset;
    private Vector3 lastSavedProjectionPosition;
    public Vector3 groundedAlignment;
    private Vector3 rotationOffset;
    private float[] gridSizes = new float[] { 0};
    private int gridSizeIndex = 0;
    public bool flip = false;
    public bool snapping = true;

    private Transform cameraMainTransform;
    private bool canUpdateProjectionPosition = false;
    private float sphereCastRadius = 0.1f;
    private static BuildManager instance;
    public Container playerInventory;

    public TextMeshProUGUI selectedBuildingText;
    public ItemSlotUI[] buldingCostSlots;

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
            heightOffset += InputManager.Instance.cameraMovement.y * Options.Instance.mouseRotationSpeed * 0.025f;
            heightOffset = 0; // Just remove the Hight Offset for now
            Vector3 orignPoint = CheckGridPlacement(lastSavedProjectionPosition);
            projectionInstance.transform.position = RoundVector(lastSavedProjectionPosition + new Vector3(0, heightOffset, 0), gridSize, orignPoint);
        }
    }

    private void Update()
    {

        if (CanUpdateProjectionPosition && Mathf.Abs(InputManager.Instance.CameraZoomDelta) > 0.1f)
        {
            if(projectionBuildingObjectHandler.gigaGrounded)
            {
                if(rotationOffset.y != 0)
                {
                    rotationOffset = Vector3.zero;
                }
                if (InputManager.Instance.CameraZoomDelta > 0)
                {
                    rotationOffset += new Vector3(0, 0, 22.5f);
                }
                else
                {
                    rotationOffset += new Vector3(0, 0, -22.5f);
                }
            }
            else
            {
                projectionInstance.transform.eulerAngles = Vector3.zero;
                if (InputManager.Instance.CameraZoomDelta > 0)
                {
                    rotationOffset += new Vector3(0, 22.5f, 0);
                }
                else
                {
                    rotationOffset += new Vector3(0, -22.5f, 0);
                }
                projectionInstance.transform.eulerAngles = rotationOffset;
            }
        }
        if (projectionBuildingObjectHandler.gigaGrounded && CanUpdateProjectionPosition)
        {
            projectionInstance.transform.eulerAngles = groundedAlignment + rotationOffset;
        }
        else
        {
            projectionInstance.transform.eulerAngles = rotationOffset;
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
        projectionBuildingObjectHandler.enabled = false;
        currentBuildingType = buildingObject.buildingType;
        currentBuildingId = (ulong)id;
        UpdateProjectionPosition();
        RefreshBuildingDescription();
    }

    public virtual void PlaceBuildingObject()
    {
        heightOffset = 0;
        ItemData[] requiredItems;
        if(projectionBuildingObjectHandler.basePivot.transform.position != Vector3.zero && CanAffordSelectedBuilding(out requiredItems))
        {
            foreach (ItemData data in requiredItems)
            {
                playerInventory.RequestRemoveItemServerRpc(data);
            }

            GameManager.Instance.PlaceBuildingServerRpc(currentBuildingId, projectionBuildingObjectHandler.basePivot.transform.position, projectionInstance.transform.rotation, flip);
            RefreshBuildingDescription();
            StatisticsAPI.AddBuildingBuilt(currentBuildingId);
        }
    }

    public bool CanAffordSelectedBuilding(out ItemData[] requiredItems)
    {
        requiredItems = new ItemData[BuildingObjectManager.AccessStaticBuildingObjectData(currentBuildingId).buildingMaterialAmounts.Length];
        for(int i = 0; i < requiredItems.Length;i++)
        {
            requiredItems[i] = new ItemData(BuildingObjectManager.AccessStaticBuildingObjectData(currentBuildingId).buildingMaterials[i].itemId,
                BuildingObjectManager.AccessStaticBuildingObjectData(currentBuildingId).buildingMaterialAmounts[i]);
        }
        foreach(ItemData data in requiredItems)
        {
            if(data.stackSize > playerInventory.GetAmmountOfItem(data.itemId))
            {
                return false;
            }
        }
        return true;
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

    public void UpdateProjectionPosition()
    {
        Vector3 projectionPosition = GetRaycastPosition(cameraMainTransform.position, cameraMainTransform.forward);
#if UNITY_EDITOR
        Debug.DrawLine(cameraMainTransform.position, projectionPosition, Color.red);
#endif
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
        RaycastHit[] raycastHits = Physics.RaycastAll(startPoint, direction, raycastMaxDistance, layerMask);
        if(raycastHits.Length > 0) // If we hit something
        {
            raycastHits = raycastHits.OrderByDescending(hit => Vector3.Distance(startPoint, hit.collider.transform.position)).ToArray(); // Sort by distance to collider
            //raycastHits = raycastHits.OrderBy(hit =>
            //{
            //    float distanceToPoint = Vector3.Distance(startPoint, hit.point);
            //    float distanceToCollider = Vector3.Distance(startPoint, hit.collider.transform.position);
            //    return Tuple.Create(distanceToPoint, distanceToCollider);
            //}, new RaycastComparer()).ToArray();
            bool closestPointWithoutSnappingFound = false; //
            foreach (RaycastHit raycastHit in raycastHits) // Loop through all hits
            {
                if(projectionBuildingObjectHandler.terrainOnly) // If we only want to place on terrain
                {
                    if(raycastHit.collider.gameObject.GetComponent<Terrain>() == null) // Basicly, don't place unless if it hits a terrain
                    {
                        lastSavedProjectionPosition = Vector3.zero;
                        return lastSavedProjectionPosition;
                    }
                }
                if(raycastHit.distance < 5) // Skip everything too close to the camera
                {
                    continue;
                }
                SnapHitboxHandler snapHitbox = raycastHit.collider.GetComponent<SnapHitboxHandler>() ?? null;
                if (snapHitbox != null && snapping) // Try to snapp to anything, if the object can snapp
                {
                    int snapperIndex = 0;
                    foreach(BuildingObject.BuildingType permittedSnapper in snapHitbox.snapBuildingTypes) // Loop through all permitted snappers
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
                if(!closestPointWithoutSnappingFound && snapHitbox == null) // If we haven't found a closest point without snapping yet, and the object can't snap
                {
                    if(projectionBuildingObjectHandler.grounded) // If the object needs to be grounded
                    {
                        if(Vector3.Dot(raycastHit.normal, Vector3.up) > 0.9f)
                        {
                            projectionBuildingObjectHandler.ChangePivot(projectionBuildingObjectHandler.groundedPivotIndex);
                            lastSavedProjectionPosition = raycastHit.point + new Vector3(0, heightOffset, 0);
                            closestPointWithoutSnappingFound = true;
                        }
                        if(projectionBuildingObjectHandler.gigaGrounded) // If the object needs to be gigaGrounded
                        {
                            projectionBuildingObjectHandler.ChangePivot(projectionBuildingObjectHandler.groundedPivotIndex);
                            groundedAlignment = Quaternion.LookRotation(CalculateAverageNormal(raycastHits)).eulerAngles;
                            lastSavedProjectionPosition = raycastHit.point + new Vector3(0, heightOffset, 0);
                            closestPointWithoutSnappingFound = true;
                        }
                    }
                    else // If the object doesn't need to be grounded
                    {
                        projectionBuildingObjectHandler.ChangePivot(0);
                        lastSavedProjectionPosition = raycastHit.point + new Vector3(0, heightOffset, 0);
                        closestPointWithoutSnappingFound = true;
                    }

                }
                else
                {
                    continue;
                }
            }
        }
        else
        {
            lastSavedProjectionPosition = Vector3.zero;
        }
        return lastSavedProjectionPosition;
    }

    Vector3 CalculateAverageNormal(RaycastHit[] hits)
    {
        Vector3 sumNormals = Vector3.zero;

        // Sum up all hit normals
        foreach (var hit in hits)
        {
            sumNormals += hit.normal;
        }

        // Calculate the average normal
        Vector3 averageNormal = sumNormals.normalized;

        return averageNormal;
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

    public virtual void SetBuildingDescription(GameObject source)
    {
        BuildingSlotUI slot = source.GetComponent<BuildingSlotUI>() ?? null;
        if(slot != null)
        {
            selectedBuildingText.text = "Selected Building: " + slot.buildingData.buildingName;
            foreach (ItemSlotUI itemSlot in buldingCostSlots)
            {
                itemSlot.ItemImage.sprite = null;
                itemSlot.StackSizeText.text = "";
            }
            for (int i = 0; i < slot.buildingData.buildingMaterials.Length;i++)
            {
                Item buildingItem = slot.buildingData.buildingMaterials[i];
                buldingCostSlots[i].ItemImage.sprite = buildingItem.sprite;
                buldingCostSlots[i].StackSizeText.text = $"x {slot.buildingData.buildingMaterialAmounts[i]}";
                // Colorize it depending if resources are available or not
                if (slot.buildingData.buildingMaterialAmounts[i] <= playerInventory.GetAmmountOfItem(buildingItem.itemId))
                {
                    buldingCostSlots[i].StackSizeText.color = Color.white;
                }
                else
                {
                    buldingCostSlots[i].StackSizeText.color = Color.red;
                }
            }
        }
    }

    public virtual void RefreshBuildingDescription()
    {
        BuildingObject currentBuilding = BuildingObjectManager.AccessStaticBuildingObjectData(currentBuildingId);
        selectedBuildingText.text = "Selected Building: " + currentBuilding.buildingName;
        foreach (ItemSlotUI itemSlot in buldingCostSlots)
        {
            itemSlot.ItemImage.sprite = null;
            itemSlot.StackSizeText.text = "";
        }
        for (int i = 0; i < currentBuilding.buildingMaterials.Length; i++)
        {
            Item buildingItem = currentBuilding.buildingMaterials[i];
            buldingCostSlots[i].ItemImage.sprite = buildingItem.sprite;
            buldingCostSlots[i].StackSizeText.text = $"x {currentBuilding.buildingMaterialAmounts[i]}";
            // Colorize it depending if resources are available or not
            if (currentBuilding.buildingMaterialAmounts[i] <= playerInventory.GetAmmountOfItem(buildingItem.itemId))
            {
                buldingCostSlots[i].StackSizeText.color = Color.white;
            }
            else
            {
                buldingCostSlots[i].StackSizeText.color = Color.red;
            }
        }
    }

    public void HideBuildingDescription()
    {
        selectedBuildingText.text = "Selected Building:";
        foreach (ItemSlotUI itemSlot in buldingCostSlots)
        {
            itemSlot.ItemImage.sprite = null;
            itemSlot.StackSizeText.text = "";
        }
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






