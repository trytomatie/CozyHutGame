using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private Animator uiAnimator;
    public enum LobbyUIState { Base,CreateLobby,ServerList,InLobby};
    [SerializeField] private Transform serverList;

    [Header("Lobby Creation Window")]
    public TextMeshProUGUI lobbyCreationUI_LobbyName;
    public TMP_Dropdown lobbyCreationUI_maxPlayers;

    [Header("Lobby Window")]
    [SerializeField] private Transform playerList;
    [SerializeField] private TextMeshProUGUI serverName;
    [SerializeField] private GameObject startButton;

    [Header("Lobby Menu")]
    public TMP_InputField playerName;

    [Header("Prefabs")]
    [SerializeField] private GameObject serverTuple;
    [SerializeField] private GameObject playerTuple;

    // Start is called before the first frame update
    void Start()
    {
        ChangeUIState(0);
        playerName.text = "Tindangle" + UnityEngine.Random.Range(1000, 10000);
    }

    public void ChangeUIState(LobbyUIState i)
    {
        uiAnimator.SetInteger("lobbyState", (int)i);
    }

    public void ChangeUIState(int i)
    {
        uiAnimator.SetInteger("lobbyState", i);
    }

    public void SetRandomLobbyName()
    {
        lobbyCreationUI_LobbyName.text = "New Lobby " + UnityEngine.Random.Range(0, 100000);
    }

    public void SetLobbyWindowStartButton(bool value)
    {
        startButton.SetActive(value);
    }

    public void Exit()
    {
        Application.Quit(0);
    }



    public void RemoveTuples()
    {

       for(int i = 0; i < serverList.childCount; i++)
       {
            Destroy(serverList.GetChild(i).gameObject);
       }
    }

    public void CreateTuple(Lobby p_lobby)
    {
        GameObject tuple = Instantiate(serverTuple,serverList);
        tuple.GetComponent<ServerTupleManager>().SetupTuple(p_lobby);
    }

    public void RefreshPlayerList(Lobby p_lobby)
    {
        for (int i = 0; i < playerList.childCount; i++)
        {
            Destroy(playerList.GetChild(i).gameObject);
        }
        foreach (Player player in p_lobby.Players)
        {
            print(player.Data["PlayerName"].Value);
            GameObject tuple = Instantiate(playerTuple, playerList);
            tuple.GetComponent<PlayerTupleManager>().playerName.text = player.Data["PlayerName"].Value;
        }
    }

    public void ReloadLobbyUI(Lobby p_lobby)
    {
        RefreshPlayerList(p_lobby);
        serverName.text = p_lobby.Name;
        SetLobbyWindowStartButton(p_lobby.HostId == AuthenticationService.Instance.PlayerId);
    }
}
