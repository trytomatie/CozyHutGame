using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCustomization : NetworkBehaviour
{
    public string playerName;
    public SkinnedMeshRenderer playerTorso;
    public SkinnedMeshRenderer playerLegs;
    public SkinnedMeshRenderer playerFeet;
    public SkinnedMeshRenderer playerHead;
    public SkinnedMeshRenderer playerEars;
    public SkinnedMeshRenderer playerFace;

    [Range(0, 2)]
    public int torsoIndex;

    [Range(0, 2)]
    public int legIndex;

    [Range(0, 2)]
    public int feetIndex;

    public Color irisColorIndex;

    public Color pupilColorIndex;

    public Color highlightColorIndex;

    public Color eyelashColorIndex;

    public Color eyebrowColorIndex;

    // Has to be int
    public int skinColorIndex;

    [Range(0, 5)]
    public int eyebrowIndex;

    [Range(0, 11)]
    public int mouthIndex;

    [Range(0, 4)]
    public int eyelashIndex;

    [Range(0, 5)]
    public int highlightIndex;


    public PlayerCustomizationAsset[] torso;
    public PlayerCustomizationAsset[] legs;
    public PlayerCustomizationAsset[] feet;

    public Material[] skinColor;

    private Material eyeMaterial;
    private Material eyebrowMaterial;
    private Material mouthMaterial;

    public Color[] irisColor;
    public Color[] pupilColor;
    public Color[] highlightColor;
    public Color[] eyebrowColor;

#if UNITY_EDITOR
    [Header("Color Pallete Assigner")]
    public Sprite colorPalleteLoaderIris;
#endif

    public void SetIndex(int value, int index)
    {
        switch(index)
        {
            case 0:
                torsoIndex = value;
                break;
            case 01:
                legIndex = value;
                break;
            case 02:
                feetIndex = value;
                break;
            case 03:
                //irisColorIndex = value;
                break;
            case 04:
                //pupilColorIndex = value;
                break;
            case 05:
                //highlightColorIndex = value;
                break;
            case 06:
                //eyelashColorIndex = value;
                break;
            case 07:
                //eyebrowColorIndex = value;
                break;
            case 08:
                skinColorIndex = value;
                break;
            case 09:
                eyebrowIndex = value;
                break;
            case 10:
                mouthIndex = value;
                break;
            case 11:
                eyelashIndex = value;
                break;
            case 12:
                highlightIndex = value;
                break;
        }
        UpdatePlayerAppearance();
    }

    public void SetColor(int index, Image image)
    {
        Color color = image.color;
        switch (index)
        {
            case 03:
                irisColorIndex = color;
                break;
            case 04:
                pupilColorIndex = color;
                break;
            case 05:
                highlightColorIndex = color;
                break;
            case 06:
                eyelashColorIndex = color;
                break;
            case 07:
                eyebrowColorIndex = color;
                break;
        }
        UpdatePlayerAppearance();
    }


    // Start is called before the first frame update
    void Start()
    {
        eyeMaterial = playerFace.materials[0];
        mouthMaterial = playerFace.materials[1];
        eyebrowMaterial = playerFace.materials[2];
        playerFace.materials[0] = eyeMaterial;
        playerFace.materials[1] = mouthMaterial;
        playerFace.materials[2] = eyebrowMaterial;
        SyncApearenceServerRpc(GameManager.Instance.playerSaveData.GetPlayerSaveData(), NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc (RequireOwnership =false)]
    public void SyncApearenceServerRpc(PlayerSaveDataSerialized playerData,ulong id)
    {
        SyncApearenceClientRpc(playerData, id);
    }

    [ClientRpc]
    public void SyncApearenceClientRpc(PlayerSaveDataSerialized playerData, ulong id,ClientRpcParams clientRpcParams = default)
    {
        torsoIndex = playerData.torsoIndex;
        legIndex = playerData.legIndex;
        feetIndex = playerData.feetIndex;
        irisColorIndex = playerData.irisColor;
        pupilColorIndex = playerData.pupilColor;
        highlightColorIndex = playerData.highlightColor;
        eyelashColorIndex = playerData.eyelashColor;
        eyebrowColorIndex = playerData.eyebrowColor;
        skinColorIndex = playerData.skinColor;
        eyebrowIndex = playerData.eyebrowIndex;
        mouthIndex = playerData.mouthIndex;
        eyelashIndex = playerData.eyelashIndex;
        highlightIndex = playerData.highlightIndex;
        UpdatePlayerAppearance();
    }
    public virtual void SetPlayerName(string name)
    {
        playerName = name;
    }

    public void UpdatePlayerAppearance()
    {
        UpdatePlayerAsset(torso[torsoIndex], playerTorso);
        UpdatePlayerAsset(legs[legIndex], playerLegs);
        UpdatePlayerAsset(feet[feetIndex], playerFeet);

        playerHead.material = skinColor[skinColorIndex];
        playerEars.material = skinColor[skinColorIndex];
    }

    private void UpdatePlayerAsset(PlayerCustomizationAsset playerCustomizationAsset, SkinnedMeshRenderer meshRenderer)
    {
        /* Switch Clothes */
        meshRenderer.sharedMesh = playerCustomizationAsset.meshReference;
        if (playerCustomizationAsset.hasSkin == true)
        {
            Material[] materials = new Material[2];

            materials[0] = skinColor[skinColorIndex];
            materials[1] = playerCustomizationAsset.material;

            meshRenderer.materials = materials;
        }
        else
        {
            meshRenderer.material = playerCustomizationAsset.material;
        }


        /* Switch Color */
        eyeMaterial.SetColor("_IrisColor", irisColorIndex);
        eyeMaterial.SetColor("_PupilColor", pupilColorIndex);
        eyeMaterial.SetColor("_HighlightColor", highlightColorIndex);
        eyeMaterial.SetColor("_EyelashColor", eyelashColorIndex);
        eyebrowMaterial.SetColor("_EyebrowColor", eyebrowColorIndex);


        switch (eyebrowIndex)
        {
            case 0:

                eyebrowMaterial.SetVector("_SwitchEyebrow", new Vector2(0, 0));

                break;

            case 1:

                eyebrowMaterial.SetVector("_SwitchEyebrow", new Vector2(0, 0));

                break;

            case 2:

                eyebrowMaterial.SetVector("_SwitchEyebrow", new Vector2(0, -1.1f));

                break;

            case 3:

                eyebrowMaterial.SetVector("_SwitchEyebrow", new Vector2(1, -1.1f));

                break;

            case 4:

                eyebrowMaterial.SetVector("_SwitchEyebrow", new Vector2(0, -2.1f));

                break;

            case 5:

                eyebrowMaterial.SetVector("_SwitchEyebrow", new Vector2(1, -2.1f));

                break;
        }

        switch (mouthIndex)
        {
            case 0:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0, 0));

                break;

            case 1:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0.64f, 0));

                break;

            case 2:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(1.285f, 0));

                break;

            case 3:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0.01f, -0.75f));

                break;

            case 4:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0.64f, -0.8f));

                break;

            case 5:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(1.285f, -0.75f));

                break;

            case 6:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(-0.01f, -1.6f));

                break;

            case 7:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0.63f, -1.65f));

                break;

            case 8:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(1.3f, -1.65f));

                break;

            case 9:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0, -2.425f));

                break;

            case 10:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0.63f, -2.45f));

                break;

            case 11:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(1.28f, -2.4f));

                break;

            case 12:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0.005f, -3.18f));

                break;
        }

        switch (eyelashIndex)
        {
            case 0:

                eyeMaterial.SetVector("_SwitchEyelashes", new Vector2(0, 0));

                break;

            case 1:

                eyeMaterial.SetVector("_SwitchEyelashes", new Vector2(0, -0.255f));

                break;

            case 2:

                eyeMaterial.SetVector("_SwitchEyelashes", new Vector2(0, -0.505f));

                break;

            case 3:

                eyeMaterial.SetVector("_SwitchEyelashes", new Vector2(0, -0.69f));

                break;


        }

        switch (highlightIndex)
        {
            case 0:

                eyeMaterial.SetVector("_SwitchHighlight", new Vector2(0, 0));

                break;

            case 1:

                eyeMaterial.SetVector("_SwitchHighlight", new Vector2(-1, 0));

                break;

            case 2:

                eyeMaterial.SetVector("_SwitchHighlight", new Vector2(0, -1));

                break;

            case 3:

                eyeMaterial.SetVector("_SwitchHighlight", new Vector2(-1, -1));

                break;

            case 4:

                eyeMaterial.SetVector("_SwitchHighlight", new Vector2(-1, -2));

                break;

            case 5:

                eyeMaterial.SetVector("_SwitchHighlight", new Vector2(-2, -2));

                break;


        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerCustomization))]
public class MyPlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlayerCustomization yourScript = (PlayerCustomization)target;

        // Add a button to the inspector
        if (GUILayout.Button("Define Player Colors"))
        {
            // Code to execute when the button is clicked
            yourScript.irisColor = SetPixelData(yourScript.colorPalleteLoaderIris.texture);
        }
    }

    private Color[] SetPixelData(Texture2D tex)
    {
        if (tex == null) return null;
        Color[] result = tex.GetPixels();
        if (result.Length > 1056)
        {
            return null;
        }
        return result;
    }
}
#endif
