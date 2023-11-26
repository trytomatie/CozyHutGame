using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
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

    [Range(0, 3)]
    public int irisColorIndex;

    [Range(0, 3)]
    public int pupilColorIndex;

    [Range(0, 3)]
    public int highlightColorIndex;

    [Range(0, 3)]
    public int eyelashColorIndex;

    [Range(0, 3)]
    public int eyebrowColorIndex;

    [Range(0, 5)]
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

    [Range(0, 5)]
    public Material[] skinColor;

    public Material eyeMaterial;
    public Material eyebrowMaterial;
    public Material mouthMaterial;

    public Color[] irisColor;
    public Color[] pupilColor;
    public Color[] highlightColor;
    public Color[] eyebrowColor;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerAppearance();
    }

    private void UpdatePlayerAppearance()
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
        eyeMaterial.SetColor("_IrisColor", irisColor[irisColorIndex]);
        eyeMaterial.SetColor("_PupilColor", pupilColor[pupilColorIndex]);
        eyeMaterial.SetColor("_HighlightColor", highlightColor[highlightColorIndex]);
        eyeMaterial.SetColor("_EyelashColor", eyebrowColor[eyelashColorIndex]);
        eyebrowMaterial.SetColor("_EyebrowColor", eyebrowColor[eyebrowColorIndex]);


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

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0.63f, 0));

                break;

            case 2:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(1.28f, 0));

                break;

            case 3:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0, -0.7f));

                break;

            case 4:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0.63f, -0.7f));

                break;

            case 5:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(1.28f, -0.7f));

                break;

            case 6:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0, -1.6f));

                break;

            case 7:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0.63f, -1.6f));

                break;

            case 8:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(1.28f, -1.6f));

                break;

            case 9:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0, -2.35f));

                break;

            case 10:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0.63f, -2.35f));

                break;

            case 11:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(1.28f, -2.35f));

                break;

            case 12:

                mouthMaterial.SetVector("_SwitchMouth", new Vector2(0, -3.15f));

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
