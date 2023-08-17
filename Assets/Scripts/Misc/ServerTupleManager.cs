using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class ServerTupleManager: MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI serverName;
    [SerializeField] private TextMeshProUGUI playerCount;
    [SerializeField] private TextMeshProUGUI ping;
    [SerializeField] private Button buttonFunction;
    [SerializeField] private DoubleClickHandler doubleClickHandler;
    public Lobby lobby;

    private void Start()
    {
        buttonFunction.onClick.AddListener(SelectLobby);
        doubleClickHandler.doubleClickEvent.AddListener(LobbyManager.Instance.JoinSelectedLobby);
    }

    public void SetupTuple(Lobby p_lobby)
    {
        serverName.text = p_lobby.Name;
        playerCount.text = string.Format("{0} / {1}", p_lobby.Players.Count, p_lobby.MaxPlayers);
        lobby = p_lobby;
    }

    public void SelectLobby()
    {
        LobbyManager.selectedLobby = lobby;
    }
}
