using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Item;

public class CraftingManagerUI : MonoBehaviour
{
    public GameObject craftingSlotUIprefab;
    public Transform craftingSlotContainer;
    public CraftingItemSlotUI[] craftingItemSlots;
    public CraftingRecepie selectedRecipe;
    public GameObject selectedCraftingSlot;
    public GameObject craftingButton;
    public UnityEvent CannotCraftEvent; 
    
    public void OpenCraftingUI()
    {
        selectedRecipe = null;
        GameObject player = GetLocalPlayer();
        for (int i = 0; i < craftingSlotContainer.childCount;i++)
        {
            Destroy(craftingSlotContainer.transform.GetChild(i).gameObject);
        }
        foreach(CraftingRecepie cr in player.GetComponent<PlayerSaveData>().discoveredRecipies)
        {
            GameObject go = Instantiate(craftingSlotUIprefab, craftingSlotContainer);
            go.GetComponent<CraftingSlotUI>().SetUpCraftingSlot(cr);
        }
    }

    public virtual void SetCraftingRecipe(GameObject go)
    {
        CraftingSlotUI craftingSlot = go.GetComponent<CraftingSlotUI>();
        selectedRecipe = craftingSlot.craftingRecipieInfo;
        int i = 0;
        foreach (Item item in craftingSlot.craftingRecipieInfo.requiredItems)
        {
            craftingItemSlots[i].SetSlotInfo(item, craftingSlot.craftingRecipieInfo.requieredItemsCount[i]);
            i++;
        }
        CanCraft();
        selectedCraftingSlot = go;
    }


    public void Craft()
    {
        GameObject player = GetLocalPlayer();

        if (CanCraft())
        {

            ItemData[] itemData = GetItemData();
            Container inventory = player.GetComponent<Container>();
            if (inventory.HasItemSpaceInInventory(new ItemData(selectedRecipe.recepieRessult.itemId, 1)))
            {
                foreach (ItemData data in itemData)
                {
                    inventory.RequestRemoveItemServerRpc(data);
                }

                // I NEVER TELL THE SERVER TO ADD AN ITEM; BUT IT JUST DOES IT... WHY?????????????ß EDIT: Nvm i found the issue, I just added it in HasItemSpaceInInventory

                inventory.AddItemServerRpc(new ItemData(selectedRecipe.recepieRessult.itemId, 1));
               
                SetCraftingRecipe(selectedCraftingSlot);
            }
            else
            {
                CannotCraftEvent.Invoke();
            }

        }

    }

    private bool CanCraft()
    {
        GameObject player = GetLocalPlayer();
        Container inventory = player.GetComponent<Container>();

        ItemData[] itemData = GetItemData();
        foreach (ItemData data in itemData)
        {
            if(!(inventory.GetAmmountOfItem(data.itemId) >= data.stackSize))
            {
                SetCraftingButton(false);
                return false;
            }
        }
        SetCraftingButton(true);
        return true;
    }

    private void SetCraftingButton(bool value)
    {
        if(value)
        {
            craftingButton.GetComponentInChildren<Button>().interactable = true;
            craftingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Craft";
        }
        else
        {
            craftingButton.GetComponentInChildren<Button>().interactable = false;
            craftingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Not Enough Materials";
        }

    }

    private ItemData[] GetItemData()
    {
        ItemData[] itemData = new ItemData[selectedRecipe.requiredItems.Length];
        for (int i = 0; i < selectedRecipe.requiredItems.Length; i++)
        {
            itemData[i] = new ItemData()
            {
                itemId = selectedRecipe.requiredItems[i].itemId,
                stackSize = selectedRecipe.requieredItemsCount[i]
            };
        }
        return itemData;
    }

    private GameObject GetLocalPlayer()
    {
        GameObject player = null;
        if (player == null)
            player = GameManager.Instance.playerList[NetworkManager.Singleton.LocalClientId];
        return player;
    }
}
