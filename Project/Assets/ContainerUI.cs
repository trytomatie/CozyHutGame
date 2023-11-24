using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerUI : MonoBehaviour, IContainerUI
{
    public Inventory syncedInvetory;
    public ItemSlotUI[] itemSlots;

    private void Start()
    {
        for (int i = 0; i < itemSlots.Length ; i++)
        {
            itemSlots[i].SlotId = i;
            itemSlots[i].manager = this;
        }
    }
    public virtual void SetSyncedInvetory(GameObject go)
    {
        SetSyncedInvetory(go.GetComponent<Inventory>());
        RefreshUI();
    }
    public void SetSyncedInvetory(Inventory inventory)
    {
        syncedInvetory = inventory;
        for (int i = 0; i < itemSlots.Length ; i++)
        {
            itemSlots[i].assignedInveotry = syncedInvetory;
        }
    }

    public void RefreshUI()
    {
        // Refresh the invenotry
        foreach (ItemSlotUI itemslotUI in itemSlots)
        {
            itemslotUI.ItemImage.sprite = null;
            itemslotUI.StackSizeText.text = "";
        }
        for (int i = 0; i < syncedInvetory.items.Length; i++)
        {
            if (syncedInvetory.items[i] != null)
            {
                itemSlots[i].ItemImage.sprite = itemSlots[i].Item.sprite;
                itemSlots[i].StackSizeText.text = "x" + itemSlots[i].Item.stackSize;
            }

        }
    }

}
