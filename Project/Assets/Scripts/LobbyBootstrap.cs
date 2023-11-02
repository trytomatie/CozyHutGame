using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyBootstrap : MonoBehaviour
{
    public LobbyManager lobbyManager;
    public Button button;


    public void CreateLobbyBootstrap()
    {
        lobbyManager.CreateLobby(button);
    }

    public void StartGameBootstrap()
    {
        lobbyManager.StartGame();
    }


}
