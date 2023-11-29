using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable_DroppedItem : Interactable
{
    public NetworkVariable<ulong> claimedByPlayerId = new NetworkVariable<ulong>(9999,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public ulong droppedBy_clientId = 0;
    public NetworkVariable<ulong> itemDropId = new NetworkVariable<ulong>(9999,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public int stackSize = 0;
    public GameObject target;
    public ParticleSystemRenderer materialVisual;
    public GameObject itemParticlesSystem;
    public GameObject spawnParticleSystem;
    public bool hasSpawned = false;
    public LayerMask raycastLayerMask;
    public Rigidbody rb;
    private float spawnTimer;

    private void Start()
    {

    }

    [ClientRpc]
    public void SpawnParametersClientRpc(Vector3 direction, float force,ulong imgId)
    {
        spawnTimer = Time.time;
        rb.velocity = direction * force;
        Material newMaterial = materialVisual.material;
        newMaterial.SetTexture("_MainTex", ItemManager.GenerateItem(imgId).sprite.texture);
        materialVisual.material = newMaterial;
    }

    public override void OnNetworkSpawn()
    {
        claimedByPlayerId.Value = 9999;
        claimedByPlayerId.OnValueChanged += SetTargetId;
        itemDropId.OnValueChanged = SetItemIcon;
    }

    public override void ServerInteraction(GameObject source)
    {
        GiveToPlayerServerRpc(GetSourceClientId(source));
    }

    public override void FocusInteraction()
    {
        GameObject source = null;
        if(source == null)
        {
            source = NetworkManager.LocalClient.PlayerObject.gameObject;
        }
        if(hasSpawned)
        {
            GiveToPlayerServerRpc(GetSourceClientId(source));
        }

    }

    public ulong  GetSourceClientId(GameObject source)
    {
        return source.GetComponent<NetworkObject>().OwnerClientId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void GiveToPlayerServerRpc(ulong clientId)
    {
        if(claimedByPlayerId.Value == 9999)
        {
            claimedByPlayerId.Value = clientId;
        }

    }

    protected virtual void SetTargetId(ulong previousValue, ulong newValue)
    {
        if(newValue != 9999)
        {
            if(GameManager.Instance.playerList.ContainsKey(newValue))
            {
                target = GameManager.Instance.playerList[newValue];
            }

        }

    }

    protected virtual void SetItemIcon(ulong previousValue, ulong newValue)
    {

    }

    public void Update()
    {
        if(claimedByPlayerId.Value != 9999 && target != null && hasSpawned)
        {
            transform.parent.transform.position += (target.transform.position + new Vector3(0,0.5f,0)- transform.position).normalized * Time.deltaTime * 10;
            float distance = Vector3.Distance(transform.parent.position, target.transform.position + new Vector3(0, 0.5f, 0));
            if(distance < 0.1f)
            {
                DespawnItemServerRpc();
            }
        }
        if(!hasSpawned)
        {
            if(spawnTimer + 0.25f < Time.time && Physics.Raycast(transform.position,Vector3.down, 0.6f,raycastLayerMask))
            {
                hasSpawned = true;
                itemParticlesSystem.SetActive(true);
                spawnParticleSystem.SetActive(false);
                rb.isKinematic = true;
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void DespawnItemServerRpc()
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { claimedByPlayerId.Value }
            }
        };
        target.GetComponent<Container>().AddItemServerRpc(new Item.ItemData(itemDropId.Value,stackSize));
        transform.parent.gameObject.GetComponent<NetworkObject>().Despawn(true);
        Destroy(transform.parent.gameObject);
    }

    [ClientRpc]
    private void DisablePickupClientRpc()
    {
        enabled = false;
    }
}
