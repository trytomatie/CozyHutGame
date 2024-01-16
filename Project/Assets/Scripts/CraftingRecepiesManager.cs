using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingRecepiesManager : MonoBehaviour
{
    public CraftingRecepie[] craftingRecepies;

    private static CraftingRecepiesManager instance;

    public static CraftingRecepiesManager Instance { get => instance; }

    private void Awake()
    {
        if (Instance == null)
        {
            instance = this;
            AssignItemIds();
        }
        else
        {
            Destroy(this);
        }
    }

    private void AssignItemIds()
    {
        ulong i = 0;
        foreach (CraftingRecepie craftingRecpie in craftingRecepies)
        {
            craftingRecpie.craftingId = i;
            i++;
        }
    }
}
