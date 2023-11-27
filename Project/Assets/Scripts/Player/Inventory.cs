using MalbersAnimations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using static Item;

public class Inventory : NetworkBehaviour
{

    public Item[] items = new Item[(maxItemSlots + toolSlots)];
    [SerializeField] private const int toolSlots = 4;
    [SerializeField] private const int maxItemSlots = 40;
    public List<ulong> observerList = new List<ulong>();
    public UnityEvent<Item> addItemEvents;
    [HideInInspector]public NetworkVariable<bool> ableToTrade = new NetworkVariable<bool>(true,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    private void Start()
    {
        if (!IsOwner)
            return;
        List<ItemData> itemData = new List<ItemData>();
        foreach (Item item in items)
        {
            ItemData data = ItemData.Null;
            if (item != null)
            {
                data = new ItemData() { itemId = item.itemId, stackSize = item.stackSize };
                itemData.Add(data);
            }
            print(Marshal.SizeOf(data));
            

        }
    }

    [ClientRpc]
    public void AddItemClientRPC(ulong id, int stackSize, ClientRpcParams clientRpcParams = default)
    {
        AddItem(id, stackSize);
    }

    [ClientRpc]
    public void RemoveItemClientRPC(ulong id, int stackSize,int pos, ClientRpcParams clientRpcParams = default)
    {
        RemoveItem(id, stackSize,pos);
    }

    [ClientRpc]
    public void ReplaceItemClientRpc(ulong id,int stackSize,int pos, ClientRpcParams clientRpcParams = default)
    {
        Item item = null;
        if (id != 0)
        {
            item = ItemManager.GenerateItem(id);
            item.stackSize = stackSize;
        }
        items[pos] = item;
       // InventoryManagerUI.Instance.RefreshUI();
    }

    private bool AddItem(ulong id,int stackSize)
    {
        Item item = ItemManager.GenerateItem(id);
        item.stackSize = stackSize;
        if(item != null)
        {
            Item itemToStackOn;
            if (ItemAlreadyInventoryAndHasSpaceOnStack(id, out itemToStackOn))
            {
                itemToStackOn.stackSize += item.stackSize;
                if(itemToStackOn.stackSize > itemToStackOn.maxStackSize)
                {
                    int rest = itemToStackOn.stackSize - itemToStackOn.maxStackSize;
                    itemToStackOn.stackSize = itemToStackOn.maxStackSize;
                    AddItem(id, rest);
                }
                addItemEvents.Invoke(item);
                NotificationManagerUI.Instance.SetNotification(item);
                //InventoryManagerUI.Instance.RefreshUI();
                return true;
            }
            // find space for added Item
            bool spaceFound = false;
            for(int i = 0; i < maxItemSlots ;i++)
            {
                if(items[i] == null)
                {
                    items[i] = item;
                    spaceFound = true;
                    break;
                }
            }
            addItemEvents.Invoke(item);
            NotificationManagerUI.Instance.SetNotification(item);
            //InventoryManagerUI.Instance.RefreshUI();
            return spaceFound;
        }
        else
        {
            return false;
        }
    }

    public void RemoveItem(ulong id, int amount)
    {
        for(int slot = 0; slot < maxItemSlots;slot++)
        {
            if (items[slot] != null)
            {
                print(items[slot].itemId);
                if (items[slot].itemId == id)
                {
                    if (amount > items[slot].stackSize)
                    {
                        amount -= items[slot].stackSize;
                        items[slot] = null;
                    }
                    else if (amount < items[slot].stackSize)
                    {
                        items[slot].stackSize -= amount;
                        amount = 0;
                    }
                    else if (amount == items[slot].stackSize)
                    {
                        items[slot] = null;
                    }
                }
            }
        }
        //InventoryManagerUI.Instance.RefreshUI();
    }

    public void RemoveItem(ulong id, int amount, int pos)
    {
        if (items[pos] != null)
        {
            if (items[pos].itemId == id)
            {
                if (amount > items[pos].stackSize)
                {
                    amount -= items[pos].stackSize;
                    items[pos] = null;
                }
                else if (amount < items[pos].stackSize)
                {
                    items[pos].stackSize -= amount;
                    amount = 0;
                }
                else if (amount == items[pos].stackSize)
                {
                    items[pos] = null;
                }
            }
        }
    }


    private bool ItemAlreadyInventoryAndHasSpaceOnStack(ulong id, out Item itemToStackOn)
    {
        foreach(Item item in items)
        {
            if(CheckItemId(item,id))
            {
                // there is no space left, so it is considered not in inventory for the purpose of stacking
                if(item.stackSize == item.maxStackSize)
                {
                    continue;
                }
                else
                {
                    itemToStackOn = item;
                    return true;
                }
            }
        }
        itemToStackOn = null;
        return false;
    }

    public bool HasItemSpaceInInventory(ulong id, int itemAmount)
    {
        Item item = ItemManager.GenerateItem(id);
        item.stackSize = itemAmount;
        Item itemToStackOn;
        if (ItemAlreadyInventoryAndHasSpaceOnStack(id, out itemToStackOn))
        {
            itemToStackOn.stackSize += item.stackSize;
            if (itemToStackOn.stackSize > itemToStackOn.maxStackSize)
            {
                int rest = itemToStackOn.stackSize - itemToStackOn.maxStackSize;
                itemToStackOn.stackSize = itemToStackOn.maxStackSize;
                AddItem(id, rest);
            }
            //InventoryManagerUI.Instance.RefreshUI();
            return true;
        }
        // find space for added Item
        bool spaceFound = false;
        for (int i = 0; i < maxItemSlots; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                spaceFound = true;
                break;
            }
        }
        return spaceFound;
    }

    private bool CheckItemId(Item item,ulong idToCheck)
    {
        if(item == null || item.itemId != idToCheck)
        {
            return false;
        }
        return true;
    }

    private bool CheckItemName(Item item, string nameToCheck)
    {
        if (item == null || item.itemName != nameToCheck)
        {
            return false;
        }
        return true;
    }

    public bool SwapItemPlaces(int item1Pos,int item2Pos)
    {
        bool item1Present =  items[item1Pos] != null;
        bool item2Present = items[item2Pos] != null;

        if (item1Present || item2Present)
        {
            if (item1Present && item2Present)
            {
                Item item1 = items[item1Pos];
                Item item2 = items[item2Pos];
                items[item1Pos] = null;
                items[item2Pos] = null;
                items[item2Pos] = item1;
                items[item1Pos] = item2;
            }
            else
            if(item1Present && !item2Present)
            {
                Item item1 = items[item1Pos];
                items[item1Pos] = null;
                items[item2Pos] = item1;
            }
            else
            if (!item1Present && item2Present)
            {
                Item item2 = items[item2Pos];
                items[item2Pos] = null;
                items[item1Pos] = item2;
            }
            if ((item2Present && items[item1Pos].itemType == Item.ItemType.Equipment) || (item1Present && items[item2Pos].itemType == Item.ItemType.Equipment))
            {
                int[] itemPos = new int[2] { item1Pos, item2Pos };
                foreach(int i in itemPos)
                {
                    // Equipment should be Equiped
                    if(i >= 40)
                    {
                        //GetComponent<MWeaponManager>()
                        // Instantiate(items[i].droppedObject)
                    }
                    else // Eqipment should be Unequiped
                    {

                    }

                }
            }
            //InventoryManagerUI.Instance.RefreshUI();
            return true;
        }

        return false;
    }

    [ServerRpc (RequireOwnership =false)]
    public void SwapItemsBetweenInveotryServerRpc(NetworkBehaviourReference inventory1, NetworkBehaviourReference inventory2,ItemData item1,int pos1,ItemData item2,int pos2)
    {
        Inventory inv1;
        Inventory inv2;
        if (inventory1.TryGet(out inv1) && inventory2.TryGet(out inv2)) // Both Inventory References are valid / not null
        {
            ClientRpcParams inv1ClientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { inv1.OwnerClientId }
                }
            };
            ClientRpcParams inv2ClientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { inv2.OwnerClientId }
                }
            };

            //if (!inv1.ableToTrade.Value && !inv2.ableToTrade.Value)
            //{
            print($"{inv1.gameObject}, {inv2.gameObject}, {pos1}, {inv1ClientRpcParams}");
                inv1.ReplaceItemClientRpc(item2.itemId, item2.stackSize, pos1, inv1ClientRpcParams);
                inv2.ReplaceItemClientRpc(item1.itemId, item1.stackSize, pos2, inv2ClientRpcParams);
                inv1.RequestItemSyncClientRpc(inv1ClientRpcParams);
                inv2.RequestItemSyncClientRpc(inv2ClientRpcParams);
                inv1.ableToTrade.Value = true;
                inv2.ableToTrade.Value = true;
            //}
        }
    }

    [ClientRpc]
    public void RequestItemSyncClientRpc(ClientRpcParams clientRpcParams = default)
    {
        List<ItemData> itemData = new List<ItemData>();
        foreach(Item item in items)
        {
            if(item != null)
            {
                itemData.Add(new ItemData() { itemId = item.itemId, stackSize = item.stackSize });
            }
            else
            {
                itemData.Add(ItemData.Null);
            }

        }
        SyncItemsAcrossNetworkClientRpc(itemData.ToArray(), GameManager.GetClientRpcParams(observerList.ToArray()));
    }

    [ClientRpc]
    public void SyncItemsAcrossNetworkClientRpc(ItemData[] itemData, ClientRpcParams clientRpcParams = default)
    {
        for(int i = 0; i < itemData.Length;i++)
        {
            Item item = ItemData.ReadItemData(itemData[i]);
            items[i] = item;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddToObserverListServerRpc(ulong id)
    {
        if (!observerList.Contains(id))
        {
            observerList.Add(id);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveFromObserverListServerRpc(ulong id)
    {
        if (observerList.Contains(id))
        {
            observerList.Remove(id);
        }
    }

    public static void SwapItemsBetweenInventory(Inventory inventory1, int item1Pos, Inventory inventory2, int item2Pos)
    {
        if(inventory1.ableToTrade.Value && inventory2.ableToTrade.Value)
        {
            ItemData itemData1 = new ItemData(inventory1.items[item1Pos].itemId, inventory1.items[item1Pos].stackSize);
            ItemData itemData2 = new ItemData(inventory2.items[item2Pos].itemId, inventory1.items[item2Pos].stackSize);
            inventory1.SwapItemsBetweenInveotryServerRpc(inventory1, inventory2, itemData1, item1Pos, itemData2, item2Pos);
        }
    }

    public int GetAmmountOfItem(ulong id)
    {
        int amount = 0;
        foreach(Item item in items)
        {
            if(CheckItemId(item,id))
            {
                amount += item.stackSize;
            }
        }
        return amount;
    }

    public int GetAmmountOfItem(string itemName)
    {
        int amount = 0;
        foreach (Item item in items)
        {
            if (CheckItemName(item,itemName))
            {
                amount += item.stackSize;
            }
        }
        return amount;
    }
}
