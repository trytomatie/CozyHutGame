using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ResourceController : NetworkBehaviour
{
    public Item itemDrop;
    [SerializeField] private MMF_Player damageFeedback;
    public UnityEvent deathEvent;

    public NetworkVariable<int> hp = new NetworkVariable<int>(100,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        hp.OnValueChanged += OnDeath;
    }

    protected virtual void OnDeath(int previousValue, int newValue)
    {
        if(newValue <= 0)
        {
            deathEvent.Invoke();
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
            source.GetComponent<Inventory>().AddItemClientRPC(itemDrop.itemId, dmg, clientRpcParams);
            PlayFeedbackClientRpc(dmg, sourceId);
        }

    }
    [ClientRpc]
    private void PlayFeedbackClientRpc(int dmg, ulong sourceId)
    {
        MMF_FloatingText floatingText = damageFeedback.GetFeedbackOfType<MMF_FloatingText>();
        floatingText.Value = string.Format( "+{0} {1}",dmg,itemDrop.itemName);
        if(NetworkManager.LocalClientId == sourceId)
        {
            floatingText.AnimateColorGradient = GameManager.Instance.myColor;
        }
        else
        {
            floatingText.AnimateColorGradient = GameManager.Instance.otherPlayerColor;
        }
        damageFeedback.StopFeedbacks();
        damageFeedback.PlayFeedbacks();

    }


}
