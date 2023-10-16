using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;

public class Interactable_Mount : Interactable
{
    public override void Interact(GameObject source)
    {
        MountUpServerRpc(source.GetComponent<NetworkObject>());
    }

    [ServerRpc (RequireOwnership = false)]
    public void MountUpServerRpc(NetworkObjectReference sourceReference)
    {
        sourceReference.TryGet(out NetworkObject pc);
        pc.transform.parent = transform;
        pc.GetComponent<CharacterController>().enabled = false;
        pc.transform.localPosition = Vector3.zero;
        pc.GetComponent<CharacterController>().enabled = true;
        pc.GetComponent<ClientNetworkTransform>().InLocalSpace = true;

    }
}
