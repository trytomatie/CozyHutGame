using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedSkinnedMeshRendererBones : MonoBehaviour
{
    public SkinnedMeshRenderer targetMeshRenderer;
    public SkinnedMeshRenderer originMeshRenderer;
    public Transform rootBone;

    // Start is called before the first frame update
    void Start()
    {
        SkinnedMeshRenderer spawnedPrefab = Instantiate(targetMeshRenderer, transform);
        spawnedPrefab.bones = originMeshRenderer.bones;
        spawnedPrefab.rootBone = rootBone;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
