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
    public static LobbyUIManager Instance;

    [Header("Lobby Creation Window")]
    public TextMeshProUGUI lobbyCreationUI_LobbyName;
    public TMP_Dropdown lobbyCreationUI_maxPlayers;
    public TMP_Dropdown lobbyCreationUI_world;

    [Header("Lobby Window")]
    [SerializeField] private Transform playerList;
    [SerializeField] private TextMeshProUGUI serverName;
    [SerializeField] private GameObject startButton;


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
    public PlayerCustomization playerVisual;
    public RawImage playerVisualImage;

    [Header("New World")]
    public TMP_InputField newWorldName;

    [Header("Loading World")]
    public GameObject worldTuple;
    public ToggleGroup toggleGroupContainer;
    public List<GameObject> loadedList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // Singelton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
        // Load Character Index that was last used
        characterIndex = PlayerPrefs.GetInt("CharacterIndex", 0);
        // Assign Button Events
        switchCharacterLeft.onClick.AddListener(SwitchCharacterLeft);
        switchCharacterRight.onClick.AddListener(SwitchCharacterRight);
        deleteCharacterButton.onClick.AddListener(DeleteCharacter);
        deleteCharacterButtonForDeletionWindow.onClick.AddListener(ShowDeleteCharacterWarningWindow);
        cancelDeleteCharacterButton.onClick.AddListener(HideDeleteCharacterWarningWindow);
        ChangeUIState(0);
        RefreshSavedPlayerDataInUI();
    }

    public void ChangeUIState(LobbyUIState i)
    {
        uiAnimator.SetInteger("lobbyState", (int)i);
    }

    public void ChangeUIState(int i)
    {
        if(characterNames.Length == 0)
        {
            // If the new State is new World, Join World, or Load World,
            if(i == 1 || i == 2|| i== 4)
            {
                SystemMessageManagerUI.ShowSystemMessage("You need to create a character first!");
                return;
            }
        }
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
    /// <summary>
    /// Depricated
    /// </summary>
    /// <param name="p_lobby"></param>
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
        //RefreshPlayerList(p_lobby);
        //serverName.text = p_lobby.Name;
       // SetLobbyWindowStartButton(p_lobby.HostId == AuthenticationService.Instance.PlayerId);
    }

    public void RefreshSavedPlayerDataInUI()
    {
        characterNames = GameManager.Instance.playerSaveData.FindSavedPlayerData().ToArray();
        if (characterNames.Length == 0)
        {
            playerVisualImage.enabled = false;
            return;
        }
        else
        {
            playerVisualImage.enabled = true;
        }
        characterName.text = characterNames[characterIndex];
        RefreshSavedPlayerUIElemetns();
    }

    private void RefreshSavedPlayerUIElemetns()
    {
        GameManager.Instance.selectedPlayer = characterNames[characterIndex];
        GameManager.Instance.LoadPlayerData(playerVisual);
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
        RefreshSavedPlayerUIElemetns();
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
        RefreshSavedPlayerUIElemetns();
    }

    public int FindCharacterIndex(string characterName)
    {
        RefreshSavedPlayerDataInUI();
        for (int i = 0; i < characterNames.Length; i++)
        {
            if (characterNames[i] == characterName)
            {
                return i;
            }
        }
        return 0;
    }

    public void SetCharacterIndex(int i)
    {
        characterIndex = i;
        characterName.text = characterNames[characterIndex];
        PlayerPrefs.SetInt("CharacterIndex", characterIndex);
        RefreshSavedPlayerUIElemetns();
    }

    public void ShowDeleteCharacterWarningWindow()
    {
        if(characterNames.Length == 0)
        {
            SystemMessageManagerUI.ShowSystemMessage("There is no Character to Delete");
            return;
        }
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
        characterIndex = 0;
        RefreshSavedPlayerDataInUI();
    }

    public void LoadSavedWorldsIntoUI()
    {
        // Clear the list
        if(loadedList != null)
        {
            foreach (GameObject tuple in loadedList)
            {
                Destroy(tuple);
            }
            loadedList.Clear();
        }
        
        // Load the list
        List<string> worlds = GameManager.Instance.worldSaveState.FindSavedWorlds();
        foreach (string world in worlds)
        {
            GameObject tuple = Instantiate(worldTuple, toggleGroupContainer.transform);
            tuple.GetComponentInChildren<TextMeshProUGUI>().text = world;
            tuple.GetComponent<Toggle>().group = toggleGroupContainer;
            loadedList.Add(tuple);
        }

        // Set the first one to be selected
        if(loadedList.Count > 0)
        {
            loadedList[0].GetComponent<Toggle>().isOn = true;
        }
    }
}
