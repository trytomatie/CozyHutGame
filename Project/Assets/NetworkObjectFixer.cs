using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class NetworkObjectFixer : MonoBehaviour
{
    public void ValidateGameObject()
    {
    }

}

[CustomEditor(typeof(NetworkObjectFixer))]
public class NetworkObjectFixerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NetworkObjectFixer script = (NetworkObjectFixer)target;

        // Add a button to the Inspector
        if (GUILayout.Button("Revalidate GameObject"))
        {
            script.GetComponent<NetworkObject>().AlwaysReplicateAsRoot = !script.GetComponent<NetworkObject>().AlwaysReplicateAsRoot;
            script.GetComponent<NetworkObject>().AlwaysReplicateAsRoot = !script.GetComponent<NetworkObject>().AlwaysReplicateAsRoot;
            EditorUtility.SetDirty(script);
        }
    }
}
