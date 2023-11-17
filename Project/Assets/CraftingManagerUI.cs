using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CraftingManagerUI : MonoBehaviour
{
    public GameObject craftingSlotUIprefab;
    public Transform craftingSlotContainer;
    public CraftingItemSlotUI[] craftingItemSlots;
    
    public void OpenCraftingUI()
    {
        GameObject player = null;
        if (player == null)
            player = GameManager.Instance.playerList[NetworkManager.Singleton.LocalClientId];
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

    public virtual void SetCraftingRecipe(GameObject gameObject)
    {
        CraftingSlotUI craftingSlot = gameObject.GetComponent<CraftingSlotUI>();
        int i = 0;
        foreach (Item item in craftingSlot.craftingRecipieInfo.requiredItems)
        {
            craftingItemSlots[i].SetSlotInfo(item, craftingSlot.craftingRecipieInfo.requieredItemsCount[i]);
            i++;
        }

    }
}
