using MalbersAnimations;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static Item;

public class InventoryManagerUI : ContainerUI, IContainerUI
{
    public GameObject dragImage;
    public TextMeshProUGUI itemNameText;
    public GameObject inventoryUI;
    public Container equipmentContainer;
    private static InventoryManagerUI instance;
    public ItemSlotUI[] equipmentSlots;
    public static InventoryManagerUI Instance { get { return instance; } }

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            equipmentSlots[i].SlotId = i;
            equipmentSlots[i].manager = this;
        }
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i].SlotId = i;
            itemSlots[i].manager = this;
        }


    }

    public void SetSyncedEquipmentInventory(Container inventory)
    {
        equipmentContainer = inventory;
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            equipmentSlots[i].assignedContainer = equipmentContainer;
        }
    }

    public void ToggleInventoryButton(object sender,InputAction.CallbackContext value)
    {
         inventoryUI.SetActive(!inventoryUI.activeSelf);
    }

    public virtual void DisplayName(GameObject go)
    {
        Item item = ItemManager.GenerateItem(go.GetComponent<ItemSlotUI>().ItemRef) ?? null;
        if (item != null)
        {
            itemNameText.text = item.itemName;
            itemNameText.transform.parent.gameObject.SetActive(true);
        }

    }


    public virtual void ToggleInventory(bool value)
    {
        if(value)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        inventoryUI.SetActive(value);
    }


    public virtual void RefreshEquipmentUI(GameObject container)
    {

        if (container.GetComponent<Container>() == equipmentContainer)
        {
            foreach (ItemSlotUI itemslotUI in equipmentSlots)
            {
                itemslotUI.ItemImage.sprite = null;
                itemslotUI.StackSizeText.text = "";
            }
            for (int i = 0; i < equipmentContainer.items.Length; i++)
            {
                if (equipmentContainer.items[i] != ItemData.Null)
                {
                    equipmentSlots[i].ItemImage.sprite = ItemManager.GenerateItem(equipmentSlots[i].ItemRef).sprite;
                    //equipmentSlots[i].StackSizeText.text = "x" + ItemManager.GenerateItem(itemSlots[i].ItemRef).stackSize;
                }
            }
        }
    }

    public GameObject DragImage { get => dragImage; }
}
