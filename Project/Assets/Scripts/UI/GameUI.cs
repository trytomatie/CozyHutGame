using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MalbersAnimations;
using MalbersAnimations.Events;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{

    public TextMeshProUGUI woodText;
    public Canvas canvas;
    public GameObject pauseMenu;
    public GameObject craftingMenu;
    public GameObject inventoryMenu;
    public GameObject equipmentSelectionMenu;

    private static GameUI instance;

    public UnityEvent pauseEvent;
    public UnityEvent negativePauseEvent;
    public UnityEvent closeAllUIWindowsEvent;
    public MEvent equipmentEquipEvent;
    public UnityEvent<bool> showCursorEvent;


    [Header("Building Menu")]
    public TextMeshProUGUI gridSizeText;



    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            GameManager.Instance.gameUI = this;

        }
    }

    public void CloseAllUIWindows()
    {
        craftingMenu.SetActive(false);
        inventoryMenu.SetActive(false);
        equipmentSelectionMenu.SetActive(false);
        closeAllUIWindowsEvent.Invoke();
        ShowMouseCursor(false);
    }

    public bool InterfaceWindowIsOpen()
    {
        return craftingMenu.activeSelf || inventoryMenu.activeSelf || equipmentSelectionMenu.activeSelf;
    }

    public virtual void ShowMouseCursor(bool value)
    {
        if (value)
        {
            showCursorEvent.Invoke(true);
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            showCursorEvent.Invoke(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void CallPauseMenu()
    {
        if(InterfaceWindowIsOpen())
        {
            CloseAllUIWindows();
            ShowMouseCursor(false);
        }
        else if (!pauseMenu.activeSelf)
        {
            pauseEvent.Invoke();
            ShowMouseCursor(true);
        }
        else
        {
            negativePauseEvent.Invoke();
        }
    }

    public void QuitGame()
    {
        Application.Quit(0);
    }

    public void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
        GameManager.Instance.Disconnect();
        GameManager.Instance.LoadScene("LobbyScreen");
    }

    public void ToggleInventory()
    {
        if(inventoryMenu.activeSelf)
        {
            CloseAllUIWindows();
        }
        else
        {
            CloseAllUIWindows();
            inventoryMenu.SetActive(true);
            ShowMouseCursor(true);
        }
    }

    public void CheckHoveredEquipmentUI()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Get the pointer data
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            // Perform a raycast using the pointer data
            List<RaycastResult> result = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, result);

            // Check if a UI element was hit
            if (result[0].gameObject != null)
            {
                int i = -1;
                switch(result[0].gameObject.name)
                {
                    case "Image_00":
                        i = 0;
                        break;
                    case "Image_01":
                        i = 1;
                        break;
                    case "Image_02":
                        i = 2;
                        break;
                    case "Image_03":
                        i = 3;
                        break;
                }
                equipmentEquipEvent.Invoke(i);

                }
        }
    }
    

    #region BuildingMenu
    public void UpdateGridSizeText()
    {
        gridSizeText.text = "(K) Grid Size: " +  BuildManager.Instance.gridSize.ToString("F2");
    }
    #endregion

    public static GameUI Instance { get => instance; }

}
