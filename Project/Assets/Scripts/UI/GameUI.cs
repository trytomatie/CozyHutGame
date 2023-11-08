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

public class GameUI : MonoBehaviour
{

    public TextMeshProUGUI woodText;
    public Canvas canvas;
    public GameObject pauseMenu;

    private static GameUI instance;

    public UnityEvent pauseEvent;
    public UnityEvent negativePauseEvent;
    public MEvent equipmentEquipEvent;



    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            GameManager.Instance.gameUI = this;

        }
    }

    public virtual void ShowMouseCursor(bool value)
    {
        if (value)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void CallPauseMenu()
    {
        print("you called?");
        if(!pauseMenu.activeSelf)
        {
            if (InventoryManagerUI.Instance.inventoryUI.activeSelf)
            {
                InventoryManagerUI.Instance.inventoryUI.SetActive(false);
            }
            else
            {
                pauseEvent.Invoke();

            }
        }
        else
        {
            negativePauseEvent.Invoke();
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


    public static GameUI Instance { get => instance; }

}
