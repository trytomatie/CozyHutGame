using UnityEngine;

public class Item_Wood : Item
{
    public Item_Wood()
    {
        itemId = 0;
        maxStackSize = 500;
        sprite = SpriteManager.Instance.Sprites[itemId];
    }
    
}