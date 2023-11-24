using MalbersAnimations;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryManagerUI : MonoBehaviour, IContainerUI
{
    [SerializeField] public GameObject inventoryUI;
    [Tooltip("The last few slots are reserved for tools")]
    [SerializeField] private ItemSlotUI[] itemSlots;
    [SerializeField] private GameObject dragImage;
    private static InventoryManagerUI instance;
    [SerializeField] private Inventory inventory;
    [SerializeField] private TextMeshProUGUI itemNameText;

    private void Awake()
    {
        if(Instance == null)
        {
            instance = this;
            // assign the item slot ids
            for(int i = 0; i < itemSlots.Length;i++)
            {
                itemSlots[i].SlotId = i;
                itemSlots[i].manager = this;
            }
        }
        else
        {
            Destroy(this);
        }
    }

    public void ToggleInventoryButton(object sender,InputAction.CallbackContext value)
    {
         inventoryUI.SetActive(!inventoryUI.activeSelf);
    }

    public virtual void DisplayName(GameObject go)
    {
        Item item = go.GetComponent<ItemSlotUI>().Item ?? null;
        if (item != null)
        {
            itemNameText.text = go.GetComponent<ItemSlotUI>().Item.itemName;
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
        Instance.inventoryUI.SetActive(value);
    }

    public void RefreshUI()
    {
        // Refresh the invenotry
        foreach(ItemSlotUI itemslotUI in itemSlots)
        {
            itemslotUI.ItemImage.sprite = null;
            itemslotUI.StackSizeText.text = "";
        }
        for (int i = 0; i < inventory.items.Length; i++)
        {
            if(inventory.items[i] != null)
            {
                itemSlots[i].ItemImage.sprite = itemSlots[i].Item.sprite;
                itemSlots[i].StackSizeText.text = "x" + itemSlots[i].Item.stackSize;
            }

        }
    }


    public GameObject DragImage { get => dragImage; }
    public static InventoryManagerUI Instance { get => instance; }
    public Inventory Inventory { get => inventory; set
        {

            inventory = value;
            for (int i = 0; i < itemSlots.Length ; i++)
            {
                itemSlots[i].assignedInveotry = inventory;
            }

        }
    }
}
