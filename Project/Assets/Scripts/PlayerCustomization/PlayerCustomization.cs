using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
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
    public MeshRenderer playerHair;
    public MeshFilter playerHairMeshFilter;

    [Range(0, 2)]
    public int torsoIndex;

    [Range(0, 2)]
    public int legIndex;

    [Range(0, 2)]
    public int feetIndex;

    public Color irisColor;

    public Color pupilColor;

    public Color highlightColor;

    public Color eyelashColor;

    public Color eyebrowColor;

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

    [Range(0, 60)]
    public int hairColorIndex;

    [Range(0, 1)]
    public int hairIndex;


    public PlayerCustomizationAsset[] torso;
    public PlayerCustomizationAsset[] legs;
    public PlayerCustomizationAsset[] feet;
    public Mesh[] hair;

    public Material[] skinColor;
    public Material[] hairMaterialMale;
    public Material[] hairMaterialFemale;

    private Material eyeMaterial;
    private Material eyebrowMaterial;
    private Material mouthMaterial;

    public UnityEvent OnUpdateCharacterApearence;


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
            case 13:
                hairIndex = value;
                break;
            case 14:
                hairColorIndex = value;
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
                irisColor = color;
                break;
            case 04:
                pupilColor = color;
                break;
            case 05:
                highlightColor = color;
                break;
            case 06:
                eyelashColor = color;
                break;
            case 07:
                eyebrowColor = color;
                break;
        }
        UpdatePlayerAppearance();
    }


    // Start is called before the first frame update
    void Awake()
    {
        eyeMaterial = playerFace.materials[0];
        mouthMaterial = playerFace.materials[1];
        eyebrowMaterial = playerFace.materials[2];
        playerFace.materials[0] = eyeMaterial;
        playerFace.materials[1] = mouthMaterial;
        playerFace.materials[2] = eyebrowMaterial;
    }

    [ServerRpc (RequireOwnership =false)]
    public void SyncApearenceServerRpc(PlayerSaveDataSerialized playerData,ulong id)
    {
        SyncApearenceClientRpc(playerData, id);
    }

    [ClientRpc]
    public void SyncApearenceClientRpc(PlayerSaveDataSerialized playerData, ulong id,ClientRpcParams clientRpcParams = default)
    {
        AssignPlayerData(playerData);
    }

    private void AssignPlayerData(PlayerSaveDataSerialized playerData)
    {
        torsoIndex = playerData.torsoIndex;
        legIndex = playerData.legIndex;
        feetIndex = playerData.feetIndex;
        irisColor = playerData.irisColor;
        pupilColor = playerData.pupilColor;
        highlightColor = playerData.highlightColor;
        eyelashColor = playerData.eyelashColor;
        eyebrowColor = playerData.eyebrowColor;
        skinColorIndex = playerData.skinColor;
        eyebrowIndex = playerData.eyebrowIndex;
        mouthIndex = playerData.mouthIndex;
        eyelashIndex = playerData.eyelashIndex;
        highlightIndex = playerData.highlightIndex;
        hairIndex = playerData.hairIndex;
        hairColorIndex = playerData.hairColorIndex;
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
        OnUpdateCharacterApearence.Invoke();
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

        // Switch Hair




        /* Switch Color */
        eyeMaterial.SetColor("_IrisColor", irisColor);
        eyeMaterial.SetColor("_PupilColor", pupilColor);
        eyeMaterial.SetColor("_HighlightColor", highlightColor);
        eyeMaterial.SetColor("_EyelashColor", eyelashColor);
        eyebrowMaterial.SetColor("_EyebrowColor", eyebrowColor);

        UpdateHair();

        switch (eyebrowIndex)
        {
            case 0:

                eyebrowMaterial.SetVector("_SwitchEyebrow", new Vector2(0, 0));

                break;

            case 1:

                eyebrowMaterial.SetVector("_SwitchEyebrow", new Vector2(1, 0));

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

    private void UpdateHair()
    {
        playerHairMeshFilter.mesh = hair[hairIndex];
        if (hairIndex == 1)
        {
            playerHair.material = hairMaterialMale[hairColorIndex];
        }
        else 
        {
            playerHair.material = hairMaterialFemale[hairColorIndex];
        }
    }
}
