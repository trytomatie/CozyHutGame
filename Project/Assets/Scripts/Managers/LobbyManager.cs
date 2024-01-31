using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Multiplayer.Samples.Utilities;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private LobbyUIManager lobbyUI;

    private Lobby joinedLobby;
    private const float heartBeatTimer = 15;
    private const float pollRate = 1.1f;
    public static Lobby selectedLobby;
    public string sceneToLoad = "PrototypeScene";
    
    bool lanGameIsStarting = false;
    public bool loadWorld = false;

    private const string KEY_START_GAME = "KEY_START_GAME";
    public UnityEvent afterStart;
    public UnityEvent afterLobbyCreation;
    public UnityTransport relayTransportProtocol;
    public UnityTransport lanTransportProtocol;

    [Header("LobbyCreation")]
    private int timeout = 0;
    private const int MAX_TIMEOUT = 20;


    // Start is called before the first frame update
    private async void Start()
    {
        await ConnectToRelayService();

    }

    private async Task ConnectToRelayService()
    {

        NetworkManager.Singleton.NetworkConfig.NetworkTransport = relayTransportProtocol;
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000000).ToString());
        await UnityServices.InitializeAsync(initializationOptions);
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignedIn += () => { Debug.Log("Has signed in" + AuthenticationService.Instance.PlayerId); };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();


        }
        InvokeRepeating("InvokeHandleLobbyPollForUpdates", pollRate, pollRate);
        afterStart.Invoke();

    }

    public async void CreateLobby(Button p_button)
    {
        CreateLobby();
    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = lobbyUI.lobbyCreationUI_LobbyName.text;
            int maxPlayer = lobbyUI.lobbyCreationUI_maxPlayers.value + 1;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayerData(),
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member,"0") }
                }
            };

            if (lobbyName == "")
            {
                lobbyName = "Lobby of" + createLobbyOptions.Player.Data["PlayerName"].Value;
            }

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer, createLobbyOptions);

            joinedLobby = lobby;
            Debug.Log("Lobby Created:" + lobby.Name + " " + lobby.Id);
            lobbyUI.ReloadLobbyUI(lobby);
            InvokeRepeating("InvokeHeartbeat", heartBeatTimer, heartBeatTimer);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);

        }
        finally
        {
            afterLobbyCreation.Invoke();
        }
    }

    private void InvokeHeartbeat()
    {
        HandleLobbyHeartBeat();
    }

    private void InvokeHandleLobbyPollForUpdates()
    {
        HandleLobbyPollForUpdates();
    }

    private async void HandleLobbyHeartBeat()
    {
        if(joinedLobby != null)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
        }
        else
        {
            CancelInvoke("InvokeHandleLobbyPollForUpdates");
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if(joinedLobby != null)
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            joinedLobby = lobby;
            lobbyUI.ReloadLobbyUI(joinedLobby);
            if(joinedLobby.Data[KEY_START_GAME].Value != "0")
            {
               
                if(!IsLobbyHost())
                {
                    JoinRelay(joinedLobby.Data[KEY_START_GAME].Value);
                }
                
                
                joinedLobby = null;
                CancelInvoke("HandleLobbyPollForUpdates");


            }
        }
    }

    private Player GetPlayerData()
    {
        GameManager.Instance.selectedPlayer = lobbyUI.characterNames[lobbyUI.characterIndex];
        return new Player(AuthenticationService.Instance.PlayerId)
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, GameManager.Instance.selectedPlayer) }
                    }
        };
    }

    public async void JoinSelectedLobby()
    {
        if(selectedLobby == null)
        {
            return;
        }
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdAsync = new JoinLobbyByIdOptions
            {
                Player = GetPlayerData()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(selectedLobby.Id, joinLobbyByIdAsync);
            lobbyUI.ChangeUIState(3);
            lobbyUI.ReloadLobbyUI(lobby);
            joinedLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    /// <summary>
    /// In Summary, Start the Game for the Server and Host
    /// </summary>
    /// <param name="p_lobby"></param>
    /// <returns></returns>
    private async Task<string> CreateRelay(Lobby p_lobby)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(p_lobby.MaxPlayers - 1);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            GameManager.Instance.relayCode = joinCode;
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData
            (
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
            NetworkManager.Singleton.StartHost();
            LoadingScreenManager.Instance.CallLoadingScreen();
            return joinCode;
        }
        catch(RelayServiceException e)
        {
            Debug.LogError(e);
        }
        return "0";

    }

    /// <summary>
    /// In Sumary, Starts the Game for the Clients
    /// </summary>
    /// <param name="joinCode"></param>
    private async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            GetPlayerData();
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );
            GameManager.Instance.relayCode = joinCode;
            NetworkManager.Singleton.StartClient();
            LoadingScreenManager.Instance.CallLoadingScreen();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public void JoinRelayWithCode(TMP_InputField joinCode)
    {
        JoinRelay(joinCode.text);
    }

    public async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                lobbyUI.CreateTuple(lobby);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

    }

    public void RefreshServerList()
    {
        lobbyUI.RemoveTuples();
        ListLobbies();
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            lobbyUI.ChangeUIState(LobbyUIManager.LobbyUIState.ServerList);
            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private bool IsLobbyHost()
    {
        if(joinedLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void StartGame()
    {
        if(timeout == 0)
        {
            StartCoroutine(StartGameAsync());
        }

    }


    public IEnumerator StartGameAsync()
    {
        LoadingScreenManager.Instance.CallConnectionLoadingScreen();
        CreateLobby();
        timeout = 0;
        while (joinedLobby == null)
        {
            yield return new WaitForSeconds(0.5f);
            timeout++;
            if(timeout == MAX_TIMEOUT)
            {
                timeout = 0;
                Debug.LogError("Lobby Creation Timed Out (10 Seconds)");
                break;
            }
        }
        timeout = 0;
        if (joinedLobby != null)
        {
            StartGameWithCreatedLobby();
        }

    }

    private async void StartGameWithCreatedLobby()
    {
        if (IsLobbyHost())
        {
            string relayCode = await CreateRelay(joinedLobby);
            Debug.Log("Starting Game: " + relayCode);
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_START_GAME,new DataObject(DataObject.VisibilityOptions.Member,relayCode) }
                }
            });
            LoadingScreenManager.Instance.DismissConnectionLoadingScreen();
            CancelInvoke("InvokeHandleLobbyPollForUpdates");
            Invoke("ChangeScene", 10);
            joinedLobby = lobby;

        }
    }


    private void LoadNewScene(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete-=LoadNewScene;
    }



    public void SpawnPlayerInWorld(SceneEvent sceneEvent)
    {
        var clientOrServer = sceneEvent.ClientId == NetworkManager.ServerClientId ? "server" : "client";
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { sceneEvent.ClientId }
            }
        };
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.Load:
                break;
            case SceneEventType.LoadComplete:
                {
                    Debug.Log($"Loaded the {sceneEvent.SceneName} scene on " +
                        $"{clientOrServer}-({sceneEvent.ClientId}).");

                    NetworkManager.Singleton.ConnectedClients[sceneEvent.ClientId]
                        .PlayerObject.GetComponent<NetworkPlayerInit>().
                        TeleportClientRpc(FindObjectOfType<SpawnPlayerBootstrap>(true).transform.position,clientRpcParams);
                    NetworkManager.Singleton.ConnectedClients[sceneEvent.ClientId]
.PlayerObject.GetComponent<NetworkPlayerInit>().DismissLoadingScreenClientRpc();
                    if (clientOrServer == "server")
                    {
                        if(loadWorld)
                        {
                            GameManager.Instance.worldSaveState.LoadWorld();
                        }
                    }
                    break;
                }
        }
    }

    public void LoadWorld(SceneEvent sceneEvent)
    {
        var clientOrServer = sceneEvent.ClientId == NetworkManager.ServerClientId ? "server" : "client";
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.Load:
                break;
            case SceneEventType.LoadComplete:
                {
                    Debug.Log($"Loaded the {sceneEvent.SceneName} scene on " + $"{clientOrServer}-({sceneEvent.ClientId}).");
                    if (clientOrServer == "server")
                    {
                        GameManager.Instance.worldSaveState.LoadWorld();
                    }
                    if(clientOrServer == "client")
                    {
                        print("SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSCLIENT"!);
                        GameManager.Instance.worldSaveState.RemoveResorucesFromInitialScene();
                    }
                    break;
                }
        }
    }

    public void ChangeScene()
    {

        NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        NetworkManager.Singleton.SceneManager.OnSceneEvent += SpawnPlayerInWorld;
    }
    public void StartLanGame()
    {
        LoadingScreenManager.Instance.CallLoadingScreen();
        Invoke("LanGame", 4);
    }

    private void LanGame()
    {
        if(lanGameIsStarting)
        {
            return;
        }
        lanGameIsStarting = true;
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = lanTransportProtocol;
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, UnityEngine.SceneManagement.LoadSceneMode.Single);
        NetworkManager.Singleton.SceneManager.OnSceneEvent += SpawnPlayerInWorld;
        String name = NetworkManager.Singleton.name;
    }

    public void StartBootstarpGame()
    {
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = lanTransportProtocol;
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.OnSceneEvent += SpawnPlayerInWorld;
        String name = NetworkManager.Singleton.name;



        // TODO: Get the Save Game Screen to select world, Also Select Character First
    }

    public void SetNewWorldSaveName()
    {
        GameManager.Instance.worldSaveState.worldName = lobbyUI.newWorldName.text;
    }

    public void SetWorldSaveToBeLoaded()
    {
        // Set the World Save to be loaded
        foreach(GameObject worldTuple in lobbyUI.loadedList)
        {
            if(worldTuple.GetComponent<Toggle>().isOn)
            {
                GameManager.Instance.worldSaveState.worldName = worldTuple.GetComponentInChildren<TextMeshProUGUI>().text;
                loadWorld = true;
                break;
            }
        }

    }

}
