using MalbersAnimations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;


public class GameManager : NetworkBehaviour
{
    private static GameManager instance;
    public GameUI gameUI;
    public MInput inputManager;
    public NetworkPrefabsList networkPrefabsList;
    public Gradient myColor;
    public Gradient otherPlayerColor;
    public string playerName;
    public Dictionary<ulong, GameObject> playerList = new Dictionary<ulong, GameObject>();
    public string relayCode = "";
    public WorldSaveState worldSaveState;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RegisterNetworkPrefabs();
    }

    [ServerRpc (RequireOwnership = false)]
    public void PlaceBuildingServerRpc(ulong buildingId, Vector3 position, Quaternion rotation)
    {
        BuildingObject buildingObject = BuildingObjectManager.GenerateBuildingObject(buildingId);
        GameObject prefab = buildingObject.buildingPrefab;
        GameObject spawnedPrefab = Instantiate(prefab, position, rotation);
        spawnedPrefab.GetComponent<NetworkObject>().Spawn(true);
        PlacedObjectData data = new PlacedObjectData()
        {
            buildingObject = buildingObject,
            prefab = prefab,
            position = position,
            rotation = rotation
        };
        if(spawnedPrefab.transform.root.GetComponent<BuildingObjectHandler>() != null)
        {
            spawnedPrefab.transform.root.GetComponent<BuildingObjectHandler>().data = data;
        }

        worldSaveState.AddPlacedObject(data);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnBuildingServerRpc(ulong networkId,ulong clientThatDespawned)
    {
        NetworkObject building = NetworkManager.SpawnManager.SpawnedObjects[networkId];
        BuildingObjectHandler boh = building.GetComponent<BuildingObjectHandler>() ?? null;
        if(boh != null)
        { 
            for(int i = 0; i < boh.data.buildingObject.buildingMaterials.Length;i++)
            {
                GameObject droppedItem = Instantiate(boh.data.buildingObject.buildingMaterials[i].droppedObject, building.transform.position, Quaternion.identity);
                droppedItem.GetComponentInChildren<Interactable_DroppedItem>().itemData.stackSize = boh.data.buildingObject.buildingMaterialAmounts[i];
                droppedItem.GetComponent<NetworkObject>().Spawn(true);
            }

        }
        building.Despawn(true);
        
    }

    private void RegisterNetworkPrefabs()
    {
        var prefabs = networkPrefabsList.PrefabList.Select(x => x.Prefab);
        foreach (var prefab in prefabs)
        {
            NetworkManager.Singleton.AddNetworkPrefab(prefab);
        }
    }


    public static GameManager Instance { get => instance; set => instance = value; }

}
