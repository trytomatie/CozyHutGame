using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetStageManquinManager : MonoBehaviour
{
    [Header("Manequins")]
    public GameObject manequinPrefab_Torso;
    public GameObject manequinPrefab_Legs;
    public GameObject manequinPrefab_Feet;
    public Transform stage;

    public PlayerCustomization playerCustomization;
    // Start is called before the first frame update
    void Start()
    {
        foreach (PlayerCustomizationAsset asset in playerCustomization.torso)
        {
            GameObject go = Instantiate(manequinPrefab_Torso, stage);
            go.name = asset.name;
            SkinnedMeshRenderer meshRenderer = go.transform.Find("Torso").GetComponent<SkinnedMeshRenderer>();
            SetUp(asset, meshRenderer);
        }
        foreach (PlayerCustomizationAsset asset in playerCustomization.legs)
        {
            GameObject go = Instantiate(manequinPrefab_Legs, stage);
            go.name = asset.name;
            SkinnedMeshRenderer meshRenderer = go.transform.Find("Legs").GetComponent<SkinnedMeshRenderer>();
            SetUp(asset, meshRenderer);
        }
        foreach (PlayerCustomizationAsset asset in playerCustomization.feet)
        {
            GameObject go = Instantiate(manequinPrefab_Feet, stage);
            go.name = asset.name;
            SkinnedMeshRenderer meshRenderer = go.transform.Find("Feet").GetComponent<SkinnedMeshRenderer>();
            SetUp(asset, meshRenderer);
        }
    }

    private void SetUp(PlayerCustomizationAsset asset, SkinnedMeshRenderer meshRenderer)
    {
            meshRenderer.sharedMesh = asset.meshReference;
            if (asset.hasSkin == true)
            {
                Material[] materials = new Material[2];

                materials[0] = meshRenderer.materials[0];
                materials[1] = asset.material;

                meshRenderer.materials = materials;
            }
            else
            {
                meshRenderer.material = asset.material;
            }
    }
}
