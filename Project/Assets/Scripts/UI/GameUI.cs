using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public TextMeshProUGUI woodText;
    public Canvas canvas;
    public GameObject pauseMenu;

    private static GameUI instance;

    public UnityEvent pauseEvent;
    public UnityEvent negativePauseEvent;



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


    public static GameUI Instance { get => instance; }

}
