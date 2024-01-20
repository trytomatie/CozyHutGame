using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private Animator uiAnimator;
    public enum LobbyUIState { Base,CreateLobby,ServerList,InLobby};
    [SerializeField] private Transform serverList;

    [Header("Lobby Creation Window")]
    public TextMeshProUGUI lobbyCreationUI_LobbyName;
    public TMP_Dropdown lobbyCreationUI_maxPlayers;
    public TMP_Dropdown lobbyCreationUI_world;

    [Header("Lobby Window")]
    [SerializeField] private Transform playerList;
    [SerializeField] private TextMeshProUGUI serverName;
    [SerializeField] private GameObject startButton;

    [Header("Lobby Menu")]
    public TMP_InputField playerName;

    [Header("Prefabs")]
    [SerializeField] private GameObject serverTuple;
    [SerializeField] private GameObject playerTuple;

    [Header("Character Selection")]
    public TextMeshProUGUI characterName;
    public Button switchCharacterLeft;
    public Button switchCharacterRight;
    public string[] characterNames;
    public int characterIndex = 0;
    public GameObject deleteCharacterWarningWindow;
    public Button deleteCharacterButtonForDeletionWindow;
    public Button deleteCharacterButton;
    public Button cancelDeleteCharacterButton;

    // Start is called before the first frame update
    void Start()
    {
        // Load Character Index that was last used
        characterIndex = PlayerPrefs.GetInt("CharacterIndex", 0);
        // Assign Button Events
        switchCharacterLeft.onClick.AddListener(SwitchCharacterLeft);
        switchCharacterRight.onClick.AddListener(SwitchCharacterRight);
        deleteCharacterButton.onClick.AddListener(DeleteCharacter);
        deleteCharacterButtonForDeletionWindow.onClick.AddListener(ShowDeleteCharacterWarningWindow);
        cancelDeleteCharacterButton.onClick.AddListener(HideDeleteCharacterWarningWindow);
        ChangeUIState(0);
        playerName.text = "Tindangle" + UnityEngine.Random.Range(1000, 10000);
        SetWorldDropdown();
        RefreshSavedPlayerDataInUI();
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

    public void SetWorldDropdown()
    {
        List<string> options = new List<string>();
        options.Add("New World...");
        options.AddRange(GameManager.Instance.worldSaveState.FindSavedWorlds());
        lobbyCreationUI_world.ClearOptions();
        lobbyCreationUI_world.AddOptions(options);
        
    }

    public void RefreshSavedPlayerDataInUI()
    {
        characterNames = GameManager.Instance.playerSaveData.FindSavedPlayerData().ToArray();
        characterName.text = characterNames[characterIndex];
    }

    public void SwitchCharacterLeft()
    {
        characterIndex--;
        if (characterIndex < 0)
        {
            characterIndex = characterNames.Length - 1;
        }
        characterName.text = characterNames[characterIndex];
        PlayerPrefs.SetInt("CharacterIndex", characterIndex);
    }

    public void SwitchCharacterRight()
    {
        characterIndex++;
        if (characterIndex >= characterNames.Length)
        {
            characterIndex = 0;
        }
        characterName.text = characterNames[characterIndex];
        PlayerPrefs.SetInt("CharacterIndex", characterIndex);
    }

    public void ShowDeleteCharacterWarningWindow()
    {
        deleteCharacterWarningWindow.SetActive(true);
    }

    public void HideDeleteCharacterWarningWindow()
    {
        deleteCharacterWarningWindow.SetActive(false);
    }

    public void DeleteCharacter()
    {
        GameManager.Instance.playerSaveData.DeletePlayerData(characterName.text);
        HideDeleteCharacterWarningWindow();
        RefreshSavedPlayerDataInUI();
    }
}
