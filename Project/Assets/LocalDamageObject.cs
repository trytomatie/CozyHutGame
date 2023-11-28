using MalbersAnimations.Weapons;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LocalDamageObject : MonoBehaviour
{
    public MWeapon weapon;
    public GameObject sourceObject;

    public virtual void ApplyDamage(Collider other)
    {
        ApplyDamage(other.transform);
    }
    
    public virtual void ApplyDamage(Transform other)
    {
        print(other.name);
        print(NetworkManager.Singleton.LocalClientId + "   " + sourceObject.GetComponent<NetworkObject>().OwnerClientId);
        if(NetworkManager.Singleton.LocalClientId == sourceObject.GetComponent<NetworkObject>().OwnerClientId)
        {

        }
        if (other.GetComponent<ResourceController>() != null)
        {
            var source = sourceObject.GetComponent<NetworkObject>();
            int damage = (int)Random.Range(weapon.MinDamage,weapon.MaxDamage+1);
            int elementId = weapon.element?.ID ?? 0;
            ResourceController rc = other.GetComponent<ResourceController>();

            rc.PlayFeedbackServerRpc(damage,elementId,source.OwnerClientId);
            // Redundant, it also beeing calculated by the server, consider doing this only on the client
            if (rc.needWeaknessForEffectiveDamage && rc.weakness != null && rc.weakness.ID == elementId)
            {
                // Weakness is hit
            }
            else if (rc.needWeaknessForEffectiveDamage)
            {
                // Weakness is not hit
                damage = 1;
            }
            other.GetComponent<ResourceController>().PlayFeedback(damage);
        }
    }


    public virtual void SetWeaponHitbox(GameObject go)
    {
        print(go.name);
        if(go.GetComponent<MMelee>() != null)
        {
            weapon = go.GetComponent<MMelee>();
        }
    }
}
