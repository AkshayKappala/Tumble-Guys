using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonController : NetworkBehaviour
{
    //input fields
    private ThirdPersonActionsAsset playerActionsAsset;
    private InputAction move;

    //references
    [Header("References:")]
    private Camera mainCamera;
    [SerializeField]
    private GameObject playerCamera;
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
    [Header("State Values (Do not modify):")]
    public bool isGrounded = true;
    public bool isReadyForDoubleJump = false;
    public CharacterState characterState = CharacterState.Idle;
    public float horizontalSpeed = 0;

    //Action Events
    public event Action JumpStarted;
    public event Action DoubleJumpStarted;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        playerActionsAsset = new ThirdPersonActionsAsset();
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        Cursor.lockState = CursorLockMode.Locked;
        mainCamera = Camera.main;
        playerCamera.SetActive(true);
        playerActionsAsset.Player.Jump.started += doJump;
        move = playerActionsAsset.Player.Move;
        playerActionsAsset.Player.Enable();
    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        playerActionsAsset.Player.Jump.started -= doJump;
        playerActionsAsset.Player.Disable();
    }
    private void Update()
    {
        if (!IsOwner) return;
        //sphere casting from feet to check whether the player is grounded
        Ray ray = new Ray(playerFeet.position + Vector3.up * 0.4f, Vector3.down);
        isGrounded = Physics.SphereCast(ray, 0.2f, 0.25f, 384);

        //enable cursor if control is held
        Cursor.lockState = playerActionsAsset.Player.FreeCursor.ReadValue<float>() > 0.1f ? CursorLockMode.None : CursorLockMode.Locked;
    }
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        //add force towards input direction
        forceDirection += move.ReadValue<Vector2>().x * movementForce * getCameraRight(mainCamera);
        forceDirection += move.ReadValue<Vector2>().y * movementForce * getCameraForward(mainCamera);
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

            if (!IsHost)
            {
                JumpStarted();
                doJumpServerRpc();
            }
            else
            {
                doJumpClientRpc();
            }
        }
        else if (isReadyForDoubleJump && horizontalSpeed >= maxSpeed * 0.75f)
        {
            forceDirection = Vector3.up * jumpForce * 0.8f;
            isReadyForDoubleJump = false;

            if (!IsHost)
            {
                DoubleJumpStarted();
                doDoubleJumpServerRpc();
            }
            else
            {
                doDoubleJumpClientRpc();
            }
        }
    }
    [ServerRpc]
    public void doJumpServerRpc()
    {
        JumpStarted();
    }
    [ServerRpc]
    public void doDoubleJumpServerRpc()
    {
        DoubleJumpStarted();
    }
    [ClientRpc]
    public void doJumpClientRpc()
    {
        JumpStarted();
    }
    [ClientRpc]
    public void doDoubleJumpClientRpc()
    {
        DoubleJumpStarted();
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