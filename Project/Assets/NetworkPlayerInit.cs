using MalbersAnimations;
using MalbersAnimations.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerInit : NetworkBehaviour
{
    public MonoBehaviour[] componentsToDisable;
    public GameObject[] objectsToDelete;
    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;
    public GameObject playerSetupPrefab;

    public TextMeshProUGUI playerNameCard;

    [HideInInspector]public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>("T", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += SetPlayerNameCard;
    }

    protected virtual void SetPlayerNameCard(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        playerNameCard.text = newValue.ToString();
    }

    [ServerRpc (RequireOwnership = false)]
    public void SetNameCardServerRpc(string name)
    {
        playerName.Value = name;
    }

    [ClientRpc]
    public void SetNameCardClientRpc(ulong id,string name)
    {
        playerNameCard.text = name;
    }

    /// <summary>
    /// Updates the player name, usualy only set On Spawn
    /// </summary>
    /// <param name="previousValue"></param>
    /// <param name="newValue"></param>
    protected virtual void UpdatePlayerName(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
            playerNameCard.text = newValue.ConvertToString();
    }


    // Start is called before the first frame update
    void Start()
    {
        if(!IsOwner)
        {
            // Is other Client
            foreach(MonoBehaviour component in componentsToDisable)
            {
                component.enabled = false;
            }

            foreach(GameObject go in objectsToDelete)
            {
                Destroy(go);
            }
            playerNameCard.text = playerName.Value.ToString();
        }
        else
        {
            // Is Local Client
            foreach (GameObject go in objectsToActivate)
            {
                go.SetActive(true);
            }
            foreach (GameObject go in objectsToDeactivate)
            {
                go.SetActive(false);
            }
            GameManager.Instance.inputManager = GetComponent<MInput>();
            GameObject playerSetup = Instantiate(playerSetupPrefab);
            DontDestroyOnLoad(playerSetup);
            playerSetup.GetComponentInChildren<InventoryManagerUI>().Inventory = GetComponent<Inventory>();
            Collider col = GetComponent<Collider>();
            if(col.isTrigger)
            {
                col.isTrigger = false;
                print("-------- Had to Fix isTrigger of Character Spawn-------");
            }
            SetNameCardServerRpc(GameManager.Instance.playerName);
        }
    }

    private void Update()
    {

    }

    [ClientRpc]
    public void TeleportClientRpc(Vector3 postion, ClientRpcParams clientRpcParams = default)
    {
        transform.position = postion;
    }

    [ClientRpc]
    public void CallLoadingScreenClientRpc(ClientRpcParams clientRpcParams = default)
    {
        LoadingScreenManager.Instance.CallLoadingScreen();
    }
    [ClientRpc]
    public void DismissLoadingScreenClientRpc(ClientRpcParams clientRpcParams = default)
    {
        LoadingScreenManager.Instance.DismissLoadingScreen();
    }


}
