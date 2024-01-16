using MalbersAnimations;
using MalbersAnimations.Events;
using MalbersAnimations.Weapons;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using static Item;

public class NetworkPlayerInit : NetworkBehaviour
{
    public MonoBehaviour[] componentsToDisable;
    public GameObject[] objectsToDelete;
    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;
    public GameObject playerSetupPrefab;
    public Interactable currentInteractable;
    public TextMeshProUGUI playerNameCard;
    public GameObject handPivotSetupObjectPrefab;
    [HideInInspector] public GameObject handPivot;
    [HideInInspector] public GameObject visual;
    [HideInInspector]public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>("T", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public Container inventory;
    public Container equipmentInventory;
    public ObserverCursor observerCursor;
    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += SetPlayerNameCard;
        playerName.Value = playerName.Value;
    }

    protected virtual void SetPlayerNameCard(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {

        playerNameCard.text = newValue.ToString();
        observerCursor.text.text = newValue.ToString();
    }

    [ServerRpc (RequireOwnership = false)]
    public void SetNameCardServerRpc(string name)
    {
        playerName.Value = name;
    }


    // Start is called before the first frame update
    void Start()
    {
        AddPlayerToPlayerList();

        if (!IsOwner)
        {
            // Is other Client
            foreach (MonoBehaviour component in componentsToDisable)
            {
                component.enabled = false;
            }

            foreach (GameObject go in objectsToDelete)
            {
                Destroy(go);
            }
            playerNameCard.text = playerName.Value.ToString();
            observerCursor.text.text = playerName.Value.ToString();
            // Disable the Collider so it doesn't desync on other clients???
            GetComponent<Collider>().enabled = false;
        }
        else
        {
            // Is Local Client
            foreach (GameObject go in objectsToActivate)
            {
                go.SetActive(true);
            }
            foreach (GameObject go in objectsToDeactivate)
            {
                go.SetActive(false);
            }
            GameManager.Instance.inputManager = GetComponent<MInput>();
            GameObject playerSetup = Instantiate(playerSetupPrefab);
            DontDestroyOnLoad(playerSetup);
            playerSetup.GetComponentInChildren<InventoryManagerUI>().SetSyncedInvetory(inventory);
            playerSetup.GetComponentInChildren<InventoryManagerUI>().SetSyncedEquipmentInventory(equipmentInventory);
            playerSetup.GetComponentInChildren<BuildManager>().playerInventory = inventory;
            inventory.AddToObserverListServerRpc(OwnerClientId);
            equipmentInventory.AddToObserverListServerRpc(OwnerClientId);
            inventory.addItemEvents.AddListener(GameManager.Instance.DiscoverItem);
            Collider col = GetComponent<Collider>();
            GameManager.Instance.LoadPlayerData(GetComponent<PlayerCustomization>(), inventory);
            SetNameCardServerRpc(GameManager.Instance.selectedPlayer);
        }
        SpawnHandPivotSetupServerRpc();

    }

    private void AddPlayerToPlayerList()
    {
        if (GameManager.Instance.playerList.ContainsKey(OwnerClientId)) // Overrides if the key is already taken (Means someone Disconnected with that id)
        {
            GameManager.Instance.playerList[OwnerClientId] = gameObject;
        }
        else // Creates if key does not Exist
        {
            GameManager.Instance.playerList.Add(OwnerClientId, gameObject);
        }
    }

    [ServerRpc (RequireOwnership =false)]
    public void SpawnHandPivotSetupServerRpc()
    {
        if (handPivot == null)
        {
            handPivot = Instantiate(handPivotSetupObjectPrefab);
            handPivot.GetComponent<NetworkObject>().Spawn(false);
        }
        handPivot.GetComponent<HandSetupForParenting>().SetFollowTransformClientRpc(this);
    }

    [ClientRpc]
    public void TeleportClientRpc(Vector3 postion, ClientRpcParams clientRpcParams = default)
    {
        transform.position = postion;
    }

    [ClientRpc]
    public void CallLoadingScreenClientRpc(ClientRpcParams clientRpcParams = default)
    {
        LoadingScreenManager.Instance.CallLoadingScreen();
    }
    [ClientRpc]
    public void DismissLoadingScreenClientRpc(ClientRpcParams clientRpcParams = default)
    {
        LoadingScreenManager.Instance.DismissLoadingScreen();
    }

    public virtual void EquipWeapon(bool value)
    {
        if(value)
        {
            //if (GetComponent<Inventory>().items[40] != null)
            //{
            //    SpawnServerVisualServerRpc(GetComponent<Inventory>().items[40].itemName);
            //    GameObject handProxy = Instantiate(GetComponent<Inventory>().items[40].handProxy);
            //    GetComponent<MWeaponManager>().Equip_External(handProxy);
            //}
        }
        else
        {
            GetComponent<MWeaponManager>().UnEquip();
            DestroyVisualServerRpc();
        }
    }

    public virtual void EquipWeapon(int i)
    {
        // Don't allow Equipment swapping during attac
        if (GetComponent<MWeaponManager>().IsAttacking) 
            return;

        EquipWeapon(false);
        if (i>=0)
        {
            if(equipmentInventory.items[i] != ItemData.Null)
            {
                SpawnServerVisualServerRpc(equipmentInventory.items[i].itemId);
                GameObject handProxy = Instantiate(ItemManager.GenerateItem(equipmentInventory.items[i]).handProxy);
                GetComponent<MWeaponManager>().Equip_External(handProxy);
            }

        }
        else
        {
            //GetComponent<MWeaponManager>().UnEquip();
            //DestroyVisualServerRpc();
        }
    }

    public void Interact()
    {
        Interactable interactable = Interactable.GetCurrentInteractable(gameObject) ?? null;
        if(interactable != null)
        {
            interactable.Interact();
            DisableInput(0.5f);
        }
    }

    private void DisableInput(float seconds)
    {
        GetComponent<MInput>().enabled = false;
        Invoke("EnableInput", seconds);
    }

    private void EnableInput()
    {
        GetComponent<MInput>().enabled = true;
    }

    [ServerRpc (RequireOwnership =false)]
    public void SpawnServerVisualServerRpc(ulong id)
    {
        visual = Instantiate(ItemManager.GenerateItem(id).serverHandProxy);
        visual.GetComponent<NetworkObject>().Spawn();
        visual.GetComponent<NetworkObject>().TrySetParent(handPivot.transform,false);
        visual.transform.localPosition = Vector3.zero + visual.GetComponent<MWeapon>().RightHandOffset.Position;
        visual.transform.localEulerAngles = Vector3.zero + visual.GetComponent<MWeapon>().RightHandOffset.Rotation;
    }


    [ServerRpc(RequireOwnership = false)]
    public void DestroyVisualServerRpc()
    {
        if(visual != null)
        {
            visual.GetComponent<NetworkObject>().Despawn(true);
            //GetComponent<MWeaponManager>().UnEquip();
        }

    }

    #region Interaction
    public void EndInteraction()
    {
        if (currentInteractable != null)
            currentInteractable.EndInteraction(gameObject);
    }
    #endregion







}
