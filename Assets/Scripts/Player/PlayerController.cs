using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    [SerializeField] private GameObject playerSetupPrefab;
    public Animator anim;

    // Character Movement Properties
    [SerializeField] private float maxMovementSpeed = 6;
    [SerializeField] private float rotationSpeed = 40;
    private float ySpeed = -9.81f;
    private float currentSpeed;
    private Vector3 movementDirection;

    // References
    public CharacterController characterController;
    private Transform cameraTransform;
    private GameObject playerSetup;
    private CinemachineVirtualCamera virtualCamera;


    public void Start()
    {
        if (!IsOwner)
        {
            this.enabled = false;
        }
        else
        {
            // Setup Player
            playerSetup = Instantiate(playerSetupPrefab, transform.position, Quaternion.identity);
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(playerSetup);
            cameraTransform = playerSetup.transform.Find("Camera");
            virtualCamera = playerSetup.transform.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;

        }

        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {

        if(transform.position.y < -100)
        {
            characterController.enabled = false;
            transform.position = new Vector3(transform.position.x, 1, transform.position.z);
            characterController.enabled = true;
        }

        Movement();

        Rotation();
        Animations();

    }

    private void Rotation()
    {
        // Rotate the character to movement direction
        if (movementDirection != Vector3.zero)
        {
            Quaternion targetCharacterRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetCharacterRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void Animations()
    {
        anim.SetFloat("speed", currentSpeed);
    }

    private void Movement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        movementDirection = new Vector3(horizontalInput, 0, verticalInput);
        float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);

        bool shouldWalk = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        currentSpeed = shouldWalk ? inputMagnitude * 0.333f : inputMagnitude;

        movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up)
    * movementDirection;

        Vector3 finalMovementDirection = movementDirection * currentSpeed;
        finalMovementDirection.y = ySpeed;
        characterController.Move(finalMovementDirection * maxMovementSpeed * Time.deltaTime );
    }
}
