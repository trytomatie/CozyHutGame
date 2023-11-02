using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : NetworkBehaviour
{
    
    public UnityEvent localInteractionEvent;
    public UnityEvent serverInteractionEvent;

    private GameObject source;
    private const float serverRequestTimeDelay = 1;
    private float serverRequestTimer = 0;
    public void Interact()
    {
        print("Testerino");
        // If there is no source, just assume the LocalClient Player did it
        if (source == null)
            source = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        LocalInteraction(source);
        ServerInteraction(source);
    }

    public virtual void LocalInteraction(GameObject source)
    {

    }

    public virtual void ServerInteraction(GameObject source)
    {

    }

    public virtual void FocusInteraction(GameObject source)
    {

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
