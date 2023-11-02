using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingMountController : State
{

    [Header("Speed stats")]
    public float walkSpeed = 2;
    public float runSpeed = 6;
    public float backwardsSpeed = 6;
    public float acceleration = 7;
    public float turnspeed = 15;

    private float movementSpeed = 0;
    private float ySpeed;
    private Vector3 movement;
    private Vector3 lastMovement;

    public CharacterController mount;
    private NetworkPlayerController pc;

    private Transform mainCamera;

    private void Start()
    {
        pc = GetComponent<NetworkPlayerController>();
    }

    public void Movement()
    {
        float horizontalInput = InputManager.Instance.movement.x;
        float verticalInput = InputManager.Instance.movement.y;
        float targetSpeed = 6;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            targetSpeed = 4;
        }

        movement = new Vector3(horizontalInput, 0, verticalInput).normalized;

        Vector3 cameraDependingMovement = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) * movement;
        movementSpeed = Mathf.MoveTowards(movementSpeed, movement.magnitude * targetSpeed, acceleration * Time.deltaTime);

        if (verticalInput != 0 || horizontalInput != 0)
        {
            lastMovement = cameraDependingMovement;
        }

        print(lastMovement);
        mount.Move(lastMovement * movementSpeed * Time.deltaTime
            + new Vector3(0, ySpeed, 0) * Time.deltaTime
            );
    }

    private void FlyingMovement()
    {

        // Calculate the direction from the object to the camera.
        Vector3 direction = transform.position - mainCamera.position;

        // Normalize the direction vector to make the movement consistent.
        direction.Normalize();

        if (Input.GetKey(KeyCode.W))
        {
            movementSpeed = Mathf.MoveTowards(movementSpeed, runSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            movementSpeed = Mathf.MoveTowards(movementSpeed, 0, acceleration * Time.deltaTime);
        }

        // Move the object in the camera's direction.
        mount.Move(direction * movementSpeed * Time.deltaTime);
    }

    private void FlyingRotation()
    {
        if (!Mathf.Approximately(movementSpeed, 0))
        {
            mount.transform.rotation = mainCamera.rotation;
        }
    }

    #region StateMethodes
    public override void UpdateState(GameObject source)
    {
        FlyingMovement();
        FlyingRotation();
    }

    public override void EnterState(GameObject source)
    {
        transform.localEulerAngles = Vector3.zero;
        mainCamera = Camera.main.transform;
        pc.cc.enabled = false;
    }

    public override StateName Transition(GameObject source)
    {
        return stateName;
    }

    public override void ExitState(GameObject source)
    {
        pc.cc.enabled = true;
    }

    #endregion

}
