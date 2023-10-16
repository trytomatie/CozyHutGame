using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        int i = 0;
        foreach (Item item in items)
        {
            item.itemId = i;
            i++;
        }
    }

    public static Item GenerateItem(int i)
    {
        return Instantiate(Instance.items[i]);
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

}
