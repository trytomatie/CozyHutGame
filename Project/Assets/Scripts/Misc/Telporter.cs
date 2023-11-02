using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class Telporter : NetworkBehaviour
{

    public Transform destination;
    // Use this for initialization
    void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && other.GetComponent<PlayerController>() != null) 
        {
            TeleportCLientRPC(other.GetComponent<PlayerController>().NetworkObject);
        }
    }

    [ClientRpc]
    private void TeleportCLientRPC(NetworkObjectReference playerRef)
    {
        playerRef.TryGet(out NetworkObject go);
        PlayerController pc = go.GetComponent<PlayerController>();
        pc.characterController.enabled = false;
        pc.transform.position = destination.position;
        pc.characterController.enabled = true;
    }
}
