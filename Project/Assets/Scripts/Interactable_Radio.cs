using CarterGames.Assets.AudioManager;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable_Radio : Interactable
{
    public NetworkVariable<int> songIndex = new NetworkVariable<int>(0);
    private AudioSource lastSource;
    public UnityEvent startPlaying;
    public UnityEvent stopPlaying;

    public override void OnNetworkSpawn()
    {
        songIndex.OnValueChanged += SetSong;
    }
   


    private void SetSong(int previousValue, int newValue)
    {
        if(lastSource != null)
        {
            lastSource.Stop();
        }

        switch(newValue)
        {
            case 0:
                // No Sound
                stopPlaying.Invoke();
                break;
            case 1:
                startPlaying.Invoke();
                lastSource = AudioManager.instance.PlayAtLocationAndGetSource("TheFeelsPLACEHOLDER", transform.position, GameManager.Instance.musicMixer, 1, 1);
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
