using MalbersAnimations.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ColorPickerCreator : MonoBehaviour
{
    public GameObject colorPickerPrefab;
    public Color[] colors;
}

[CustomEditor(typeof(ColorPickerCreator))]
public class ColorPickerCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Fill Content"))
        {
            ColorPickerCreator creator = (ColorPickerCreator)target;
            FillContent(creator);
            // Save changes
            EditorUtility.SetDirty(creator);

        }


    }
    public void FillContent(ColorPickerCreator creator)
    {
        Transform parent = creator.transform;
        ToggleGroup toggleGroup = creator.GetComponent<ToggleGroup>();
        // Clear content
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        // Fill content
        foreach (Color color in creator.colors)
        {
            GameObject colorPicker = Instantiate(creator.colorPickerPrefab, parent);
            colorPicker.GetComponent<Image>().color = color;
            colorPicker.GetComponent<Toggle>().group = toggleGroup;
            // Save chagnes
            EditorUtility.SetDirty(colorPicker);
        }
    }
}
