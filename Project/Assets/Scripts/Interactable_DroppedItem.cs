using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable_DroppedItem : Interactable
{
    public NetworkVariable<ulong> claimedByPlayerId = new NetworkVariable<ulong>(9999,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public ulong droppedBy_clientId = 0;
    public Item itemData;
    public GameObject target;
    

    public override void OnNetworkSpawn()
    {
        claimedByPlayerId.Value = 9999;
        claimedByPlayerId.OnValueChanged += SetTargetId;
    }

    public override void ServerInteraction(GameObject source)
    {
        GiveToPlayerServerRpc(GetSourceClientId(source));
    }

    public override void FocusInteraction()
    {
        GameObject source = null;
        if(source == null)
        {
            source = NetworkManager.LocalClient.PlayerObject.gameObject;
        }
        GiveToPlayerServerRpc(GetSourceClientId(source));
    }

    public ulong  GetSourceClientId(GameObject source)
    {
        return source.GetComponent<NetworkObject>().OwnerClientId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void GiveToPlayerServerRpc(ulong clientId)
    {
        if(claimedByPlayerId.Value == 9999)
        {
            claimedByPlayerId.Value = clientId;
        }

    }

    protected virtual void SetTargetId(ulong previousValue, ulong newValue)
    {
        if(newValue != 9999)
        {
            if(GameManager.Instance.playerList.ContainsKey(newValue))
            {
                target = GameManager.Instance.playerList[newValue];
                transform.parent.GetComponent<Rigidbody>().isKinematic = true;
                transform.parent.gameObject.layer = LayerMask.NameToLayer("IgnoreCollision");
            }

        }

    }

    public void Update()
    {
        if(claimedByPlayerId.Value != 9999 && target != null)
        {
            transform.parent.transform.position += (target.transform.position + new Vector3(0,0.5f,0)- transform.position).normalized * Time.deltaTime * 5;
            float distance = Vector3.Distance(transform.parent.position, target.transform.position + new Vector3(0, 0.5f, 0));
            if(distance < 0.1f)
            {
                DespawnItemServerRpc();
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void DespawnItemServerRpc()
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { claimedByPlayerId.Value }
            }
        };
        target.GetComponent<Inventory>().AddItemClientRPC(itemData.itemId, itemData.stackSize, clientRpcParams);
        transform.parent.gameObject.GetComponent<NetworkObject>().Despawn(true);
        Destroy(transform.parent.gameObject);
    }

    [ClientRpc]
    private void DisablePickupClientRpc()
    {
        enabled = false;
    }
}
