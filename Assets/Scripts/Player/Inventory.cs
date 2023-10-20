using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    [SerializeField] private int maxItemSlots = 40;

    private void Start()
    {
        if (!IsOwner)
            return;


    }

    [ClientRpc]
    public void AddItemClientRPC(int id, int stackSize, ClientRpcParams clientRpcParams = default)
    {
        AddItem(id, stackSize);
    }
    private bool AddItem(int id,int stackSize)
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
                InventoryManagerUI.Instance.RefreshUI();
                return true;
            }
            // find space for added Item
            bool spaceFound = false;
            for(int i = 0; i < 40-1;i++)
            {
                if(!items.ContainsKey(i))
                {
                    items.Add(i, item);
                    spaceFound = true;
                    break;
                }
            }
            InventoryManagerUI.Instance.RefreshUI();
            return spaceFound;
        }
        else
        {
            return false;
        }
    }

    private bool ItemAlreadyInventoryAndHasSpaceOnStack(int id, out Item itemToStackOn)
    {
        foreach(Item item in items.Values)
        {
            if(item.itemId == id)
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

    public bool SwapItemPlaces(int item1Pos,int item2Pos)
    {
        bool item1Present =  items.ContainsKey(item1Pos);
        bool item2Present = items.ContainsKey(item2Pos);

        if(item1Present || item2Present)
        {

            if(item1Present && item2Present)
            {
                Item item1 = items[item1Pos];
                Item item2 = items[item2Pos];
                items.Remove(item1Pos);
                items.Remove(item2Pos);
                items.Add(item2Pos, item1);
                items.Add(item1Pos, item2);
            }
            else
            if(item1Present && !item2Present)
            {
                Item item1 = items[item1Pos];
                items.Remove(item1Pos);
                items.Add(item2Pos, item1);
            }
            else
            if (!item1Present && item2Present)
            {
                Item item2 = items[item2Pos];
                items.Remove(item2Pos);
                items.Add(item1Pos, item2);
            }
            InventoryManagerUI.Instance.RefreshUI();
            return true;

        }

        return false;
    }

}
