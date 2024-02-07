using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Item;

public class ContainerUI : MonoBehaviour, IContainerUI
{
    public Container syncedContainer;
    public ItemSlotUI[] itemSlots;

    public void Start()
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
        RefreshUI(go);
    }
    public void SetSyncedInvetory(Container inventory)
    {
        syncedContainer = inventory;
        for (int i = 0; i < itemSlots.Length ; i++)
        {
            itemSlots[i].assignedContainer = syncedContainer;
        }
    }

    public virtual void RefreshUI(GameObject container)
    {
        if (container.GetComponent<Container>() == syncedContainer)
        {
            foreach (ItemSlotUI itemslotUI in itemSlots)
            {
                itemslotUI.ItemImage.sprite = null;
                itemslotUI.StackSizeText.text = "";
            }
            for (int i = 0; i < itemSlots.Length; i++)
            {
                Sprite sprite = ItemManager.GetItemReference(syncedContainer.items[i].itemId).sprite;
                itemSlots[i].ItemImage.sprite = sprite;
                if (syncedContainer.items[i] != ItemData.Null)
                {
                    itemSlots[i].StackSizeText.text = "x" + ItemManager.GenerateItem(itemSlots[i].ItemRef).stackSize;
                }
            }
        }
    }

    public virtual void RefreshUI(Container container)
    {

    }
}
