using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Interactable_Radio : Interactable
{
    public NetworkVariable<int> songIndex = new NetworkVariable<int>(0);

    public override void OnNetworkSpawn()
    {
        songIndex.OnValueChanged += SetSong;
    }


    private void SetSong(int previousValue, int newValue)
    {
        switch(newValue)
        {
            case 0:
                break;
            case 1:
                break;
        }
    }


    public override void ServerInteraction(ulong id)
    {
        if(songIndex.Value == 0)
        {
            songIndex.Value = 1;
        }
        else
        {
            songIndex.Value = 0;
        }
    }


}
