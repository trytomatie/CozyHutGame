using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectionHandler : MonoBehaviour
{
    public Material projectionMaterial;
    public Transform objectPosition;

    public void SpawnProjection(GameObject prefab)
    {
        gameObject.SetActive(true);
        GameObject prefabInstance = Instantiate(prefab, objectPosition);
        prefabInstance.transform.localPosition = Vector3.zero;
        prefabInstance.transform.localRotation = Quaternion.identity;
        prefabInstance.layer = LayerMask.NameToLayer("Ignore Raycast");
        if (prefabInstance != null)
        {
            Renderer[] renderers = prefabInstance.GetComponentsInChildren<Renderer>();
            Collider[] colliders = prefabInstance.GetComponentsInChildren<Collider>();
            foreach (Renderer renderer in renderers)
            {
                if(renderer != null)
                {
                    renderer.material = projectionMaterial;
                }
            }
            foreach (Collider collider in colliders)
            {
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }
    }

    public void DismissProjection()
    {
        gameObject.SetActive(false);
        for(int i = 0; i < objectPosition.childCount;i++)
        {
            Destroy(objectPosition.GetChild(i).gameObject);
        }
    }
}
