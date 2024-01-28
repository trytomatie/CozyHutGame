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
[CanEditMultipleObjects]
public class PlayerCustomizationAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PlayerCustomizationAsset[] assets = Selection.GetFiltered<PlayerCustomizationAsset>(SelectionMode.Assets);
        //asset.meshReference = (Mesh)EditorGUILayout.ObjectField("Mesh Reference", asset.meshReference, typeof(Mesh), false);
        //asset.thumbnail = (Sprite)EditorGUILayout.ObjectField("Thumbnail", asset.thumbnail, typeof(Sprite), false);
        //asset.material = (Material)EditorGUILayout.ObjectField("Material", asset.material, typeof(Material), false);
        //asset.hasSkin = EditorGUILayout.Toggle("Has Skin", asset.hasSkin);
        if (GUILayout.Button("Set Thumbnail",GUILayout.Height(40)))
        {
            SetThumbnail(assets);
        }
    }

    private void SetThumbnail(PlayerCustomizationAsset[] assets)
    {
        foreach(PlayerCustomizationAsset asset in assets)
        {
            // Look for the Sprite in the Assets folder
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Screenshots/{asset.name}.png");
            asset.thumbnail = sprite;
            if(sprite != null)
            {
                Debug.Log($"Loaded Sprite for{asset.name}: {sprite} as thumbnail");
            }
            else
            {
                Debug.LogError($"Could not find Sprite for {asset.name}");
            }
            // Set dirty to save changes
            EditorUtility.SetDirty(asset);
        }
    }
}
#endif
