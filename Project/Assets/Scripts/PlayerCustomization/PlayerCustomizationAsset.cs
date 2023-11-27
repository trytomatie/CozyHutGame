using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerCustomization/Player Customization Asset")]
public class PlayerCustomizationAsset : ScriptableObject
{
    public Mesh meshReference;

    public Material material;
    public bool hasSkin;

}
