using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DamageObject : NetworkBehaviour
{
    public PlayerController source;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner) return;
        Invoke("DestroyTime",0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DestroyTime()
    {
        gameObject.SetActive(false);
        // GetComponent<NetworkObject>().Despawn(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.GetComponent<ResourceController>() != null)
        {
            other.GetComponent<ResourceController>().PlayFeedbackClientRpc();
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { source.OwnerClientId }
                }
            };
            TriggerCameraShakeClientRpc(clientRpcParams);
        }
    }
    [ClientRpc]
    private void TriggerCameraShakeClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log(source.OwnerClientId);
        source.cameraShakeFeedback.PlayFeedbacks();
        Debug.LogFormat("GameObject: {0}", gameObject.name);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSourceObjectServerRpc(NetworkObjectReference sourceReference)
    {
        SetSourceObjectClientRpc(sourceReference);
    }
    [ClientRpc]
    private void SetSourceObjectClientRpc(NetworkObjectReference sourceReference)
    {
        sourceReference.TryGet(out NetworkObject pc);
        PlayerController sourceFinal = pc.GetComponent<PlayerController>();
        source = sourceFinal;
    }
}
