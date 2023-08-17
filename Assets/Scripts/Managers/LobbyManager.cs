using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private LobbyUIManager lobbyUI;

    private Lobby joinedLobby;
    private const float heartBeatTimer = 15;
    private const float pollRate = 1.1f;
    public static Lobby selectedLobby;

    private const string KEY_START_GAME = "KEY_START_GAME";

    // Start is called before the first frame update
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () => { Debug.Log("Has signed in" + AuthenticationService.Instance.PlayerId); };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        InvokeRepeating("InvokeHandleLobbyPollForUpdates", pollRate, pollRate);
    }

    public async void CreateLobby(Button p_button)
    {
        p_button.interactable = false;
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

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer,createLobbyOptions);

            joinedLobby = lobby;
            Debug.Log("Lobby Created:" + lobby.Name + " " + lobby.Id);
            lobbyUI.ChangeUIState(3);
            lobbyUI.ReloadLobbyUI(lobby);
            InvokeRepeating("InvokeHeartbeat", heartBeatTimer, heartBeatTimer);
        }
        catch(LobbyServiceException e)
        {
            Debug.LogException(e);
            
        }
        finally
        {
            p_button.interactable = true;
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
        return new Player(AuthenticationService.Instance.PlayerId)
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, lobbyUI.playerName.text) }
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

    private async Task<string> CreateRelay(Lobby p_lobby)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(p_lobby.MaxPlayers - 1);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData
            (
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
            return joinCode;
        }
        catch(RelayServiceException e)
        {
            Debug.LogError(e);
        }
        return "0";

    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
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

    public async void StartGame()
    {
        if(IsLobbyHost())
        {
            Debug.Log("Starting Game");

            string relayCode = await CreateRelay(joinedLobby);

            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_START_GAME,new DataObject(DataObject.VisibilityOptions.Member,relayCode) }
                }
            });
            NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            joinedLobby = lobby;
        }
    }

}
