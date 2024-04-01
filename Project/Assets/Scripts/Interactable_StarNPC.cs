using MalbersAnimations.Controller;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Interactable_StarNPC : Interactable
{
    public Item requiredItem;
    public int amount;

    public int recievedAmount = 0;

    public Image itemImage;
    public TextMeshProUGUI itemAmount;

    private void Start()
    {
        if (IsServer) UpdateUIClientRpc();
    }

    public override void ServerInteraction(ulong id)
    {
        // Check if the player has the required items
        if (GameManager.GetLocalPlayer().GetComponent<NetworkPlayerInit>().inventory.GetAmmountOfItem(requiredItem.itemId) > 0)
        {
            int playerAmount = GameManager.GetLocalPlayer().GetComponent<NetworkPlayerInit>().inventory.GetAmmountOfItem(requiredItem.itemId);
            int amountToGive = Mathf.Clamp(playerAmount,0,amount - recievedAmount);
            recievedAmount += amountToGive;
            GameManager.GetLocalPlayer().GetComponent<NetworkPlayerInit>().inventory.RequestRemoveItemServerRpc(new Item.ItemData(requiredItem.itemId, amountToGive));
            print("That MF had the Items!");
        }
        else
        {
            print(source.name + " didn't have the items");
        }
        UpdateUIClientRpc();
    }

    [ClientRpc]
    private void UpdateUIClientRpc()
    {
        itemImage.sprite = requiredItem.sprite;
        itemAmount.text = recievedAmount + "/" + amount;
    }



}
