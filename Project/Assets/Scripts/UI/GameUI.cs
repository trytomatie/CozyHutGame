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
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

public class GameUI : MonoBehaviour
{
    public enum UI_State { Base,Inventory,Container,Building,Pause}
    private int baseStateOverride = 0;
    public TextMeshProUGUI timeText;
    public Canvas canvas;
    public GameObject pauseMenu;
    public GameObject craftingMenu;
    public GameObject inventoryMenu;
    public GameObject equipmentSelectionMenu;
    public GameObject buildingSelectionMenu;
    public bool showCursor = false;
    private bool previousCursorState = true;
    private Animator anim;
    private static GameUI instance;

    public UnityEvent pauseEvent;
    public UnityEvent negativePauseEvent;
    public UnityEvent closeAllUIWindowsEvent;
    public MEvent equipmentEquipEvent;
    public UnityEvent<bool> showCursorEvent;


    [Header("Building Menu")]
    public TextMeshProUGUI gridSizeText;

    [Header("Refinment Menu")]
    public TextMeshProUGUI refinmentTimer;
    public Image refinmentProgressBar;
    public MMProgressBar refinmentProgressbarFeedback;
    public GameObject refinementRecipiePanel;
    public GameObject refinementRecipieSlotPrefab;

    [Header("EquipmentSelection Menu")]
    public EquipmentSelector[] equipmentSelectors;

    [Header("Quest UI")]
    public TextMeshProUGUI questName;
    public TextMeshProUGUI questDescription;


    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (instance == null)
        {
            instance = this;
            GameManager.Instance.gameUI = this;
            QuestManager.Instance.UpdateQuestUI();
        }
    }

    private void Update()
    {
        if(showCursor != previousCursorState)
        {
            if (showCursor)
            {
                showCursorEvent.Invoke(true);
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                showCursorEvent.Invoke(false);
                Cursor.lockState = CursorLockMode.Locked;
            }
            previousCursorState = showCursor;
        }
    }

    public void SetShowCursor(bool value)
    {
        showCursor = value;
    }

    public void SetUI_StateBaseStateOverride(int i)
    {
        baseStateOverride = i;
    }

    public void SetUI_State(int i)
    {
        print("Setting UI State to: " + i);
        if(baseStateOverride != 0 && i == 0)
        {
            i = baseStateOverride;
        }
        anim.SetInteger("Ui_State", i);
        QuestManager.CheckQuestConditions();
    }
    public void SetUI_State(UI_State i)
    {
        SetUI_State((int)i);
    }


    public int GetUI_State()
    {
        return anim.GetInteger("Ui_State");
    }

    public void CloseAllUIWindows()
    {
        SetUI_State(0);
    }

    public bool InterfaceWindowIsOpen()
    {
        return GetUI_State() != 0 && GetUI_State() != 4;
    }

    public virtual void ShowMouseCursor(bool value)
    {
        // Not gonna be used anymore, cursor visibility is controlled via Animator
    }

    public void CallPauseMenu()
    {
        if(InterfaceWindowIsOpen())
        {
            CloseAllUIWindows();
            SetUI_State(UI_State.Base);
        }
        else if (!pauseMenu.activeSelf)
        {
            pauseEvent.Invoke();
            SetUI_State(UI_State.Pause);
        }
        else
        {
            negativePauseEvent.Invoke();
            SetUI_State(UI_State.Base);
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
            SetUI_State(UI_State.Base);
        }
        else
        {
            SetUI_State(UI_State.Inventory);
        }
    }

    public void SetUpEquipmentWindow()
    {
        Container equipmentContainer = GameManager.GetLocalPlayer().GetComponent<NetworkPlayerInit>().equipmentInventory;
        int i = 0;
        foreach(Item.ItemData item in equipmentContainer.items)
        {
            equipmentSelectors[i].SetUpEquipmentSelector(item);
            i++;
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
            SetUI_StateBaseStateOverride(0);
            // Check if a UI element was hit
            if (result[0].gameObject != null)
            {
                int i = -1;
                switch(result[0].gameObject.name)
                {
                    case "Image_01":
                        i = 0;
                        break;
                    case "Image_02":
                        i = 1;
                        break;
                    case "Image_03":
                        i = 2;
                        break;
                    case "Image_04":
                        i = 3;
                        break;
                }
                equipmentEquipEvent.Invoke(i);
            }
        }
    }

    #region RefinmentMenu

    public void SetUpRefinmentMenu(RefiningRecipie[] recipies)
    {
        for(int i = 0; i < refinementRecipiePanel.transform.childCount; i++)
        {
            Destroy(refinementRecipiePanel.transform.GetChild(i).gameObject);
        }
        foreach(RefiningRecipie rr in recipies)
        {
            GameObject go = Instantiate(refinementRecipieSlotPrefab, refinementRecipiePanel.transform);
            go.GetComponent<RefiningRecipieSlotUI>().Setup(rr);
        }
    }
    #endregion
    public virtual void SetRefinmentTimer(int timer)
    {
        refinmentTimer.text = $"Refining for: {timer} seconds!";
    }

    #region BuildingMenu
    public void UpdateGridSizeText()
    {
        gridSizeText.text = "(K) Grid Size: " +  BuildManager.Instance.gridSize.ToString("F2");
    }
    #endregion

    public static GameUI Instance { get => instance; }

}
