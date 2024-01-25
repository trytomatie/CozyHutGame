using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerCustomization/Player Customization Asset")]
public class PlayerCustomizationAsset : ScriptableObject
{
    public Mesh meshReference;
    public Sprite thumbnail;
    public Material material;
    public bool hasSkin;

}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerCustomizationAsset))]
public class PlayerCustomizationAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PlayerCustomizationAsset asset = (PlayerCustomizationAsset)target;
        //asset.meshReference = (Mesh)EditorGUILayout.ObjectField("Mesh Reference", asset.meshReference, typeof(Mesh), false);
        //asset.thumbnail = (Sprite)EditorGUILayout.ObjectField("Thumbnail", asset.thumbnail, typeof(Sprite), false);
        //asset.material = (Material)EditorGUILayout.ObjectField("Material", asset.material, typeof(Material), false);
        //asset.hasSkin = EditorGUILayout.Toggle("Has Skin", asset.hasSkin);
        if (GUILayout.Button("Set Thumbnail",GUILayout.Height(40)))
        {
            SetThumbnail(asset);
        }
    }

    private void SetThumbnail(PlayerCustomizationAsset asset)
    {
        // Look for the Sprite in the Assets folder
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Screenshots/{asset.name}.png");
        asset.thumbnail = sprite;
        Debug.Log($"Loaded Sprite for{asset.name}: {sprite} as thumbnail");

    }
}
#endif
