using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "BuildingObject/Building Object")]
public class BuildingObject : ScriptableObject
{
    public enum BuildingType { Other, Floor, Wall, Stair, Plliar}
    public enum BuildingCategory { Building,Crafting,Furniture,Agriculture}
    // Ids are getting Distributed from the BuildingManager on Start
    [HideInInspector]public ulong buildingId;
    public BuildingType buildingType;
    public BuildingCategory buildingCategory;
    public string buildingName;
    public GameObject buildingPrefab;
    public Sprite sprite;
    public Item[] buildingMaterials;
    public int[] buildingMaterialAmounts;
}

#if UNITY_EDITOR
[CustomEditor(typeof(BuildingObject))]
[CanEditMultipleObjects]
public class BuildingObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        BuildingObject[] assets = Selection.GetFiltered<BuildingObject>(SelectionMode.Assets);
        //asset.meshReference = (Mesh)EditorGUILayout.ObjectField("Mesh Reference", asset.meshReference, typeof(Mesh), false);
        //asset.thumbnail = (Sprite)EditorGUILayout.ObjectField("Thumbnail", asset.thumbnail, typeof(Sprite), false);
        //asset.material = (Material)EditorGUILayout.ObjectField("Material", asset.material, typeof(Material), false);
        //asset.hasSkin = EditorGUILayout.Toggle("Has Skin", asset.hasSkin);
        if (GUILayout.Button("Set Thumbnail", GUILayout.Height(40)))
        {
            SetThumbnail(assets);
        }
    }

    private void SetThumbnail(BuildingObject[] assets)
    {
        foreach (BuildingObject asset in assets)
        {
            if(asset.buildingPrefab == null)
            {
                Debug.LogError("BuildingPrefab is null");
            }
            // Look for the Sprite in the Assets folder
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Screenshots/{asset.buildingPrefab.name}.png");
            asset.sprite = sprite;
            if (sprite != null)
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
