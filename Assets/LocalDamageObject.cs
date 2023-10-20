using MalbersAnimations.Weapons;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LocalDamageObject : MonoBehaviour
{
    public MWeapon weapon;
    public GameObject sourceObject;
    public virtual void ApplyDamage(Transform other)
    {
        if (other.GetComponent<ResourceController>() != null)
        {
            var source = sourceObject.GetComponent<NetworkObject>();
            int damage = (int)weapon.MaxDamage;
            other.GetComponent<ResourceController>().hp.Value -= damage;
            other.GetComponent<ResourceController>().PlayFeedbackServerRpc(damage,source.OwnerClientId);
        }
    }
}
