using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonController : MonoBehaviour
{
    //input fields
    private ThirdPersonActionsAsset playerActionsAsset;
    private InputAction move;

    //references
    [Header("References:")]
    [SerializeField]
    private Camera playerCamera;
    [SerializeField]
    private Transform playerFeet;

    //movement fields
    private Rigidbody rb;
    [Header("Variables:")]
    [SerializeField]
    private float movementForce = 1f;
    [SerializeField]
    private float jumpForce = 5f;
    [SerializeField]
    private float maxSpeed = 5f;
    private Vector3 forceDirection = Vector3.zero;
    [SerializeField]
    private float downwardGravityBias = 1f;

    //State fields
    [Header("State Values:")]
#if UNITY_EDITOR
    [ReadOnly]
#endif
    public bool isGrounded = true;
#if UNITY_EDITOR
    [ReadOnly]
#endif
    public bool isReadyForDoubleJump = false;
#if UNITY_EDITOR
    [ReadOnly]
#endif
    public CharacterState characterState = CharacterState.Idle;
#if UNITY_EDITOR
    [ReadOnly]
#endif
    public float horizontalSpeed = 0;

    //Action Events
    public event Action JumpStarted;
    public event Action DoubleJumpStarted;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = this.GetComponent<Rigidbody>();
        playerActionsAsset = new ThirdPersonActionsAsset();
    }
    private void OnEnable()
    {
        playerActionsAsset.Player.Jump.started += doJump;
        move = playerActionsAsset.Player.Move;
        playerActionsAsset.Player.Enable();
    }
    private void OnDisable()
    {
        playerActionsAsset.Player.Jump.started -= doJump;
        playerActionsAsset.Player.Disable();
    }
    private void Update()
    {
        //raycasting from feet to check whether the player is grounded
        Ray ray = new Ray(playerFeet.position + Vector3.up * 0.25f, Vector3.down);
        isGrounded = Physics.Raycast(ray, out RaycastHit hit, 0.3f, 384);
    }
    private void FixedUpdate()
    {
        //add force towards input direction
        forceDirection += move.ReadValue<Vector2>().x * movementForce * getCameraRight(playerCamera);
        forceDirection += move.ReadValue<Vector2>().y * movementForce * getCameraForward(playerCamera);
        rb.AddForce(forceDirection, ForceMode.Impulse);

        //zero force after every impulse
        forceDirection = Vector3.zero;

        //if falling increase falling velocity (to make jumping less floaty)
        if (rb.velocity.y < 0f)
            rb.velocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime * downwardGravityBias;

        //cap horizontal velocity to max speed
        capHorizontalSpeed();

        //look in the direction character is moving
        LookForward();
    }
    private void capHorizontalSpeed()
    {
        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = horizontalVelocity + Vector3.up * rb.velocity.y;
        }
        horizontalSpeed = horizontalVelocity.magnitude;
    }
    private void LookForward()
    {
        Vector3 direction = rb.velocity;
        direction.y = 0f;
        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
            rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        else
            rb.angularVelocity = Vector3.zero;
    }
    private Vector3 getCameraForward(Camera playerCamera)
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }
    private Vector3 getCameraRight(Camera playerCamera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }
    private void doJump(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            forceDirection = Vector3.up * jumpForce;
            isReadyForDoubleJump = true;
            JumpStarted();
        }
        else if (isReadyForDoubleJump && horizontalSpeed >= maxSpeed * 0.75f)
        {
            forceDirection = Vector3.up * jumpForce * 0.8f;
            isReadyForDoubleJump = false;
            DoubleJumpStarted();
        }
    }

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }
}

public enum CharacterState
{
    Idle,
    Walking,
    Running,
    Jumping,
    Dashing,
    Falling
}