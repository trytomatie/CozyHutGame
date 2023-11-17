using MalbersAnimations.Controller;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : NetworkBehaviour
{
    private const ulong maxValue = 99999999;
    public bool canBeOccupied = false;
    public NetworkVariable<ulong> interactorId = new NetworkVariable<ulong>(maxValue);

    public UnityEvent<GameObject> localInteractionEvent;
    public UnityEvent<GameObject> endInteractionEvent;
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
        InteractServerRpc(source.GetComponent<NetworkObject>().OwnerClientId);

    }

    private void OnServerInitialized()
    {

    }

    private void Start()
    {
        if(IsServer)
            interactorId.Value = maxValue;
    }
    public override void OnNetworkSpawn()
    {
        interactorId.OnValueChanged += OnInteracotIdChanged;
    }

    private void OnInteracotIdChanged(ulong oldValue,ulong newValue)
    {
        if(canBeOccupied && newValue != maxValue)
        {
            FocusInteractionExit(NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<MAnimal>() ?? null);
        }
    }


    public virtual void LocalInteraction(GameObject source)
    {
        localInteractionEvent.Invoke(source);
    }

    public virtual void ServerInteraction(GameObject source)
    {

    }

    public virtual void FocusInteraction()
    {

    }

    public virtual void FocusInteraction(MAnimal animal)
    {
        if(canBeOccupied && interactorId.Value != maxValue)
        {
            return;
        }
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

    [ServerRpc (RequireOwnership =false)]
    private void InteractServerRpc(ulong id)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { id }
            }
        };

        if(canBeOccupied)
        {
            if (interactorId.Value == maxValue) // means it's not occupied
            {
                interactorId.Value = id;
                InteractClientRpc(clientRpcParams);
            }
        }
        else
        {
            InteractClientRpc(clientRpcParams);
        }
    }

    [ClientRpc]
    private void InteractClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if(canBeOccupied)
        {
            source.GetComponent<NetworkPlayerInit>().currentInteractable = this;
        }
        LocalInteraction(source);
    }

    public void EndInteraction(GameObject source)
    {
        if(canBeOccupied)
        {
            source.GetComponent<NetworkPlayerInit>().currentInteractable = null;
            EndInteractionServerRpc(source.GetComponent<NetworkObject>().OwnerClientId);
            endInteractionEvent.Invoke(source);
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void EndInteractionServerRpc(ulong id)
    { 
        if(interactorId.Value == id)
        {
            interactorId.Value = maxValue;
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
