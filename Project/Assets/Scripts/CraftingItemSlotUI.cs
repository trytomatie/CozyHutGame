using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CraftingItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI stackSizeText;

    public void SetSlotInfo(Item item, int stackAmount)
    {
        itemImage.sprite = item.sprite;
        Container inventory = GameManager.Instance.playerList[NetworkManager.Singleton.LocalClientId].GetComponent<Container>();
        int stacksInInventory = inventory.GetAmmountOfItem(item.itemId);
        stackSizeText.text = stacksInInventory + " / "+ stackAmount ;
    }
}
