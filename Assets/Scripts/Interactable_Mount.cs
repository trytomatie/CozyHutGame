using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;

public class Interactable_Mount : Interactable
{
    public Transform saddlePosition;
    public GameObject lastInteractor;
    public override void Interact(GameObject source)
    {
        source.GetComponent<FlyingMountController>().mount = GetComponent<CharacterController>();
        source.GetComponent<StateMachine>().ForceState(State.StateName.Mounted);
        source.GetComponent<CharacterController>().enabled = false;
        lastInteractor = source;
        MountUpServerRpc(source.GetComponent<NetworkObject>());
    }

    [ServerRpc (RequireOwnership = false)]
    public void MountUpServerRpc(NetworkObjectReference sourceReference)
    {
        sourceReference.TryGet(out NetworkObject pc);
        GetComponent<NetworkObject>().ChangeOwnership(pc.OwnerClientId);
        print(pc.GetComponent<NetworkObject>().OwnerClientId);
        pc.GetComponent<NetworkObject>().TrySetParent(gameObject, false);
        pc.GetComponent<ClientNetworkTransform>().InLocalSpace = true;
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { pc.OwnerClientId }
            }
        };
        SetMountPositionClientRPC(clientRpcParams);
    }

    [ClientRpc]
    public void SetMountPositionClientRPC(ClientRpcParams clientRpcParams = default)
    {
        lastInteractor.transform.localPosition = saddlePosition.localPosition;
        lastInteractor.transform.localEulerAngles = Vector3.zero;
    }
}
