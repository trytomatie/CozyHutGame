using MalbersAnimations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HandSetupForParenting : NetworkBehaviour
{
    public Transform followTransform;

    // Update is called once per frame
    void Update()
    {
        if(followTransform != null)
        {
            print(followTransform.position);
            transform.position = followTransform.position;
            transform.rotation = followTransform.rotation;
        }

    }

    [ClientRpc]
    public void SetFollowTransformClientRpc(NetworkBehaviourReference playerGo)
    {
        if (playerGo.TryGet<NetworkPlayerInit>(out NetworkPlayerInit playerRef))
        {
            followTransform = playerRef.GetComponent<MWeaponManager>().RightHand;
        }

    }
}
