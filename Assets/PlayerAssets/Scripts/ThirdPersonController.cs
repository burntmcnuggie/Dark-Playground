using UnityEngine;

/*
    This file has a commented version with details about how each line works. 
   he  Tcommented version contains code that is easier and simpler to read. This file is minified.
*/


/// <summary>
/// Main script for third-person movement of the character in the game.
/// Make sure that the object that will receive this script (the player) 
/// has the Player tag and the Character Controller component.
/// </summary>
public class ThirdPersonController : MonoBehaviour
{

    StaminaController stamina;

    [Tooltip("Speed ​​at which the character moves. It is not affected by gravity or jumping.")]
    public float velocity = 5f;
    [Tooltip("This value is added to the speed value while the character is sprinting.")]
    public float sprintAddition = 3.5f;
    [Space]
    [Tooltip("Force that pulls the player down. Changing this value causes all movement, jumping and falling to be changed as well.")]
    public float gravity = 9.8f;

    // Player states
    public bool isJumping { get; private set; } = false;
    public bool isSprinting { get; private set; } = false;
    public bool isCrouching { get; private set; } = false;

    // Inputs
    float inputHorizontal;
    float inputVertical;
    bool inputJump;
    bool inputCrouch;
    bool inputSprint;

    //direction
    float verticalDirection;
    Vector3 horizontalDirection;
    float directionY;
    float directionX;
    float directionZ;
    Vector3 forward;
    Vector3 right;

    Animator animator;
    bool _hasAnimator;
    int animState = 0;
    [SerializeField] CharacterController cc;

    Transform cameraT;
    // Transform cameraParent;

    public bool lockRotation = false;

    [SerializeField] float headHitDistance = 0.25f;
    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    [SerializeField]public bool Grounded { get; private set; } = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    private float _terminalVelocity = 53.0f;

    public bool movementEnabled = true;

    public bool testing = false;

    void Awake()
    {
        if(cc == null)
        {
            cc = GetComponent<CharacterController>();
        }
        animator = GetComponent<Animator>();

        if (cameraT == null)
        {
            cameraT = Camera.main.transform;
        }

        // cameraParent = cameraT.parent;

        _hasAnimator = animator != null;

        // Message informing the user that they forgot to add an animator
        if (!_hasAnimator)
            Debug.LogWarning("Hey buddy, you don't have the Animator component in your player. Without it, the animations won't work.");

        if(testing)
            sprintAddition = 60;

            stamina = GetComponent<StaminaController>();
    }


    // Update is only being used here to identify keys and trigger animations
    void Update()
    {
        if (!movementEnabled)
            return;
        // print(Grounded + " grounded");
        GatherInputs();

        // Check if you pressed the crouch input key and change the player's state
        if (inputCrouch)
            isCrouching = !isCrouching;

        HandleAnimations();
        GroundedCheck();

        // Handle can jump or not
        if (inputJump && Grounded)
        {
            isJumping = true;
            // Disable crounching when jumping
            //isCrouching = false; 
        }

        RotateCharacter();
        Movement();

        if(Input.GetKeyDown(KeyCode.Alpha7))
        {
            testing = !testing;
            sprintAddition = testing ? 60 : 6;
        }
    }

    // With the inputs and animations defined, FixedUpdate is responsible for applying movements and actions to the player
    private void FixedUpdate()
    {
        if (!movementEnabled)
            return;
        JumpAndGravity();
        HeadHittingDetect();
    }

    private void RotateCharacter()
    {
        forward = Camera.main.transform.forward;
        right = Camera.main.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 horizontalDirection = forward * inputVertical * Time.deltaTime + right * inputHorizontal * Time.deltaTime;

        if (!lockRotation && (inputHorizontal != 0 || inputVertical != 0))
        {
            float angle = Mathf.Atan2(horizontalDirection.x, horizontalDirection.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }
        else if (lockRotation)
        {
            Quaternion rotation = Quaternion.Euler(0, cameraT.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }
    }

    private void Movement()
    {
        float velocityAddition = 0;
        if (isSprinting && stamina.canSprint)
            velocityAddition = sprintAddition;
        if (isCrouching)
            velocityAddition = -(velocity * 0.50f);  // Reduce velocity by 50% when crouching


        directionX = inputHorizontal * (velocity + velocityAddition);
        directionZ = inputVertical * (velocity + velocityAddition);

        animator.SetFloat("speedZ", directionZ);
        animator.SetFloat("speedX", directionX);

        forward *= directionZ * Time.deltaTime;
        right *= directionX * Time.deltaTime;

        horizontalDirection = forward + right;
        Vector3 movement = horizontalDirection;
        movement.y = verticalDirection * Time.deltaTime;
        cc.Move(movement);
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // stop our velocity dropping infinitely when grounded
            if (verticalDirection < 0.0f)
            {
                verticalDirection = -2f;
            }

            // Jump
            if (movementEnabled && inputJump && _jumpTimeoutDelta <= 0.0f && stamina.canSprint)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                verticalDirection = Mathf.Sqrt(JumpHeight * 2f);

            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            // else
            // {
            //     // update animator if using character
            //     if (_hasAnimator)
            //     {
            //         _animator.SetBool(_animIDFreeFall, true);
            //     }
            // }

            // if we are not grounded, do not jump
            inputJump = false;

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (verticalDirection < _terminalVelocity)
            {
                verticalDirection += gravity * Time.deltaTime;
            }
        }
    }

    void GatherInputs()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            lockRotation = !lockRotation;
        }

        // Input checkers
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        inputJump = Input.GetAxis("Jump") == 1f;
        inputSprint = Input.GetAxis("Fire3") == 1f;

        // Unfortunately GetAxis does not work with GetKeyDown, so inputs must be taken individually
        inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);
    }

    void HandleAnimations()
    {
        // Run and Crouch animation
        // If dont have animator component, this block wont run
        if (Grounded && animator != null)
        {
            // Crouch
            // Note: The crouch animation does not shrink the character's collider
            animator.SetBool("crouch", isCrouching);

            // Run
            float minimumSpeed = 0.9f;
            animator.SetBool("run", cc.velocity.magnitude > minimumSpeed);

            // Sprint
            isSprinting = cc.velocity.magnitude > minimumSpeed && inputSprint;
            animator.SetBool("sprint", isSprinting);
        }
        if (animator)
        {
            animator.SetBool("air", Grounded == false && _fallTimeoutDelta < 0.0f);
        }

    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // print("ground " + Grounded);
    }

    //This function makes the character end his jump if he hits his head on something
    void HeadHittingDetect()
    {
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = cc.height / 2f * headHitDistance;

        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc, GroundLayers))
        {
            verticalDirection = gravity * Time.deltaTime;
        }
    }

    public void DeactivateControls()
    {
        cc.enabled = false;
        movementEnabled = false;
        animator.SetFloat("speedZ", 0f);
        animator.SetFloat("speedX", 0f);
    }

    public void ActivateControls()
    {
        cc.enabled = true;
        movementEnabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // Draw a sphere at the grounded check location
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);

        // Draw the head hit detection raycast
        if (cc != null)
        {
            Vector3 ccCenter = transform.TransformPoint(cc.center);
            float hitCalc = cc.height / 2f * headHitDistance;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(ccCenter, ccCenter + Vector3.up * hitCalc);
            Gizmos.DrawWireSphere(ccCenter + Vector3.up * hitCalc, 0.1f);  // Optional: Draw a small sphere at the endpoint
        }
    }

}
