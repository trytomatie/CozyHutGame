using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayerCustomizationUI : MonoBehaviour
{
    public Animator mainAnimator;
    public PlayerCustomization playerCustomization;
    [Header("Toggle Container")]
    public GameObject skinColorToggleContainer;
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
    }

    private void AssignToggleFunctionality(ToggleSelectionUI[] toggles, int changeValue)
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i] != null)
            {
                int capturedIndex = i;
                toggles[i].onToggleOn.AddListener(() => playerCustomization.SetIndex(capturedIndex, changeValue));
            }
        }
    }

    public void SetUIState(int i)
    {
        mainAnimator.SetInteger("UIState", i);
    }
}
