using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public GameObject[] resources;
    public static ResourceManager instance;
    public void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void CheckIfObjectIsSpawned()
    {
        foreach(GameObject go in resources)
        {
            if(go.GetComponent<NetworkObject>().IsSpawned)
            {
                go.SetActive(true);
            }
        }
    }

}
