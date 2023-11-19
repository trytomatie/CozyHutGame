using MalbersAnimations.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerEnterHandler , IPointerExitHandler
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI stackSizeText;
    public Item.ItemType typeRestriction = Item.ItemType.None;
    private Item item;
    private int slotId = 0;
    private GameObject draggedObject;
    public MEvent hoverEvent;


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
                ItemSlotUI slot = go.GetComponent<ItemSlotUI>();
                if (slot.typeRestriction== Item.ItemType.None || slot.typeRestriction == Item.itemType)
                {
                    InventoryManagerUI.Instance.Inventory.SwapItemPlaces(slotId, go.GetComponent<ItemSlotUI>().SlotId);
                    InventoryManagerUI.Instance.RefreshUI();
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
    public Item Item { get => item; set => item = value; }
    public TextMeshProUGUI StackSizeText { get => stackSizeText; set => stackSizeText = value; }
}
