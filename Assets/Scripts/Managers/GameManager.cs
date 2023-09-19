using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class GameManager : NetworkBehaviour
{
    private static GameManager instance;
    public GameObject playerPrefab;
    public NetworkVariable<int> woodCounter = new NetworkVariable<int>(0);
    public GameUI gameUI;

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

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId)
    {
        GameObject go = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();
        go.GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }

    public static GameManager Instance { get => instance; set => instance = value; }

}
