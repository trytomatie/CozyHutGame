using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryManagerUI : MonoBehaviour
{
    [SerializeField] private ItemSlotUI[] itemSlots;
    [SerializeField] private GameObject dragImage;
    private static InventoryManagerUI instance;
    [SerializeField] private Inventory inventory;

    private void Awake()
    {
        if(Instance == null)
        {
            instance = this;
            for(int i = 0; i < itemSlots.Length-1;i++)
            {
                itemSlots[i].SlotId = i;
            }
        }
        else
        {
            Destroy(this);
        }
    }

    public void RefreshUI()
    {
        foreach(ItemSlotUI itemslotUI in itemSlots)
        {
            itemslotUI.ItemImage.sprite = null;
            itemslotUI.StackSizeText.text = "";
        }
        foreach(int key in inventory.items.Keys)
        {
            itemSlots[key].Item = inventory.items[key];
            itemSlots[key].ItemImage.sprite = itemSlots[key].Item.sprite;
            itemSlots[key].StackSizeText.text = "x" + itemSlots[key].Item.stackSize;
        }
    }


    public GameObject DragImage { get => dragImage; }
    public static InventoryManagerUI Instance { get => instance; }
    public Inventory Inventory { get => inventory; set => inventory = value; }
}
