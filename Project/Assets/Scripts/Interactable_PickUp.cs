using MalbersAnimations.Controller;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable_PickUp : Interactable
{
    public Item droppedItem;
    public int amount;
    public NetworkObject networkObject;
    public override void ServerInteraction(ulong id)
    {
        GameManager.Instance.SpawnDroppedItemServerRpc(droppedItem.itemId, amount, transform.position + new Vector3(0, 0.25f, 0), (Random.onUnitSphere + new Vector3(0, 2.75f, 0)).normalized,5);
        networkObject.gameObject.SetActive(false);
        SpawnVFXClientRpc();
        Invoke("Despawn", 10);
    }
    [ClientRpc]
    private void SpawnVFXClientRpc()
    {
        VFXSpawner.SpawnVFX(VFXSpawner.VFX_Type.Dust, transform.position);
    }

    public void Despawn()
    {
        networkObject.Despawn(true);
    }


}
