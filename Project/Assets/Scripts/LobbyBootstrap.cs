using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyBootstrap : MonoBehaviour
{
    public LobbyManager lobbyManager;
    public Button button;


    public void CreateLobbyBootstrap()
    {
        lobbyManager.sceneToLoad = SceneManager.GetAllScenes()[0].name;
        lobbyManager.CreateLobby(button);
    }

    public void StartGameBootstrap()
    {
        lobbyManager.StartGame();
    }


}
