using MalbersAnimations.Controller;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable_Container: Interactable
{
    public Container container1;
    public Container container2;
    private float interactionDistance = 3;

    public override void CheckForExitCondition()
    {
        if(prolongedInteraction)
        {
            if(Vector3.Distance(source.transform.position,transform.position) > interactionDistance)
            {
                CancelInvoke("CheckForExitCondition");
                EndInteraction(source);
            }
        }
        else
        {
            CancelInvoke("CheckForExitCondition");
        }
    }

    public override void EndInteraction(GameObject source)
    {

        if(prolongedInteraction)
        {
            RemoveFromObservationList(source.GetComponent<NetworkObject>().OwnerClientId);
        }
        base.EndInteraction(source);
    }

    public override void ServerInteraction(ulong id)
    {
        AddToObservationList(id);
    }

    public override void LocalInteraction(GameObject source)
    {
        base.LocalInteraction(source);
    }

    public void AddToObservationList(ulong id)
    {
        if(container1 != null)
        {
            container1.AddToObserverListServerRpc(id);
        }
        if(container2 != null)
        {
            container2.AddToObserverListServerRpc(id);
        }
    }

    public void RemoveFromObservationList(ulong id)
    {
        if (container1 != null)
        {
            container1.RemoveFromObserverListServerRpc(id);
        }
        if (container2 != null)
        {
            container2.RemoveFromObserverListServerRpc(id);
        }
    }


}
