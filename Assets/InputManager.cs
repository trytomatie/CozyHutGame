using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;

    private int currentDevice = 0; // 0 == Keyboard, 1 == Controller
    private PlayerInput inputActions;

    [Header("Inputs")]
    public Vector2 movement;
    public Vector2 cameraMovement;

    public static InputManager Instance { get => instance; }

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
        InputAction movement = inputActions.FindAction("Player/Movement");
        // movement.ApplyBindingOverride("<Keyboard/W");
        inputActions.Enable();
        inputActions.Player.Movement.performed += SetMovement;
        inputActions.Player.Movement.canceled += CancelMovement;
        inputActions.Player.CameraMovement.performed += SetCameraMovement;
        inputActions.Player.CameraMovement.canceled += CancelCameraMovement;
    }



    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Movement.performed -= SetMovement;
        inputActions.Player.Movement.canceled -= CancelMovement;
        inputActions.Player.CameraMovement.performed -= SetCameraMovement;
        inputActions.Player.CameraMovement.canceled -= CancelCameraMovement;
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
}
