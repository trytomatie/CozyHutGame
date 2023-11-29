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
    public UnityEvent respawnEvent;
    public bool needWeaknessForEffectiveDamage = true;
    public bool canRespawn = false;
    private float respawnTimer = 300;
    public StatElement weakness;
    public int maxHp;
    public NetworkVariable<int> hp = new NetworkVariable<int>(100,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

    public VFXSpawner.VFX_Type spawnVFX = VFXSpawner.VFX_Type.None;
    public VFXSpawner.VFX_Type deathVFX = VFXSpawner.VFX_Type.None;

    public override void OnNetworkSpawn()
    {
        hp.OnValueChanged += OnDeath;
    }

    protected virtual void OnDeath(int previousValue, int newValue)
    {
        if(newValue <= 0)
        {
            deathEvent.Invoke();
            VFXSpawner.SpawnVFX(deathVFX, transform.position);
            if(IsServer && canRespawn)
            {
                Invoke("Respawn", respawnTimer);
            }
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
        VFXSpawner.SpawnVFX(spawnVFX, transform.position);
        damageFeedback.StopFeedbacks();
        damageFeedback.PlayFeedbacks();
    }

    public void PlayFeedback(int dmg)
    {
        MMF_FloatingText floatingText = damageFeedback.GetFeedbackOfType<MMF_FloatingText>();
        floatingText.Value = string.Format("{0}", dmg);

        floatingText.AnimateColorGradient = GameManager.Instance.myColor;

        VFXSpawner.SpawnVFX(spawnVFX, transform.position);
        damageFeedback.StopFeedbacks();
        damageFeedback.PlayFeedbacks();
    }

    public void Respawn()
    {
        hp.Value = maxHp;
        RespawnClientRpc();
    }

    [ClientRpc]
    public void RespawnClientRpc()
    {
        respawnEvent.Invoke();
    }




}
