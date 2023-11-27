using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Item;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private Item[] items;

    private static ItemManager instance;

    public static ItemManager Instance { get => instance; }

    private void Start()
    {
        if(Instance == null)
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
        foreach (Item item in items)
        {
            item.itemId = i;
            i++;
        }
    }

    public static Item GenerateItem(ulong i)
    {
        if(i == 0)
        {
            return null;
        }
        return Instantiate(Instance.items[i]);
    }

    public static Item GenerateItem(ItemData data)
    {
        if(data.itemId == 0)
        {
            return null;
        }
        Item item = Instance.items[data.itemId];
        item.stackSize = data.stackSize;
        return Instantiate(item);
    }

    public static Item GenerateItem(string name)
    {
        int i = 0;
        foreach(Item item in Instance.items)
        {
            if(item.itemName == name)
            {
                return Instantiate(Instance.items[i]);
            }
            i++;
        }
        return null;

    }

    public static int GetMaxStackSize(ulong id)
    {
        return Instance.items[id].maxStackSize;
    }

    public static ulong GetItemId(string itemName)
    {
        ulong i = 0;
        foreach (Item item in Instance.items)
        {
            if (item.itemName == itemName)
            {
                return i;
            }
            i++;
        }
        return 0;
    }

}
