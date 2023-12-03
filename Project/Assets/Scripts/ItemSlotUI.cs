using MalbersAnimations.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Item;

public class ItemSlotUI : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerEnterHandler , IPointerExitHandler
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI stackSizeText;
    public Item.ItemType typeRestriction = global::Item.ItemType.None;
    private int slotId = 0;
    private GameObject draggedObject;
    public MEvent hoverEvent;
    public Container assignedContainer;
    public IContainerUI manager;


    public void OnBeginDrag(PointerEventData eventData)
    {
        draggedObject = Instantiate(InventoryManagerUI.Instance.DragImage, GameManager.Instance.gameUI.canvas.transform);
        Image img = draggedObject.GetComponent<Image>();
        img.sprite = itemImage.sprite;
        img.rectTransform.sizeDelta = itemImage.rectTransform.sizeDelta;
    }

    public void OnDrag(PointerEventData eventData)
    {
        draggedObject.GetComponent<RectTransform>().position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
       
        foreach(GameObject go in eventData.hovered)
        {
            if(go.GetComponent<ItemSlotUI>() != null)
            {
                Item itemObject = ItemManager.GenerateItem(ItemRef);
                ItemSlotUI slot = go.GetComponent<ItemSlotUI>();
                if (slot.typeRestriction== Item.ItemType.None || slot.typeRestriction == itemObject.itemType)
                {
                    ItemData item1 = ItemData.Null;
                    ItemData item2 = ItemData.Null;
                    if (itemObject != null)
                        item1 = new ItemData(itemObject.itemId, itemObject.stackSize);
                    if (slot.ItemRef != null)
                        item2 = slot.ItemRef;
                    int pos1 = SlotId;
                    int pos2 = slot.slotId;
                    if(ItemRef.itemId == slot.ItemRef.itemId)
                    {
                        assignedContainer.RequestItemStackOntopOfServerRpc(slot.assignedContainer, pos1, pos2, item1, item2);
                    }
                    else
                    {
                        // print($"{assignedContainer}___{slot.assignedContainer}");
                        assignedContainer.RequestItemSwapServerRpc(slot.assignedContainer, pos1, pos2, item1, item2);
                        //InventoryManagerUI.Instance.Inventory.SwapItemPlaces(slotId, go.GetComponent<ItemSlotUI>().SlotId);
                        //manager.RefreshUI();
                        //slot.manager.RefreshUI();
                    }
                    break;
                }
            }
        }
        Destroy(draggedObject);
        draggedObject = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverEvent.Invoke(true);
        hoverEvent.Invoke(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverEvent.Invoke(false);
    }

    public int SlotId { get => slotId; set => slotId = value; }
    public Image ItemImage { get => itemImage; set => itemImage = value; }
    public ItemData ItemRef { get => assignedContainer.items[SlotId]; }
    public TextMeshProUGUI StackSizeText { get => stackSizeText; set => stackSizeText = value; }
}
