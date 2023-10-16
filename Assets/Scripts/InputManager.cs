using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;

    private int currentDevice = 0; // 0 == Keyboard, 1 == Controller
    public PlayerInput inputActions;

    [Header("Inputs")]
    public Vector2 movement;
    public Vector2 cameraMovement;
    private float cameraZoomDelta;

    // Events
    public delegate void CameraZoomEventHandler(object sender, float value);
    public delegate void InputEventHandler(object sender, InputAction.CallbackContext value);
    public event CameraZoomEventHandler CameraZoomDeltaPerformed;
    public event InputEventHandler InventoryButton;
    public event InputEventHandler InteractionButton;



    void Awake()
    {
        if(Instance == null)
        {
            inputActions = new PlayerInput();
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }


    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Movement.performed += SetMovement;
        inputActions.Player.Movement.canceled += CancelMovement;
        inputActions.Player.CameraMovement.performed += SetCameraMovement;
        inputActions.Player.CameraMovement.canceled += CancelCameraMovement;
        inputActions.Player.CameraZoom.performed += SetCameraZoom;
        inputActions.Player.CameraZoom.canceled += CancelCameraZoom;
        inputActions.Interface.Inventory.performed += InventoryButtonDown;
        inputActions.Player.Interact.performed += InteractionButtonDown;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Movement.performed -= SetMovement;
        inputActions.Player.Movement.canceled -= CancelMovement;
        inputActions.Player.CameraMovement.performed -= SetCameraMovement;
        inputActions.Player.CameraMovement.canceled -= CancelCameraMovement;
        inputActions.Player.CameraZoom.performed -= SetCameraZoom;
        inputActions.Player.CameraZoom.canceled -= CancelCameraZoom;
        inputActions.Interface.Inventory.performed -= InventoryButtonDown;
    }

    private void SetMovement(InputAction.CallbackContext value)
    {
        movement = value.ReadValue<Vector2>();
    }

    private void CancelMovement(InputAction.CallbackContext value)
    {
        movement = Vector2.zero;
    }   
    
    private void SetCameraMovement(InputAction.CallbackContext value)
    {
        cameraMovement = value.ReadValue<Vector2>();
    }

    private void CancelCameraMovement(InputAction.CallbackContext value)
    {
        cameraMovement = Vector2.zero;
    }

    private void SetCameraZoom(InputAction.CallbackContext value)
    {
        CameraZoomDelta = value.ReadValue<Vector2>().y;
    }

    private void CancelCameraZoom(InputAction.CallbackContext value)
    {
        CameraZoomDelta = 0;
    }

    public float CameraZoomDelta 
    { 
        get => cameraZoomDelta;
        set
        {
            cameraZoomDelta = value;
            OnCameraZoomDelta(value);
        }
    }

    public static InputManager Instance { get => instance; }

    #region Events
    protected virtual void OnCameraZoomDelta(float value)
    {
        CameraZoomEventHandler handler = CameraZoomDeltaPerformed;
        if(handler != null)
        {
            handler(this, value);
        }
    }

    protected virtual void InventoryButtonDown(InputAction.CallbackContext value)
    {
        InputEventHandler handler = InventoryButton;
        if (handler != null)
        {
            handler(this, value);
        }
    }

    private void InteractionButtonDown(InputAction.CallbackContext value)
    {
        InputEventHandler handler = InteractionButton;
        if (handler != null)
        {
            handler(this, value);
        }
    }
    #endregion
}
