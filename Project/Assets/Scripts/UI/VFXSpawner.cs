using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;


public class VFXSpawner : MonoBehaviour
{
    public enum VFX_Type { None = -1, Dust ,Big_Dust, Biggest_Dust}

    private static VFXSpawner instance;

    public MMF_Player[] feedbacks;
    // Use this for initialization
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public static void SpawnVFX(VFX_Type type,Vector3 pos)
    {
        if(type == VFX_Type.None)
        {
            return;
        }
        MMF_ParticlesInstantiation vfx = instance.feedbacks[(int)type].GetFeedbackOfType<MMF_ParticlesInstantiation>();
        vfx.TargetWorldPosition = pos;
        instance.feedbacks[(int)type].PlayFeedbacks();
    }
        
}
