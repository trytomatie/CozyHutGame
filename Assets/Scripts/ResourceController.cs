using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ResourceController : NetworkBehaviour
{

    [SerializeField] private MMF_Player damageFeedback;

    public NetworkVariable<int> hp = new NetworkVariable<int>(100,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        hp.OnValueChanged += OnDeath;
    }

    protected virtual void OnDeath(int previousValue, int newValue)
    {
        if(newValue <= 0)
        {

        }
    }

    [ServerRpc (RequireOwnership =false)]
    public void PlayFeedbackServerRpc(int dmg,ulong sourceId)
    {
        if(hp.Value > 0)
        {
            hp.Value -= dmg;
            var source = NetworkManager.Singleton.ConnectedClients[sourceId].PlayerObject;
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { sourceId }
                }
            };
            source.GetComponent<Inventory>().AddItemClientRPC(0, dmg, clientRpcParams);
            PlayFeedbackClientRpc(dmg, sourceId);
        }

    }
    [ClientRpc]
    private void PlayFeedbackClientRpc(int dmg, ulong sourceId)
    {
        MMF_FloatingText floatingText = damageFeedback.GetFeedbackOfType<MMF_FloatingText>();
        floatingText.Value = "+" + dmg + " Wood";
        if(NetworkManager.LocalClientId == sourceId)
        {
            floatingText.DisplayColor = Color.green;
        }
        else
        {
            floatingText.DisplayColor = Color.white;
        }
        damageFeedback.StopFeedbacks();
        damageFeedback.PlayFeedbacks();

    }
}
