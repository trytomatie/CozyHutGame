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
    [SerializeField] private float rotationSpeed = 40;
    private float ySpeed = -9.81f;
    private float currentSpeed;
    private Vector3 movementDirection;

    // References
    public CharacterController characterController;
    private Transform cameraTransform;
    private GameObject playerSetup;
    private CinemachineVirtualCamera virtualCamera;
    public Vector3 rootMotionMotion;
    public MMF_Player cameraShakeFeedback;

    // Prefabs
    public GameObject hitBoxSphere;

    // hitboxStuff
    private Vector3 hitboxPosition;
    private float hitboxSize;

    // SpinAttackStuff
    private float cooldown = 1;
    private float currentCooldown = 0;


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
        HandleAttack();
        HandleCooldown();
        HandleSpinAttack();

    }

    private void HandleCooldown()
    {
        if(currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
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
        if (!anim.GetBool("attack"))
        {
            // Rotate the character to movement direction
            if (movementDirection != Vector3.zero)
            {

                    Quaternion targetCharacterRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetCharacterRotation, rotationSpeed * Time.deltaTime);
            


            }
        }
        else
        {
            Vector3 mousePosition = Input.mousePosition;
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            float hitDistance;

            if (groundPlane.Raycast(ray, out hitDistance))
            {
                Vector3 cursorPosition = ray.GetPoint(hitDistance);

                Vector3 direction = cursorPosition - transform.position;
                direction.Normalize();
                Quaternion targetCharacterRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetCharacterRotation, 10000 * Time.deltaTime);
            }
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
