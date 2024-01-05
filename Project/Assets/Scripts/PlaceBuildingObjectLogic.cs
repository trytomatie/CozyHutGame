using System.Collections;
using UnityEngine;


public class PlaceBuildingObjectLogic : MonoBehaviour
{
    public void PlaceObject(int resourceId)
    {
        float rndScale = Random.Range(1, 1.5f);
        transform.Rotate(Vector3.right, 90f, Space.Self);
        ResourceObjectData data = new ResourceObjectData()
        {
            resource_id = resourceId,
            positon = transform.position + new Vector3(0, 0.25f, 0),
            roation = transform.eulerAngles + new Vector3(0,Random.Range(0,360),0),
            scale = new Vector3(rndScale, rndScale, rndScale),
            hp = ResourceManager.instance.resources[resourceId].GetComponentInChildren<ResourceController>().maxHp,
            maxhp = ResourceManager.instance.resources[resourceId].GetComponentInChildren<ResourceController>().maxHp
        };
        GameManager.Instance.SpawnResource(data);
    }

    public void UnregisterPlacedObject(BuildingObjectHandler handler)
    {
        GameManager.Instance.worldSaveState.RemovePlacedObject(handler.data);

    }
}
