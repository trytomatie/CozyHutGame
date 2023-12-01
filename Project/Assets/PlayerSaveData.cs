using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSaveData : MonoBehaviour
{
    public List<ulong> discoverdItemIDs = new List<ulong>();
    [HideInInspector] public List<CraftingRecepie> discoveredRecipies = new List<CraftingRecepie>();

    public int torsoIndex;
    public int legIndex;
    public int feetIndex;
    public int irisColorIndex;
    public int pupilColorIndex;
    public int highlightColorIndex;
    public int eyelashColorIndex;
    public int eyebrowColorIndex;
    public int skinColorIndex;
    public int eyebrowIndex;
    public int mouthIndex;
    public int eyelashIndex;
    public int highlightIndex;

    private void Start()
    {
        DiscoverRecepies();
    }
    public virtual void DiscoverItem(Item item)
    {
        if(!discoverdItemIDs.Contains(item.itemId))
        {
            discoverdItemIDs.Add(item.itemId);
            DiscoverRecepies();
        }
    }

    public bool CheckIfItemHasBeenDiscovered(Item item)
    {
        if (discoverdItemIDs.Contains(item.itemId))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public virtual void DiscoverRecepies()
    {
        discoveredRecipies = new List<CraftingRecepie>();
        foreach (CraftingRecepie cr in CraftingRecepiesManager.Instance.craftingRecepies)
        {
            int count = cr.requiredItems.Length;
            foreach (Item item in cr.requiredItems)
            {
                if(CheckIfItemHasBeenDiscovered(item))
                {
                    count--;
                }
            }
            if(count <= 0)
            {
                discoveredRecipies.Add(cr);
            }
        }

    }
}
