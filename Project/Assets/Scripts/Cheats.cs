using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class Cheats : NetworkBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Cheat_GiveResourcesDebug()
    {
        GameManager.Instance.playerList[NetworkManager.LocalClientId].GetComponent<NetworkPlayerInit>().inventory.AddItemServerRpc(new Item.ItemData(1, 300));
        GameManager.Instance.playerList[NetworkManager.LocalClientId].GetComponent<NetworkPlayerInit>().inventory.AddItemServerRpc(new Item.ItemData(2, 300));
    }
}
