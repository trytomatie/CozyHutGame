using System.Collections;
using UnityEditor;
using UnityEngine;

public class Options : MonoBehaviour
{
    [Range(0.0f, 10.0f)]
    public float mouseRotationSpeed = 1;

    [Range(0.0f, 10.0f)]
    public float mouseScrollSpeed = 1;

    private static Options instance;


    // Use this for initialization
    void Start()
    {
        if(Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        LoadOptions();
    }

    public void SaveOptions()
    {
        PlayerPrefs.SetFloat("mouseRotationSpeed", mouseRotationSpeed);
        PlayerPrefs.SetFloat("mouseScrollSpeed", mouseScrollSpeed);
        PlayerPrefs.Save();
    }

    private void LoadOptions()
    {
        if(PlayerPrefs.HasKey("mouseRotationSpeed"))
        {
            mouseRotationSpeed = PlayerPrefs.GetFloat("mouseRotationSpeed");
            mouseRotationSpeed = PlayerPrefs.GetFloat("mouseScrollSpeed");
        }

    }


    public static Options Instance { get => instance;}



}

[CustomEditor(typeof(Options))]
public class OptionsEditor: Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Options options = (Options)target;

        if(GUILayout.Button("Save"))
        {
            options.SaveOptions();
        }
    }
}