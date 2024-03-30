using MalbersAnimations.Controller;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable_StarNPC : Interactable
{
    public Item[] requiredItems;
    public int[] amounts;
    public NetworkObject networkObject;
    public override void ServerInteraction(ulong id)
    {
        // Open UI to Trade Items
    }

    public void Despawn()
    {
        networkObject.Despawn(true);
    }


}
