using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// By Christian Scherzer
/// </summary>
public class NetworkPlayerController : State
{
    [Header("Speed stats")]
    public float walkSpeed = 2;
    public float  runSpeed = 6;
    public float backwardsSpeed = 6;
    public float acceleration = 7;
    public float turnspeed = 15;
    public const float sneakSpeed = 2f;
    public Vector3 rootMotionMotion;

    [Header("Physics")]
    public float gravity = -9.81f;
    public bool grounded = false;

    [Header("GroundCheck")]
    public float castDistance = 0.09f;
    public float castScaleFactor = 1;
    public LayerMask layerMask;


    private bool isJumping = false;
    
    public float jumpStrength = 5;
    private Vector3 lastHitPoint;
    private Vector3 slideMovement;



    private float movementSpeed = 0;
    private float ySpeed;
    private Vector3 movement;
    private Vector3 lastMovement;

    private bool isTransitioning = false;

    private CharacterController cc;
    public Animator anim;
    private Camera mainCamera;
    private Animator cameraAnimator;
    private Inventory inventory;

    [Header("PlayerSetup")]
    public GameObject playerSetupPrefab;

    [Header("Camera")]
    private CinemachineStateDrivenCamera virtualCamera;
    [SerializeField] private Transform cameraFollowTarget;
    [SerializeField] private float zoomSpeed = 2.0f;
    [SerializeField] private float minZoom = 5.0f;
    [SerializeField] private float maxZoom = 20.0f;




    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        inventory = GetComponent<Inventory>();
        mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        if (!IsOwner)
        {
            this.enabled = false;
        }
        else
        {
            // Setup Player
            GameObject playerSetup = Instantiate(playerSetupPrefab, transform.position, Quaternion.identity);
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(playerSetup);
            mainCamera = playerSetup.transform.Find("Camera").GetComponent<Camera>();
            virtualCamera = playerSetup.transform.Find("State-Driven Camera").GetComponent<CinemachineStateDrivenCamera>();
            virtualCamera.Follow = cameraFollowTarget;
            virtualCamera.LookAt = transform;
            cameraAnimator = virtualCamera.GetComponent<Animator>();
            inventory = GetComponent<Inventory>();

        }
    }




    private void LateUpdate()
    {
        if(!(ySpeed - cc.velocity.y > -10))
        {
            slideMovement = (transform.position - lastHitPoint).normalized * 0.1f;
        }
        else
        {
            slideMovement = Vector3.zero;
        }

    }

    /// <summary>
    /// Handle Animations
    /// </summary>
    private void Animations()
    {
        anim.SetFloat("speed", movementSpeed);
        anim.SetFloat("ySpeed", ySpeed / 12);
    }

    /// <summary>
    /// Handle Rotation
    /// </summary>
    private void Rotation()
    {
        if (cc.velocity.magnitude > 0)
        {
            float rotation = Mathf.Atan2(lastMovement.x, lastMovement.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, rotation, 0), turnspeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Handle Movement
    /// </summary>
    private void Movement()
    {
        float horizontalInput = InputManager.Instance.movement.x;
        float verticalInput = InputManager.Instance.movement.y;
        float targetSpeed = runSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            targetSpeed = walkSpeed;
        }

        // If character is landing, he cant move
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Jumping"))
        {
            movement = new Vector3(horizontalInput * 0.1f, 0, verticalInput*0.1f).normalized;
        }
        else
        {
            movement = new Vector3(horizontalInput, 0, verticalInput).normalized;
        }


        Vector3 cameraDependingMovement = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) * movement;
        movementSpeed = Mathf.MoveTowards(movementSpeed, movement.magnitude * targetSpeed, acceleration * Time.deltaTime);

        if (verticalInput != 0 || horizontalInput != 0)
        {
            lastMovement = cameraDependingMovement;
        }


        cc.Move(lastMovement * movementSpeed * Time.deltaTime 
            + new Vector3(0, ySpeed, 0) * Time.deltaTime
            + slideMovement + rootMotionMotion);
    }


    /// <summary>
    /// Handle Jump Input
    /// </summary>
    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && !isJumping && grounded 
            && anim.GetCurrentAnimatorStateInfo(0).IsName("Movement"))
        {
            print("test");
            isJumping = true;
            ySpeed += jumpStrength;
            anim.SetTrigger("jump");
        }
        if (grounded && isJumping && ySpeed <= 0)
        {
            isJumping = false;
            anim.SetTrigger("land");
        }
    }

    /// <summary>
    /// Calculates gravity
    /// </summary>
    private void CalculateGravity()
    {
        
        if (!Helper.CheckBeneath(transform.position, cc, layerMask, castDistance, castScaleFactor))
        {
            ySpeed += gravity * Time.deltaTime;
            grounded = false;
        }
        else
        {
            grounded = true;
            if (ySpeed < 0)
            {
                ySpeed = 0;
            }
        }

    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        lastHitPoint = hit.point;
    }

    #region StateMethodes
    public override void UpdateState(GameObject source)
    {
        if (isTransitioning)
        {
            isTransitioning = false;
            return;
        }
        HandleJump();
        CalculateGravity();
        Movement();
        Rotation();
        Animations();

    }

    public override void EnterState(GameObject source)
    {

    }

    public override StateName Transition(GameObject source)
    {
        return stateName;
    }

    public override void ExitState(GameObject source)
    {

    }

    #endregion

}
