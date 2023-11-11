using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public float gridSize = 0.25f;
    public float raycastMaxDistance = 7;
    public LayerMask layerMask;
    public ProjectionHandler projectionInstance;
    private Transform cameraMainTransform;
    private bool canUpdateProjectionPosition = false;

    public bool CanUpdateProjectionPosition { get => canUpdateProjectionPosition; set => canUpdateProjectionPosition = value; }

    // Start is called before the first frame update
    void Start()
    {
        cameraMainTransform = Camera.main.transform;
    }



    private void FixedUpdate()
    {
        if(CanUpdateProjectionPosition)
        {
            UpdateProjectionPosition();
        }
    }

    public virtual void CallProjection(GameObject prefab)
    {
        projectionInstance.SpawnProjection(prefab);
        UpdateProjectionPosition();
    }

    public virtual void PlaceBuildingObject(BuildingObject buildingObject)
    {
        GameManager.Instance.PlacePrefabServerRpc(buildingObject.buildingId, projectionInstance.transform.position, projectionInstance.transform.rotation);
    }

    public virtual void PlaceBuildingObject(int id)
    {
        GameManager.Instance.PlacePrefabServerRpc((ulong)id, projectionInstance.transform.position, projectionInstance.transform.rotation);
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
        if(Physics.Raycast(startPoint, direction, out raycastHit, raycastMaxDistance, layerMask))
        {
            return raycastHit.point;
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
