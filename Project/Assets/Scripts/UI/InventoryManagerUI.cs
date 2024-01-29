using MalbersAnimations;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Item;

public class InventoryManagerUI : ContainerUI, IContainerUI
{
    public GameObject dragImage;

    public GameObject inventoryUI;
    public Container equipmentContainer;
    private static InventoryManagerUI instance;
    public ItemSlotUI[] equipmentSlots;

    [Header("Hover Item")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemNameDescription;
    public Image itemImage;
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

    public void RefreshUI()
    {
        if(syncedContainer != null)
        {
            RefreshUI(syncedContainer.gameObject);
            RefreshEquipmentUI(equipmentContainer.gameObject);
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
            itemNameDescription.text = item.itemDescription;
            itemImage.sprite = item.sprite;
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

    public virtual void RefreshEquipmentUI(Container container)
    {
        if (container == equipmentContainer)
        {
            foreach (ItemSlotUI itemslotUI in equipmentSlots)
            {
                itemslotUI.ItemImage.sprite = null;
                itemslotUI.ItemImage.enabled = false;
                itemslotUI.StackSizeText.text = "";
            }
            for (int i = 0; i < equipmentContainer.items.Length; i++)
            {
                if (equipmentContainer.items[i] != ItemData.Null)
                {
                    equipmentSlots[i].ItemImage.enabled = true;
                    equipmentSlots[i].ItemImage.sprite = ItemManager.GenerateItem(equipmentSlots[i].ItemRef).sprite;
                    //equipmentSlots[i].StackSizeText.text = "x" + ItemManager.GenerateItem(itemSlots[i].ItemRef).stackSize;
                }
            }
        }
    }
    public virtual void RefreshEquipmentUI(GameObject container)
    {
        RefreshEquipmentUI(container.GetComponent<Container>());
    }

    public GameObject DragImage { get => dragImage; }
}
