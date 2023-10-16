using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public TextMeshProUGUI woodText;
    public Canvas canvas;
    public TextMeshProUGUI interactionUI;
    private Vector3 interactionWorldPos;

    private Camera mainCamera;

    private static GameUI instance;




    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            mainCamera = Camera.main;
            GameManager.Instance.gameUI = this;
            woodText.text = "0 Wood";
        }
    }

    private void Update()
    {
        if(interactionUI.gameObject.activeSelf)
        {
            interactionUI.rectTransform.position = mainCamera.WorldToScreenPoint(interactionWorldPos);
        }
    }

    private void OnEnable()
    {
        InteractionManager.Instance.InteractableSet += PositionInteractionUI;
        InteractionManager.Instance.InteractableSet += SetInteractionText;
        InteractionManager.Instance.InteractableLost += DismissInteractionUI;
    }

    private void OnDisable()
    {
        InteractionManager.Instance.InteractableSet -= PositionInteractionUI;
        InteractionManager.Instance.InteractableSet -= SetInteractionText;
        InteractionManager.Instance.InteractableLost -= DismissInteractionUI;
    }

    #region Interaction
    public void PositionInteractionUI(Interactable interactable)
    {
        interactionUI.gameObject.SetActive(true);
        interactionWorldPos = interactable.transform.position + new Vector3(0, interactable.heightOffset, 0);
    }

    public void SetInteractionText(Interactable interactable)
    {
        // Parse out the Binded Key (e.g "<Keyboard>/s" returns "S")
        string input = InputManager.Instance.inputActions.Player.Interact.bindings[0].path;
        int startIndex = input.IndexOf("<Keyboard>/") + "<Keyboard>/".Length;
        string substring = input.Substring(startIndex);
        string capitalizedSubstring = char.ToUpper(substring[0]) + substring.Substring(1);

        interactionUI.text = string.Format("{0} [{1}]", interactable.interactionText, capitalizedSubstring);
    }

    public void DismissInteractionUI(Interactable interactable)
    {
        interactionUI.gameObject.SetActive(false);
    }
    #endregion


    public static GameUI Instance { get => instance; }

}
