using System.Collections;
using System.Collections.Generic;
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

    [Header("PickedColor Indictaor")]
    public Image eyeBrowColorIndicator;
    public Image hairColorIndicator;
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
        playerCustomization.OnUpdateCharacterApearence.AddListener(() => eyeBrowColorIndicator.color = UngodlyMethodToGetTheHairColor(playerCustomization.hairColorIndex));
    }

    // Yes, i am aware that this is not the best way to do this, but that's just how it's setup for now
    public Color UngodlyMethodToGetTheHairColor(int index)
    {
        Color result = Color.white;
        ToggleSelectionUI[] toggles = baseColorToggleContainer.GetComponentsInChildren<ToggleSelectionUI>();
        result = toggles[index].transform.Find("Color_").GetComponent<Image>().color;
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

    public void AssignToggleColorFunctionality(int changeValue)
    {
        ToggleSelectionUI[] toggles = baseColorToggleContainer.GetComponentsInChildren<ToggleSelectionUI>();
        pickerIcon.sprite = IconList[changeValue];
        SetUIState(4);
        for (int i = 0; i < toggles.Length; i++)
        {
            Color color = toggles[i].transform.Find("Color_").GetComponent<Image>().color;
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
}
