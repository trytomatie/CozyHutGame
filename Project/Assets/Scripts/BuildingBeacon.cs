using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BuildingBeacon : MonoBehaviour
{
    public Vector3 beaconSize = new Vector3(50, 50, 50);

    private void Start()
    {
        if(GetComponent<NetworkObject>().IsSpawned)
        {
            GameManager.Instance.buildingBeacons.Add(this);
        }

    }

    private void OnDestroy()
    {
        if(GameManager.Instance.buildingBeacons.Contains(this))
        {
            GameManager.Instance.buildingBeacons.Remove(this);
        }

    }
}
