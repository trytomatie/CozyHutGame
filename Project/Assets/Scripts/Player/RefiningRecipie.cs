using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Recepies/RefiningRecepie")]
public class RefiningRecipie : ScriptableObject
{
    [HideInInspector] public ulong refiningId;
    public enum CraftingCategory { Proccesed, Tool}
    public CraftingCategory craftingCategory;
    public Sprite sprite;
    public string recepieName;
    public Item recepieRessult;
    public Item[] requiredItems;
    public int[] requieredItemsCount;
    public float refiningTime = 20;
}