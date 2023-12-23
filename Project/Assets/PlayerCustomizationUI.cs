using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerCustomizationUI : MonoBehaviour
{
    public PlayerCustomization customization;
    public Button[] torsoButtons;
    public Button[] legButtons;
    public Button[] feetButtons;
    public Button[] irisButtons;
    public Button[] pupilButtons;
    public Button[] highlightButtons;
    public Button[] eyelashButtons;
    public Button[] eyelashColorButtons;
    public Button[] eyebrowColorButtons;
    public Button[] skincolorButtons;
    public Button[] eyebrowButtons;
    public Button[] mouthButtons;
    public Button[] eyehighlightButtons;
    public void Start()
    {
        AssignButtonFunctions(torsoButtons, 0);
        AssignButtonFunctions(legButtons, 1);
        AssignButtonFunctions(feetButtons, 2);
        AssignButtonFunctionsColor(irisButtons, 3);
        AssignButtonFunctionsColor(pupilButtons, 4);
        AssignButtonFunctionsColor(highlightButtons, 5);
        AssignButtonFunctions(eyelashButtons, 11);
        AssignButtonFunctionsColor(eyelashColorButtons, 6);
        AssignButtonFunctionsColor(eyebrowColorButtons, 7);
        AssignButtonFunctions(skincolorButtons, 8);
        AssignButtonFunctions(eyebrowButtons, 9);
        AssignButtonFunctions(mouthButtons, 10);
        AssignButtonFunctions(eyehighlightButtons, 12);
    }

    public void AssignButtonFunctions(Button[] buttons,int index)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button != null)
            {
                int capturedIndex = i;
                button.onClick.AddListener(() => customization.SetIndex(capturedIndex, index));
            }
        }
    }

    public void AssignButtonFunctionsColor(Button[] buttons, int index)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button != null)
            {
                Image imageReference = button.GetComponent<Image>();
                button.onClick.AddListener(() => customization.SetColor(index, imageReference));
            }
        }
    }
}
