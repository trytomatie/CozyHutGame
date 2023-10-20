using MalbersAnimations;
using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class GameManager : NetworkBehaviour
{
    private static GameManager instance;
    public GameObject playerPrefab;
    public NetworkVariable<int> woodCounter = new NetworkVariable<int>(0);
    public GameUI gameUI;
    public Transform spawnPoint;
    public MInput inputManager;

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


    // Dead?
    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId)
    {
        GameObject go = Instantiate(playerPrefab);
        go.GetComponent<NetworkObject>().Spawn();
        go.GetComponent<NetworkObject>().ChangeOwnership(clientId);
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { go.GetComponent<NetworkObject>().OwnerClientId }
            }
        };
        go.GetComponent<NetworkPlayerInit>().TeleportClientRpc(FindObjectOfType<SpawnPlayerBootstrap>().transform.position, clientRpcParams);
    }


    public static GameManager Instance { get => instance; set => instance = value; }

}
