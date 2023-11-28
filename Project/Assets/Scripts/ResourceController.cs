using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations;
using System.Linq;

public class ResourceController : NetworkBehaviour
{
    public Item itemDrop;
    [SerializeField] private MMF_Player damageFeedback;
    public UnityEvent deathEvent;
    public bool needWeaknessForEffectiveDamage = true;
    public StatElement weakness;
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
    public void PlayFeedbackServerRpc(int dmg,int statElementId,ulong sourceId)
    {
        if(hp.Value > 0)
        {
            if(needWeaknessForEffectiveDamage && weakness != null && weakness.ID == statElementId)
            {
                // Weakness is hit
            }
            else if(needWeaknessForEffectiveDamage)
            {
                // Weakness is not hit
                dmg = 1;
            }

            hp.Value -= dmg;
            var source = NetworkManager.Singleton.ConnectedClients[sourceId].PlayerObject;
            List<ulong> clientList = NetworkManager.ConnectedClientsIds.ToList();
            clientList.Remove(sourceId);
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = clientList.AsReadOnly()
                }
            };
            // source.GetComponent<Inventory>().AddItemClientRPC(itemDrop.itemId, dmg, clientRpcParams);
            GameManager.Instance.SpawnDroppedItemServerRpc(itemDrop.itemId, dmg,transform.position + new Vector3(0,1,0));
            print($"Damage: {dmg}");
            PlayFeedbackClientRpc(dmg, sourceId, clientRpcParams);


        }

    }
    [ClientRpc]
    private void PlayFeedbackClientRpc(int dmg, ulong sourceId,ClientRpcParams clientRpcParams = default)
    {
        MMF_FloatingText floatingText = damageFeedback.GetFeedbackOfType<MMF_FloatingText>();
        floatingText.Value = string.Format( "{0}",dmg);
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

    public void PlayFeedback(int dmg)
    {
        MMF_FloatingText floatingText = damageFeedback.GetFeedbackOfType<MMF_FloatingText>();
        floatingText.Value = string.Format("{0}", dmg);

        floatingText.AnimateColorGradient = GameManager.Instance.myColor;


        damageFeedback.StopFeedbacks();
        damageFeedback.PlayFeedbacks();
    }


}
