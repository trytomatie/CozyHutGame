using MalbersAnimations;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryManagerUI : ContainerUI, IContainerUI
{
    public GameObject dragImage;
    public TextMeshProUGUI itemNameText;
    public GameObject inventoryUI;
    public Container equipmentContainer;
    private static InventoryManagerUI instance;
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
        inventoryUI.SetActive(value);
    }


    public GameObject DragImage { get => dragImage; }
}
