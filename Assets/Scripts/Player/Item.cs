using UnityEngine;

[CreateAssetMenu(menuName = "Items/Generic Item")]
public class Item : ScriptableObject
{

    [HideInInspector] public int itemId;
    public string itemName;
    public Sprite sprite;
    public int stackSize = 1;
    public int maxStackSize = 64;
    
}