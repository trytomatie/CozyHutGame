using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Item;

public class EquipmentSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image equipmentImage;
    public TextMeshProUGUI equipmentText;
    public Sprite empty;
    public string equipmentName;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetUpEquipmentSelector(ItemData data)
    {
        if(data.itemId != 0)
        {
            Item itemData = ItemManager.Instance.items[data.itemId];
            equipmentName = itemData.itemName;
            equipmentImage.sprite = itemData.sprite;
        }
        else
        {
            equipmentName = "Unequip";
            equipmentImage.sprite = empty;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        equipmentText.text = equipmentName;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        equipmentText.text = "";
    }
}
