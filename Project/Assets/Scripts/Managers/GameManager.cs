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
    public void PlacePrefabServerRpc(ulong buildingId, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = BuildingObjectManager.GenerateBuildingObject(buildingId).buildingPrefab;
        GameObject spawnedPrefab = Instantiate(prefab, position, rotation);
        spawnedPrefab.GetComponent<NetworkObject>().Spawn(true);
        PlacedObjectData data = new PlacedObjectData()
        {
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
