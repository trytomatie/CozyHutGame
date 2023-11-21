using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedSkinnedMeshRendererBones : MonoBehaviour
{
    public SkinnedMeshRenderer targetMeshRenderer;
    public SkinnedMeshRenderer originMeshRenderer;
    public Transform rootBone;
    public Transform[] targetBones;
    public Transform[] originBones;

    // Start is called before the first frame update
    void Start()
    {

        targetBones = targetMeshRenderer.bones;
        originBones = originMeshRenderer.bones;
        targetMeshRenderer.bones = originMeshRenderer.bones;
        targetMeshRenderer.rootBone = rootBone;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
