using MalbersAnimations.Weapons;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LocalDamageObject : MonoBehaviour
{
    public MWeapon weapon;
    public virtual void ApplyDamage(Transform other)
    {
        if (other.GetComponent<ResourceController>() != null)
        {
            var source = other.GetComponent<NetworkObject>();
            int damage = (int)weapon.MaxDamage;
            other.GetComponent<ResourceController>().hp.Value -= damage;
            other.GetComponent<ResourceController>().PlayFeedbackServerRpc(damage,source.OwnerClientId);

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { source.OwnerClientId }
                }
            };
            other.GetComponent<Inventory>().AddItemClientRPC(0, damage, clientRpcParams);
        }
    }
}
