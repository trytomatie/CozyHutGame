using MalbersAnimations.Controller;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using static Item;

public class Interactable_Furnace : Interactable
{
    public Container container1;
    public Container container2;
    private float interactionDistance = 3;
    public RefiningRecipie[] recipies;
    private bool isRefining = false;
    public int refinmentTime = 0;
    public List<ulong> observerList = new List<ulong>();
    public UnityEvent<int> updateTimerEvent;
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

    public void AttemptRefinement()
    {
        print("Attempting Refinment!");
        RefiningRecipie refiningRecipie = CanRefine();
        if(refiningRecipie != null && !isRefining)
        {
            StartCoroutine(Refine(refiningRecipie));
        }
    }

    private RefiningRecipie CanRefine()
    {
        foreach(RefiningRecipie rp in recipies)
        {
            bool canRefine = CanRefineRecipie(rp);
            if(canRefine)
            {
                return rp;
            }
        }
        return null;
    }

    private bool CanRefineRecipie(RefiningRecipie rp)
    {
        bool canRefine = true;
        ItemData[] requiredItems = new ItemData[rp.requiredItems.Length];
        for (int i = 0; i < rp.requiredItems.Length; i++)
        {
            requiredItems[i] = new ItemData(rp.requiredItems[i].itemId, rp.requieredItemsCount[i]);
            if (requiredItems[i].stackSize > container1.GetAmmountOfItem(requiredItems[i].itemId))
            {
                canRefine = false;
            }
        }
        return canRefine;
    }

    private IEnumerator Refine(RefiningRecipie recipie)
    {
        isRefining = true;
        int timer = (int)recipie.refiningTime;
        while (timer > 0)
        {
            yield return new WaitForSeconds(1);
            if(!CanRefineRecipie(recipie))
            {
                isRefining = false;
            }
            if (!isRefining)
            {
                break;
            }
            timer--;
            UpdateRefinmentTimeClientRpc(timer, GameManager.GetClientRpcParams(observerList.ToArray()));
        }
        if (isRefining)
        {
            isRefining = false;
            ItemData[] requiredItems = new ItemData[recipie.requiredItems.Length];
            for (int i = 0; i < recipie.requiredItems.Length; i++)
            {
                requiredItems[i] = new ItemData(recipie.requiredItems[i].itemId, recipie.requieredItemsCount[i]);
                container1.RequestRemoveItemServerRpc(new ItemData(recipie.requiredItems[i].itemId, recipie.requieredItemsCount[i]));
            }
            container2.AddItemServerRpc(new ItemData(recipie.recepieRessult.itemId, 1));
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
        if(!observerList.Contains(id))
        {
            observerList.Add(id);
        }
        UpdateRefinmentTimeClientRpc(refinmentTime, GameManager.GetClientRpcParams(id));
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
        observerList.Remove(id);
    }

    [ClientRpc]
    public void UpdateRefinmentTimeClientRpc(int time, ClientRpcParams clientRpcParams = default)
    {
        refinmentTime = time;
        updateTimerEvent.Invoke(refinmentTime);
    }
}
