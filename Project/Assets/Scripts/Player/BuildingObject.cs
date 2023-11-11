using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "BuildingObject/Building Object")]
public class BuildingObject : ScriptableObject
{
    // Ids are getting Distributed from the BuildingManager on Start
    [HideInInspector]public ulong buildingId;
    public string buildingName;
    public GameObject buildingPrefab;
    public Sprite sprite;
    public Item[] buildingMaterials;
    public int[] buildingMaterialAmounts;
}