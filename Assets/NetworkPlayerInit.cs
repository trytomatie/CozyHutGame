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

    public NetworkVariable<FixedString64Bytes> playerName;
    public TextMeshProUGUI playerNameCard;

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += UpdatePlayerName;
    }

    /// <summary>
    /// Updates the player name, usualy only set On Spawn
    /// </summary>
    /// <param name="previousValue"></param>
    /// <param name="newValue"></param>
    protected virtual void UpdatePlayerName(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        if (!newValue.Equals(previousValue))
        {
            playerNameCard.text = newValue.ConvertToString();
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        if(!IsOwner)
        {
            foreach(MonoBehaviour component in componentsToDisable)
            {
                component.enabled = false;
            }

            foreach(GameObject go in objectsToDelete)
            {
                Destroy(go);
            }
        }
        else
        {
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
