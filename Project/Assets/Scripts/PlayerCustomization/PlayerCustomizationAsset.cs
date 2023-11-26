using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerCustomization/Player Customization Asset")]
public class PlayerCustomizationAsset : ScriptableObject
{
    public Mesh meshReference;

    public Material material;
    public bool hasSkin;










































    [MenuItem("Tools/MyTool/Do It in C#")]
    static void DoIt()
    {
        EditorUtility.DisplayDialog("MyTool", "Do It in C# !", "OK", "");
    }
}
