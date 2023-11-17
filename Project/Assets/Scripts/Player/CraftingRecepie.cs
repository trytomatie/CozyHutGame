using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting Recepie/Crafting Recepie")]
public class CraftingRecepie : ScriptableObject
{
    [HideInInspector] public ulong craftingId;
    public enum CraftingCategory { Proccesed, Tool}
    public CraftingCategory craftingCategory;
    public Sprite sprite;
    public string recepieName;
    public Item recepieRessult;
    public Item[] requiredItems;
    public int[] requieredItemsCount;
}