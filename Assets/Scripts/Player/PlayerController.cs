using Cinemachine;
using MoreMountains.Feedbacks;
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
    [SerializeField] private float maxFlyingSpeed = 15;
    [SerializeField] private float rotationSpeed = 40;
    private float ySpeed = -9.81f;
    private float currentSpeed;
    private Vector3 movementDirection;

    public float acceleration = 5.0f;
    public float deceleration = 10.0f;
    private float currentFlyingSpeed;

    // References
    public CharacterController characterController;
    private Transform cameraTransform;
    private GameObject playerSetup;
    private CinemachineStateDrivenCamera virtualCamera;
    private Animator cameraAnimator;
    public Vector3 rootMotionMotion;
    public MMF_Player cameraShakeFeedback;
    public Inventory inventory;

    // Prefabs
    public GameObject hitBoxSphere;


    // hitboxStuff
    private Vector3 hitboxPosition;
    private float hitboxSize;

    // SpinAttackStuff
    private float cooldown = 1;
    private float currentCooldown = 0;

    // Equipment
    public GameObject axe;
    public GameObject pickaxe;
    public NetworkVariable<int> currentEquipment = new NetworkVariable<int>(0);

    //flying
    private bool toggleFlying = false;


    public override void OnNetworkSpawn()
    {
        currentEquipment.OnValueChanged += CurrentEquipmentValueChangedOperation;
    }

    private void CurrentEquipmentValueChangedOperation(int prevVal,int newVal)
    {
        anim.SetInteger("equipment", newVal);
        axe.SetActive(false);
        pickaxe.SetActive(false);
        if (newVal == 1)
        {
            pickaxe.SetActive(true);
        }
        if (newVal == 0)
        {
            axe.SetActive(true);
        }
    }

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
            virtualCamera = playerSetup.transform.Find("State-Driven Camera").GetComponent<CinemachineStateDrivenCamera>();
            for(int i = 0; i < virtualCamera.transform.childCount - 1; i++)
            {
                CinemachineVirtualCameraBase cam=virtualCamera.transform.GetChild(i).GetComponent<CinemachineVirtualCameraBase>();
                cam.Follow = transform;
                cam.LookAt = transform;
            }
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
            cameraAnimator = virtualCamera.GetComponent<Animator>();
            inventory = GetComponent<Inventory>();

        }

        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {

        if(transform.position.y < -100)
        {
            characterController.enabled = false;
            transform.position = new Vector3(0, 1,0);
            characterController.enabled = true;
        }

        if(toggleFlying == false){
            Movement();
            Rotation();
            HandleAttack();
            HandleEquipment();
        }
        else
        {
            FlyingMovement();
            FlyingRotation();
        }

        Animations();
        HandleCooldown();
        HandleSpinAttack();
        

        if(Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            toggleFlying = !toggleFlying;
            if (toggleFlying)
            {
                cameraAnimator.SetInteger("cam", 1);
            }
            else
            {
                cameraAnimator.SetInteger("cam", 0);
            }
        }
    }

    private void FlyingMovement()
    {

        // Calculate the direction from the object to the camera.
        Vector3 direction = transform.position - cameraTransform.position;

        // Normalize the direction vector to make the movement consistent.
        direction.Normalize();

        if (Input.GetKey(KeyCode.W))
        {
            currentFlyingSpeed = Mathf.MoveTowards(currentFlyingSpeed, maxFlyingSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentFlyingSpeed = Mathf.MoveTowards(currentFlyingSpeed, 0, deceleration * Time.deltaTime);
        }

        // Move the object in the camera's direction.
        characterController.Move(direction * currentFlyingSpeed * Time.deltaTime);
    }

    private void HandleCooldown()
    {
        if(currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    private void HandleEquipment()
    { 
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentEquipment.Value = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentEquipment.Value = 1;
        }
    }

    private void HandleSpinAttack()
    {
        if(Input.GetKeyDown(KeyCode.Q) && currentCooldown <= 0)
        {
            anim.SetBool("spin", true);

        }

        if(anim.GetCurrentAnimatorStateInfo(0).IsName("SpinAttackAxe") && anim.GetBool("spin"))
        {
            anim.SetBool("spin", false);
            currentCooldown = cooldown;
            GetComponentInChildren<VFXController>().TriggerVFX(3,6);
        }
    }

    private void HandleAttack()
    {
        if(Input.GetMouseButton(0))
        {
            anim.SetBool("attack", true);
        }
        else
        {
            anim.SetBool("attack", false);
        }
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

    private void FlyingRotation()
    {
        if(!Mathf.Approximately(currentFlyingSpeed,0))
        {
            transform.rotation = cameraTransform.rotation;
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
        if(anim.GetBool("attack"))
        {
            currentSpeed *= 0.3f;
        }

        movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up)
    * movementDirection;

        Vector3 finalMovementDirection = movementDirection * currentSpeed;
        finalMovementDirection.y = ySpeed;
        characterController.Move((finalMovementDirection * maxMovementSpeed * Time.deltaTime)+ rootMotionMotion);

        rootMotionMotion = Vector3.zero;

    }

    public void SpawnHitBox(Vector3 pos, float size)
    {
        hitboxPosition = pos;
        hitboxSize = size;
        SpawnHitboxServerRpc(pos,size);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnHitboxServerRpc(Vector3 pos, float size)
    {

        GameObject _hitbox = Instantiate(hitBoxSphere, pos, Quaternion.identity);
        _hitbox.transform.localScale = new Vector3(size, size, size);

        _hitbox.GetComponent<NetworkObject>().Spawn();
        _hitbox.GetComponent<DamageObject>().SetSourceObjectServerRpc(NetworkObject);


    }


}
