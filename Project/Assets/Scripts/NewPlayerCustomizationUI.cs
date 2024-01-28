using JetBrains.Annotations;
using MalbersAnimations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewPlayerCustomizationUI : MonoBehaviour
{
    public Animator mainAnimator;
    private int previousUIState = 0;
    public PlayerCustomization playerCustomization;
    [Header("Toggle Container")]
    public GameObject skinColorToggleContainer;
    public GameObject eyeBrowShapeToggleContainer;
    public GameObject hairToggleContainer;
    public GameObject hairColorToggleContainer;
    public GameObject lashesToggleContainer;
    public GameObject highLightToggleContainer;
    public GameObject mouthToggleContainer;

    [Header("Color Toggle Container")]
    public GameObject baseColorToggleContainer;
    public Image pickerIcon;
    public Sprite[] IconList;

    [Header("Eye Color Toggle Container")]
    public GameObject eyeColorToggleContainer;

    [Header("PickedColor Indictaor")]
    public Image eyeBrowColorIndicator;
    public Image hairColorIndicator;
    public Image lashesColorIndicator;
    public Image pupilColorIndicator;
    public Image irisColorIndicator;
    public Image highLightColorIndicator;

    [Header("Cloth Tab")]
    public GameObject ccSelectorPrefab;
    public GameObject torsoContainer;
    public GameObject legsContainer;
    public GameObject feetContainer;

    [Header("Name Input")]
    public TMP_InputField nameInputField;
    // Singelton
    public static NewPlayerCustomizationUI instance;
    public void Start()
    {
        // Singelton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        AssignToggleFunctionality(skinColorToggleContainer.GetComponentsInChildren<ToggleSelectionUI>(), 8);
        AssignToggleFunctionality(eyeBrowShapeToggleContainer.GetComponentsInChildren<ToggleSelectionUI>(), 9);
        AssignToggleFunctionality(hairToggleContainer.GetComponentsInChildren<ToggleSelectionUI>(), 13);
        AssignToggleFunctionality(lashesToggleContainer.GetComponentsInChildren<ToggleSelectionUI>(), 11);
        AssignToggleFunctionality(highLightToggleContainer.GetComponentsInChildren<ToggleSelectionUI>(), 12);
        AssignToggleFunctionality(mouthToggleContainer.GetComponentsInChildren<ToggleSelectionUI>(), 10);
        AssignToggleFunctionality(hairColorToggleContainer.GetComponentsInChildren<ToggleSelectionUI>(), 14);
        playerCustomization.OnUpdateCharacterApearence.AddListener(() => eyeBrowColorIndicator.color = playerCustomization.eyebrowColor);
        playerCustomization.OnUpdateCharacterApearence.AddListener(() => hairColorIndicator.color = UngodlyMethodToGetTheHairColor(playerCustomization.hairColorIndex));
        playerCustomization.OnUpdateCharacterApearence.AddListener(() => irisColorIndicator.color = playerCustomization.irisColor);
        playerCustomization.OnUpdateCharacterApearence.AddListener(() => lashesColorIndicator.color = playerCustomization.eyelashColor);
        playerCustomization.OnUpdateCharacterApearence.AddListener(() => highLightColorIndicator.color = playerCustomization.highlightColor);
        playerCustomization.OnUpdateCharacterApearence.AddListener(() => pupilColorIndicator.color = playerCustomization.pupilColor);
        SetUpClothingTab();
    }

    // Yes, i am aware that this is not the best way to do this, but that's just how it's setup for now
    public Color UngodlyMethodToGetTheHairColor(int index)
    {
        Color result = Color.white;
        ToggleSelectionUI[] toggles = baseColorToggleContainer.GetComponentsInChildren<ToggleSelectionUI>();
        result = toggles[index].transform.ChildContainsName("Color_").GetComponent<Image>().color;
        return result;
    }

    private void AssignToggleFunctionality(ToggleSelectionUI[] toggles, int changeValue)
    {
        // Assign Toggle Functionality to each toggle in the array 
        for (int i = 0; i < toggles.Length; i++)
        {
            toggles[i].onToggleOn.RemoveAllListeners();
            if (toggles[i] != null)
            {
                int capturedIndex = i;
                toggles[i].onToggleOn.AddListener(() => playerCustomization.SetIndex(capturedIndex, changeValue));
            }
        }
    }

    private void SetUpClothingTab()
    {
        // Set up the containers
        if (playerCustomization != null)
        {
            // Torso
            SetupContainer(playerCustomization.torso, torsoContainer.transform, 0);
            // Legs
            SetupContainer(playerCustomization.legs, legsContainer.transform, 1);
            // Feet
            SetupContainer(playerCustomization.feet, feetContainer.transform, 2);
        }
    }

    private void SetupContainer(PlayerCustomizationAsset[] assets, Transform container, int type)
    {
        for (int i = 0; i < container.transform.childCount; i++)
        {
            Destroy(container.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < assets.Length; i++)
        {
            int capturedIndex = i;
            GameObject go = Instantiate(ccSelectorPrefab, container);
            go.transform.GetChild(0).GetComponent<Image>().sprite = assets[i].thumbnail;
            go.GetComponent<Toggle>().group = container.GetComponent<ToggleGroup>();
            go.GetComponent<ToggleSelectionUI>().onToggleOn.AddListener(() => playerCustomization.SetIndex(capturedIndex, type));
        }
    }

    public void AssignToggleColorFunctionality(int changeValue)
    {
        ToggleSelectionUI[] toggles;
        int uiStateToTransitionTo = 4;
        if (changeValue>= 3) // Base Colors
        {
            pickerIcon.sprite = IconList[changeValue];
            uiStateToTransitionTo = 4;
            toggles = baseColorToggleContainer.GetComponentsInChildren<ToggleSelectionUI>();
        }
        else // Eye Colors
        {
            toggles = eyeColorToggleContainer.GetComponentsInChildren<ToggleSelectionUI>();
            uiStateToTransitionTo = 6;
        }
        SetUIState(uiStateToTransitionTo);
        for (int i = 0; i < toggles.Length; i++)
        {
            Color color = toggles[i].transform.GetComponentInChildren<Image>().color;
            toggles[i].onToggleOn.RemoveAllListeners();
            switch (changeValue)
            {
                case 0: // irisColor
                    toggles[i].onToggleOn.AddListener(() => playerCustomization.irisColor = color);
                    break;
                case 1: // pupil
                    toggles[i].onToggleOn.AddListener(() => playerCustomization.pupilColor = color);
                    break;
                case 2: // Highlight
                    toggles[i].onToggleOn.AddListener(() => playerCustomization.highlightColor = color);
                    break;
                case 3: // Eyelash
                    toggles[i].onToggleOn.AddListener(() => playerCustomization.eyelashColor = color);
                    break;
                case 4: // Eyebrow
                    toggles[i].onToggleOn.AddListener(() => playerCustomization.eyebrowColor = color);
                    break;
            }
            toggles[i].onToggleOn.AddListener(() => playerCustomization.UpdatePlayerAppearance());
        }
    }

    public void SetUIState(int i)
    {
        previousUIState = mainAnimator.GetInteger("UIState");
        mainAnimator.SetInteger("UIState", i);
    }

    public void ReturnToPreivousUIState()
    {
        if(mainAnimator.GetInteger("UIState") == previousUIState)
        {
            SetUIState(0);
        }
        else
        {
            SetUIState(previousUIState);
        }
    }

    public void CreateNewPlayer()
    {
        string name = nameInputField.text;
        if (name != "" && !CheckForSymbols(name))
        {
            playerCustomization.playerName = name;
            GameManager.Instance.playerSaveData.CreatePlayerData(playerCustomization);
            LobbyUIManager.Instance.SetCharacterIndex(LobbyUIManager.Instance.FindCharacterIndex(name));
            LobbyUIManager.Instance.ChangeUIState(0);
        }
        else
        {
            string error = "Name must not be empty! Name may not contain special Symbols!";
            SystemMessageManagerUI.ShowSystemMessage(error);
            Debug.Log(error);
        }
    }

    private bool CheckForSymbols(string text)
    {
        bool result = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '!' || text[i] == '@' || text[i] == '#' || text[i] == '$' || text[i] == '%' || text[i] == '^' || text[i] == '&' || text[i] == '*' || text[i] == '(' || text[i] == ')' || text[i] == '-' || text[i] == '_' || text[i] == '+' || text[i] == '=' || text[i] == '[' || text[i] == ']' || text[i] == '{' || text[i] == '}' || text[i] == '|' || text[i] == '\\' || text[i] == ':' || text[i] == ';' || text[i] == '"' || text[i] == '\'' || text[i] == '<' || text[i] == '>' || text[i] == ',' || text[i] == '.' || text[i] == '?' || text[i] == '/')
            {
                result = true;
            }
        }
        return result;

    }
}
