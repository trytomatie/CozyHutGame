using MalbersAnimations.Controller;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : NetworkBehaviour
{
    
    public UnityEvent<GameObject> localInteractionEvent;
    public UnityEvent serverInteractionEvent;
    public UnityEvent focusEnter;
    public UnityEvent focusExit;

    private GameObject source;
    private const float serverRequestTimeDelay = 1;
    private float serverRequestTimer = 0;

    private static List<Interactable> interactablesInRange = new List<Interactable>();
    public void Interact()
    {
        // If there is no source, just assume the LocalClient Player did it
        if (source == null)
            source = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        print("I work)");
        LocalInteraction(source);
        ServerInteraction(source);
    }

    public virtual void LocalInteraction(GameObject source)
    {
        localInteractionEvent.Invoke(source);
        source.GetComponent<MAnimal>().Mode_Activate(4);
    }

    public virtual void ServerInteraction(GameObject source)
    {

    }

    public virtual void FocusInteraction()
    {

    }

    public virtual void FocusInteraction(MAnimal animal)
    {
        NetworkObject animalNetworkObject = animal.GetComponent<NetworkObject>()?? null;
        if(animalNetworkObject != null)
        {
            if(animalNetworkObject.IsLocalPlayer)
            {
                interactablesInRange.Add(this);
                focusEnter.Invoke();
            }
        }
    }

    public virtual void FocusInteractionExit(MAnimal animal)
    {
        NetworkObject animalNetworkObject = animal.GetComponent<NetworkObject>() ?? null;
        if (animalNetworkObject != null)
        {
            if (animalNetworkObject.IsLocalPlayer)
            {
                interactablesInRange.Remove(this);
                focusExit.Invoke();
            }
        }
    }

    public static Interactable GetCurrentInteractable(GameObject requester)
    {
        if(interactablesInRange.Count > 0)
        {
            return interactablesInRange.OrderBy(e => Vector3.Distance(requester.transform.position, e.transform.position)).First();
        }
        else
        {
            return null;
        }

    }

    protected bool NotTimeoutedByServer()
    {
        return serverRequestTimer < Time.time;
    }

    protected void SetServerTimeoutTimer()
    {
        serverRequestTimer = Time.time + serverRequestTimeDelay;
    }
}
