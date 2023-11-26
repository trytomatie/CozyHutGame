using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerUI : MonoBehaviour, IContainerUI
{
    public Container syncedContainer;
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
        SetSyncedInvetory(go.GetComponent<Container>());
        RefreshUI();
    }
    public void SetSyncedInvetory(Container inventory)
    {
        syncedContainer = inventory;
        for (int i = 0; i < itemSlots.Length ; i++)
        {
            itemSlots[i].assignedContainer = syncedContainer;
        }
    }

    public void RefreshUI()
    {
        foreach (ItemSlotUI itemslotUI in itemSlots)
        {
            itemslotUI.ItemImage.sprite = null;
            itemslotUI.StackSizeText.text = "";
        }
        for (int i = 0; i < syncedContainer.items.Length; i++)
        {
            if (syncedContainer.items[i] != null)
            {
                itemSlots[i].ItemImage.sprite = itemSlots[i].Item.sprite;
                itemSlots[i].StackSizeText.text = "x" + itemSlots[i].Item.stackSize;
            }
        }
    }

}
