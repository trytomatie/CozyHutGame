using MalbersAnimations.Controller;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable_Chair : Interactable
{
    public virtual void DisableCollider(GameObject soruce)
    {
        soruce.GetComponent<Collider>().enabled = false;
    }

    public virtual void EnableCollider(GameObject soruce)
    {
        soruce.GetComponent<Collider>().enabled = true;
    }

    public override void LocalInteraction(GameObject source)
    {
        base.LocalInteraction(source);
        source.GetComponent<MAnimal>().Mode_Activate(4);
    }
}
