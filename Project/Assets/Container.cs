using MalbersAnimations.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using static Item;

public class Container : NetworkBehaviour
{
    public ItemData[] items;
    public UnityEvent<Item> addItemEvents;
    public List<ulong> observerList = new List<ulong>() {};
    public MEvent observationEvent;
    public bool cursorObserver = false;
    public bool canNotify = false;

    private void Start()
    {
        if(IsServer && !cursorObserver)
        {
            AddToObserverListServerRpc(OwnerClientId);
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void RequestItemSwapServerRpc(NetworkBehaviourReference otherContainerRef,int pos1, int pos2,ItemData validationData1,ItemData validationData2)
    {
        Container otherContainer;
        //print($"{pos1} {pos2} {validationData1.itemId} {validationData2.itemId} {validationData1.stackSize} {validationData2.stackSize} {gameObject.name}");
        if (otherContainerRef.TryGet(out otherContainer)) // Both Inventory References are valid / not null
        {
            if (ValidateDataSwap(otherContainer, pos1, pos2, validationData1, validationData2)) // The Item Positions are also Valid
            {
                // Swap Items here
                // Replicate this Operation on all Observers

                ClientRpcParams clientRpcParams = GameManager.GetClientRpcParams(observerList.ToArray());
                SwapItemsClientRpc(pos1, pos2, validationData1, validationData2, otherContainer, clientRpcParams);

                // Just gonna sync stuff anyway, 
                SyncContainerClientRpc(items, GameManager.GetClientRpcParams(observerList.ToArray()));
            }
            else
            {
                Debug.LogError("Inventory Desync, syncing Inventory Again");
                SyncContainerClientRpc(items, GameManager.GetClientRpcParams(observerList.ToArray()));
            }
        }
        else
        {
            Debug.LogError("Other Container not Valid / is null");
        }
    }
    [ClientRpc]
    private void SwapItemsClientRpc(int pos1, int pos2, ItemData validationData1, ItemData validationData2, NetworkBehaviourReference otherContainerRef, ClientRpcParams clientRpcParams = default)
    {
        Container otherContainer;
        if (otherContainerRef.TryGet(out otherContainer)) // Both Inventory References are valid / not null
        {
            otherContainer.items[pos2] = validationData1;
            items[pos1] = validationData2;
            observationEvent.Invoke(gameObject);
            otherContainer.observationEvent.Invoke(otherContainer.gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestRemoveItemServerRpc(ItemData itemData)
    {
        int amount = itemData.stackSize;
        for (int i = items.Length - 1; i >= 0; i--)
        {
            if (itemData.itemId == items[i].itemId)
            {
                if (items[i].stackSize >= amount)
                {
                    print($"Removing {itemData.itemId} {itemData.stackSize} {i}");
                    // If stackSize is greater than or equal to amount, subtract amount
                    RemoveItemWithValidation(new ItemData(items[i].itemId, amount),i);
                    amount = 0;  
                    break;
                }
                else
                {
                    // If stackSize is smaller, subtract stackSize and adjust amount
                    amount -= items[i].stackSize;
                    RemoveItemWithValidation(new ItemData(items[i].itemId, items[i].stackSize), i);
                }
            }
        }
        SyncContainerClientRpc(items, GameManager.GetClientRpcParams(observerList.ToArray()));
    }

    [ServerRpc (RequireOwnership =false)]
    public void RequestRemoveItemServerRpc(ItemData itemData,int pos)
    {
        RemoveItemWithValidation(itemData, pos);
    }

    private void RemoveItemWithValidation(ItemData itemData, int pos)
    {
        // No Validation Needed
        RemoveItemClientRpc(itemData, pos);
    }

    /// <summary>
    /// To Avoid double Crafts/Builds, since cost is calculated Clientside
    /// </summary>
    public void RemoveItemClientSidePrediction(ItemData itemData)
    {
        int amount = itemData.stackSize;
        for (int i = items.Length - 1; i >= 0; i--)
        {
            if (itemData.itemId == items[i].itemId)
            {
                if (items[i].stackSize >= amount)
                {
                    // If stackSize is greater than or equal to amount, subtract amount
                    RemoveItem(new ItemData(items[i].itemId, amount), i);
                    amount = 0;
                    break;
                }
                else
                {
                    // If stackSize is smaller, subtract stackSize and adjust amount
                    amount -= items[i].stackSize;
                    RemoveItem(new ItemData(items[i].itemId, items[i].stackSize), i);
                }
            }
        }
    }

    [ClientRpc]
    private void RemoveItemClientRpc(ItemData data, int pos)
    {
        // if (IsHost) return;
        if (items[pos].stackSize >= data.stackSize) // if The stacksize is sufficent
        {
            items[pos].stackSize -= data.stackSize;
            if(items[pos].stackSize == 0)
            {
                items[pos] = ItemData.Null;
            }
            observationEvent.Invoke(gameObject);
        }
        else // data missmatch must have happend / Invalid Request
        {
            Debug.LogError($"Data Missmatch happened Client: {NetworkManager.LocalClientId} for Removing Items");
            SyncContainerServerRpc(NetworkManager.LocalClientId);
        }
    }

    public void RemoveItem(ItemData data,int pos)
    {
        if (items[pos].stackSize >= data.stackSize) // if The stacksize is sufficent
        {
            items[pos].stackSize -= data.stackSize;
            if (items[pos].stackSize == 0)
            {
                items[pos] = ItemData.Null;
            }
            SyncContainerServerRpc(NetworkManager.LocalClientId);

        }
        else // data missmatch must have happend / Invalid Request
        {
            Debug.LogError($"Data Missmatch happened Client: {NetworkManager.LocalClientId} for Removing Items");
            SyncContainerServerRpc(NetworkManager.LocalClientId);
        }
    }

    public void AddItemToPos(ItemData data, int pos)
    {

        if (items[pos].stackSize + data.stackSize <= items[pos].MaxStackSize) // if The stacksize is sufficent
        {
            items[pos].stackSize += data.stackSize;
            SyncContainerServerRpc(NetworkManager.LocalClientId);
        }
        else // data missmatch must have happend / Invalid Request
        {
            Debug.LogError($"Data Missmatch happened Client: {NetworkManager.LocalClientId} for Removing Items");
            SyncContainerServerRpc(NetworkManager.LocalClientId);
        }
    }


    private bool ValidateDataSwap(Container otherContainer, int pos1,int pos2,ItemData validationData1,ItemData validationData2)
    {
        // Just checks if the item on that position has the same id and stacksize (Very rudementery I know! But shouldnt lead to any bugs (Right?))
        if(items[pos1] == validationData1 && otherContainer.items[pos2] == validationData2) 
        {
            return true;
        }
        return false;
    }

    private static bool ValidateData(Container container,int pos,ItemData validationData)
    {
        if (container.items[pos] == validationData)
        {
            return true;
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddToObserverListServerRpc(ulong id)
    {
        if (!observerList.Contains(id))
        {
            SyncContainerServerRpc(id);
            observerList.Add(id);
            UpdateObserverListClientRpc(observerList.ToArray(),GameManager.GetClientRpcParams(observerList.ToArray()));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveFromObserverListServerRpc(ulong id)
    {
        if (observerList.Contains(id))
        {
            ClientRpcParams senderList = GameManager.GetClientRpcParams(observerList.ToArray()); // we also need to notify the removed client that he is removed
            observerList.Remove(id);
            UpdateObserverListClientRpc(observerList.ToArray(), senderList);
        }
    }

    [ClientRpc]
    public void UpdateObserverListClientRpc(ulong[] ids, ClientRpcParams clientRpcParams = default)
    {
        observerList = ids.ToList();
        if (!cursorObserver)
        {
            return;
        }
        UpdateObserverCursors();
    }

    private void UpdateObserverCursors()
    {
        bool localClientIsObsvering = false;
        foreach (ulong id in observerList)
        {
            if (id == NetworkManager.LocalClientId)
            {
                localClientIsObsvering = true;
                break;
            }
        }
        foreach (ulong id in GameManager.Instance.playerList.Keys)
        {
            GameManager.Instance.playerList[id].GetComponent<NetworkPlayerInit>().observerCursor.SetVisible(false);
        }
        if (localClientIsObsvering)
        {
            foreach (ulong id in observerList)
            {
                GameManager.Instance.playerList[id].GetComponent<NetworkPlayerInit>().observerCursor.SetVisible(true);
            }
        }
    }

    [ServerRpc]
    public void SyncContainerServerRpc(ulong clientId)
    {
        SyncContainerClientRpc(items, GameManager.GetClientRpcParams(clientId));
    }

    [ClientRpc]
    public void SyncContainerClientRpc(ItemData[] serverItems, ClientRpcParams clientRpcParams = default)
    {
        items = serverItems;
        observationEvent.Invoke(gameObject);
    }

    [ServerRpc (RequireOwnership =false)]
    public void AddItemServerRpc(ItemData item)
    {
        ClientRpcParams clientRpcParams = GameManager.GetClientRpcParams(observerList.ToArray());
        AddItem(item);
    }

    private void AddItem(ItemData itemData)
    {
        Item item = ItemManager.GenerateItem(itemData);
        if (item != null)
        {
            int itemToStackOnPos;
            if (ItemAlreadyInventoryAndHasSpaceOnStack(itemData.itemId, out itemToStackOnPos))
            {
                items[itemToStackOnPos].stackSize += item.stackSize;
                if (items[itemToStackOnPos].stackSize > items[itemToStackOnPos].MaxStackSize)
                {
                    int rest = items[itemToStackOnPos].stackSize - items[itemToStackOnPos].MaxStackSize;
                    items[itemToStackOnPos].stackSize = items[itemToStackOnPos].MaxStackSize;
                    ItemData restData = new ItemData(itemData.itemId, rest);
                    AddItem(restData);
                }
                addItemEvents.Invoke(item);
                if(NetworkManager.LocalClientId == OwnerClientId && canNotify)
                {
                    NotificationManagerUI.Instance.SetNotification(item);
                }
                observationEvent.Invoke(gameObject);
                return;
            }
            // find space for added Item
            bool spaceFound = false;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == ItemData.Null)
                {

                    items[i] = itemData;
                    spaceFound = true;
                    break;
                }
            }
            if(!spaceFound)
            {
                Debug.LogError("No Space in Container");
            }
            addItemEvents.Invoke(item);
            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                NotificationManagerUI.Instance.SetNotification(item);
            }
            SyncContainerClientRpc(items, GameManager.GetClientRpcParams(observerList.ToArray()));
            return;
        }
        else
        {
            return;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestItemStackOntopOfServerRpc(NetworkBehaviourReference otherContainerRef, int pos1, int pos2, ItemData validationData1, ItemData validationData2)
    {
        Container otherContainer;
        if (otherContainerRef.TryGet(out otherContainer)) // Both Inventory References are valid / not null
        {
            if (ValidateDataSwap(otherContainer, pos1, pos2, validationData1, validationData2)) // The Item Positions are also Valid
            {
                int spaceAvailable = validationData2.MaxStackSize - validationData2.stackSize;
                if(spaceAvailable >= validationData1.stackSize)
                {
                    otherContainer.AddItemToPos(validationData1, pos2);
                    RemoveItem(validationData1, pos1);
                }
                else if(spaceAvailable != 0)
                {
                    ItemData toAdd = new ItemData(validationData1.itemId, spaceAvailable);
                    ItemData toRemove = new ItemData(validationData1.itemId, spaceAvailable);
                    otherContainer.AddItemToPos(toAdd, pos2);
                    RemoveItem(toRemove, pos1);
                }
                // Just gonna sync stuff anyway, 
                SyncContainerClientRpc(items, GameManager.GetClientRpcParams(observerList.ToArray()));
            }
            else
            {
                Debug.LogError("Inventory Desync, syncing Inventory Again");
                SyncContainerClientRpc(items, GameManager.GetClientRpcParams(observerList.ToArray()));
            }
        }
        else
        {
            Debug.LogError("Other Container not Valid / is null");
        }
    }

    private bool ItemAlreadyInventoryAndHasSpaceOnStack(ulong id, out int itemToStackOnId)
    {
        int i = 0;
        foreach (ItemData item in items)
        {
            if (CheckItemId(item.itemId, id))
            {

                // there is no space left, so it is considered not in inventory for the purpose of stacking
                if (item.stackSize == ItemManager.GetMaxStackSize(id))
                {
                    i++;
                    continue;
                }
                else
                {
                    itemToStackOnId = i;
                    return true;
                }
            }
            i++;
        }
        itemToStackOnId = -1;
        return false;
    }

    public bool HasItemSpaceInInventory(ItemData item)
    {
        int itemToStackOnPos;
        if (ItemAlreadyInventoryAndHasSpaceOnStack(item.itemId, out itemToStackOnPos))
        {
            items[itemToStackOnPos].stackSize += item.stackSize;
            if (items[itemToStackOnPos].stackSize > items[itemToStackOnPos].MaxStackSize)
            {
                int rest = items[itemToStackOnPos].stackSize - items[itemToStackOnPos].MaxStackSize;
                items[itemToStackOnPos].stackSize = items[itemToStackOnPos].MaxStackSize;
                ItemData restData = new ItemData(item.itemId, rest);
                return HasItemSpaceInInventory(restData); // Pretty sure there is gonna be a problem in the future?
            }
            return true;
        }
        // find space for added Item
        bool spaceFound = false;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == ItemData.Null)
            {
                spaceFound = true;
                break;
            }
        }
        return spaceFound;
    }

    public int FindItemFromBehind(ItemData item)
    {
        for(int i = items.Length-1;i >= 0;i--)
        {
            if(item.itemId == items[i].itemId)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetAmmountOfItem(ulong id)
    {
        int amount = 0;
        foreach (ItemData item in items)
        {
            if (CheckItemId(item.itemId, id))
            {
                amount += item.stackSize;
            }
        }
        return amount;
    }

    private bool CheckItemId(ulong id1, ulong id2)
    {
        if (id1 == id2)
        {
            return true;
        }
        return false;
    }
}
