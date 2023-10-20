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
            int rnd = Random.Range(12, 24);
            other.GetComponent<ResourceController>().hp.Value -= rnd;
            //other.GetComponent<ResourceController>().PlayFeedbackClientRpc(rnd);
            GameManager.Instance.woodCounter.Value += rnd;

        }
    }
    [ClientRpc]
    private void TriggerCameraShakeClientRpc(ClientRpcParams clientRpcParams = default)
    {

        source.cameraShakeFeedback.PlayFeedbacks();

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
