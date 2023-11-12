using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public float gridSize = 0.25f;
    public float[] gridSizes = new float[] { 0.25f, 0.5f, 1, 2 };
    private int gridSizeIndex = 0;
    public float raycastMaxDistance = 7;
    public LayerMask layerMask;
    public LayerMask layerMaskDelete;
    public ProjectionHandler projectionInstance;
    private Transform cameraMainTransform;
    private bool canUpdateProjectionPosition = false;
    private float sphereCastRadius = 0.1f;
    private static BuildManager instance;
    public ulong currentBuildingId;

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
        if(CanUpdateProjectionPosition)
        {
            UpdateProjectionPosition();
        }
    }

    private void Update()
    {
        if(Mathf.Abs(InputManager.Instance.CameraZoomDelta) > 0.1f)
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

    public virtual void CallProjection(GameObject prefab)
    {
        DismissProjection();
        projectionInstance.SpawnProjection(prefab);
        UpdateProjectionPosition();
    }

    public virtual void CallProjection(int id)
    {
        DismissProjection();
        projectionInstance.SpawnProjection(BuildingObjectManager.GenerateBuildingObject((ulong)id).buildingPrefab);
        currentBuildingId = (ulong)id;
        UpdateProjectionPosition();
    }

    public virtual void PlaceBuildingObject()
    {
        GameManager.Instance.PlaceBuildingServerRpc(currentBuildingId, projectionInstance.transform.position, projectionInstance.transform.rotation);
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
        GameManager.Instance.PlaceBuildingServerRpc((ulong)id, projectionInstance.transform.position, projectionInstance.transform.rotation);
    }

    public void UpdateProjectionPosition()
    {

        projectionInstance.transform.position = RoundVector(GetRaycastPosition(cameraMainTransform.position, cameraMainTransform.forward),gridSize);
    }

    public void DismissProjection()
    {
        projectionInstance.DismissProjection();
    }
    
    public Vector3 GetRaycastPosition(Vector3 startPoint, Vector3 direction)
    {
        RaycastHit raycastHit;
        if(Physics.SphereCast(startPoint,sphereCastRadius, direction, out raycastHit, raycastMaxDistance, layerMask))
        {
            Vector3 pivotOffset = Vector3.zero;
            // Snapping but is disabled for now
            /*
            BuildingObjectHandler boh = raycastHit.collider.transform.root.GetComponent<BuildingObjectHandler>() ?? null;
            if(boh != null)
            {
                return boh.GetClosestSnappingPoint(raycastHit.point) - pivotOffset;
            }
            */



            return raycastHit.point - pivotOffset;
        }
        else
        {
            return new Ray(startPoint, direction).GetPoint(raycastMaxDistance);
        }
    }

    // Custom method to round Vector3 components to a specified precision
    Vector3 RoundVector(Vector3 vector, float precision)
    {
        float x = RoundToPrecision(vector.x, precision);
        float y = RoundToPrecision(vector.y, precision);
        float z = RoundToPrecision(vector.z, precision);

        return new Vector3(x, y, z);
    }

    float RoundToPrecision(float value, float precision)
    {
        return Mathf.Round(value / precision) * precision;
    }
}
