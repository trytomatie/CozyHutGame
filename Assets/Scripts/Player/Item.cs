using UnityEngine;

public class Item
{
    public int itemId;
    public Sprite sprite;
    public int stackSize = 1;
    public int maxStackSize = 64;
    public static Item GenerateItem(int id)
    {
        switch (id)
        {
            case 0:
                return new Item_Wood();
        }
        return null;
    }
}