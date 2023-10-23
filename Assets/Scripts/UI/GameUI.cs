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
        return;

        InteractionManager.Instance.InteractableLost += DismissInteractionUI;
    }

    private void OnDisable()
    {
        return;

        InteractionManager.Instance.InteractableLost -= DismissInteractionUI;
    }

    #region Interaction


    public void DismissInteractionUI(Interactable interactable)
    {
        interactionUI.gameObject.SetActive(false);
    }
    #endregion


    public static GameUI Instance { get => instance; }

}
