using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static Item;

public class Interactable_SubmitResources : Interactable
{
    public NetworkVariable<int> requestedWood = new NetworkVariable<int>(1000);
    public NetworkVariable<int> requestedStone = new NetworkVariable<int>(1000);

    public GameObject buildingPrefab;
    public GameObject constructionSite;
    public GameObject sign;
    
    public TextMeshProUGUI woodCount;
    public TextMeshProUGUI stoneCount;
    public override void FocusInteraction()
    {
        if (NotTimeoutedByServer())
        {

        }
        SetServerTimeoutTimer();

    }

    public override void OnNetworkSpawn()
    {
        requestedWood.OnValueChanged += RefreshUI;
        requestedStone.OnValueChanged += RefreshUI;
    }

    protected virtual void RefreshUI(int previousValue, int newValue)
    {
        if (requestedWood.Value == 0 && requestedStone.Value == 0)
        {
            CompletionEvent();
        }
        woodCount.text = "Wood: " + requestedWood.Value;
        stoneCount.text = "Stone " + requestedStone.Value;

    }

    private void CompletionEvent()
    {
        sign.SetActive(false);
        Instantiate(buildingPrefab, constructionSite.transform.position, constructionSite.transform.rotation);
        constructionSite.SetActive(false);
    }



    public override void LocalInteraction(GameObject source)
    {
        base.LocalInteraction(source);
    }

    public override void ServerInteraction(GameObject source)
    {
        Inventory inventory = source.GetComponent<Inventory>();
        ItemData woodItemData = new ItemData()
        {
            itemId = ItemManager.GetItemId("Wood"),
            itemAmount = inventory.GetAmmountOfItem("Wood")
        };
        ItemData stoneItemData = new ItemData()
        {
            itemId = ItemManager.GetItemId("Stone"),
            itemAmount = inventory.GetAmmountOfItem("Stone")
        };
        CheckIfResourcesCanBeDeductedServerRpc(woodItemData, stoneItemData, source.GetComponent<NetworkObject>().OwnerClientId);
    }

    [ServerRpc (RequireOwnership = false)]
    private void CheckIfResourcesCanBeDeductedServerRpc(ItemData woodData, ItemData stoneData,ulong senderId)
    {
        if(requestedWood.Value > 0)
        {
            int requestedWoodint = requestedWood.Value;
            requestedWoodint -= woodData.itemAmount;
            if (requestedWoodint < 0)
            {
                // Demand Less wood from Player
                woodData.itemAmount -= Mathf.Abs(requestedWoodint);
                requestedWoodint = 0;
            }
            requestedWood.Value = requestedWoodint;
        }

        if (requestedStone.Value > 0)
        {
            int requestedStoneint = requestedStone.Value;
            requestedStoneint -= stoneData.itemAmount;
            if (requestedStoneint < 0)
            {
                // Demand Less wood from Player
                stoneData.itemAmount -= Mathf.Abs(requestedStoneint);
                requestedStoneint = 0;
            }
            requestedStone.Value = requestedStoneint;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { senderId }
            }
        };

        if(woodData.itemAmount > 0 || stoneData.itemAmount > 0) 
            RemoveResourcesFromClientRpc(woodData, stoneData,senderId, clientRpcParams);

    }

    [ClientRpc]
    private void RemoveResourcesFromClientRpc(ItemData woodData, ItemData stoneData,ulong senderId, ClientRpcParams clientRpcAttribute = default)
    {
        print("Removing " + woodData.itemAmount);
        Inventory inventory = NetworkManager.LocalClient.PlayerObject.GetComponent<Inventory>();
        inventory.RemoveItem(woodData.itemId, woodData.itemAmount);
        inventory.RemoveItem(stoneData.itemId, stoneData.itemAmount);
    }
}
