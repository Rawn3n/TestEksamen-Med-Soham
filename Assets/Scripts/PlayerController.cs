using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;

public class PlayerController : NetworkIdentity
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float dodgeForce = 15f;
    [SerializeField] float dodgeDuration = 0.25f;
    [SerializeField] float dodgeCooldown = 1.2f;
    [SerializeField] float sprintMultiplier = 1.5f;

    private bool isDodging;
    private float lastHorizontal;



    [Header("Jump")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] LayerMask groundMask;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.2f;

    private bool isGrounded;


    [Header("Mouse Look")]
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] Transform cameraPivot;
    [SerializeField] CinemachineCamera cmCamera;

    Rigidbody rb;
    Animator animator;
    PlayerInputActions input;

    float xRotation;
    bool canDodge = true;

    private void Awake()
    {
        input = new PlayerInputActions();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;

        if (isOwner)
        {
            input.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            cmCamera.Priority = 20;
        }
        else
        {
            cmCamera.Priority = 0;
        }
    }


    private void OnDisable()
    {
        if (input != null)
            input.Disable();
    }

    private void Update()
    {
        if (!isOwner) return;

        HandleMouseLook();
        HandleDodge();
        HandleJump();
    }

    private void FixedUpdate()
    {
        if (!isOwner) return;

        CheckGround();
        HandleMovement();
    }

    #region Movement
    void HandleMovement()
    {
        if (isDodging) return;

        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();

        if (Mathf.Abs(moveInput.x) > 0.1f)
            lastHorizontal = moveInput.x;

        Vector3 moveDir = transform.forward * moveInput.y + transform.right * moveInput.x;

        float speed = moveSpeed;
        if (input.Player.Sprint.IsPressed())
            speed *= sprintMultiplier;

        Vector3 currentVelocity = rb.linearVelocity;

        rb.linearVelocity = new Vector3(
            moveDir.x * speed,
            currentVelocity.y,
            moveDir.z * speed
        );
    }

    #endregion

    #region Dodge
    void HandleDodge()
    {
        if (!canDodge) return;

        if (input.Player.Sprint.WasPressedThisFrame())
        {
            if (Mathf.Abs(lastHorizontal) > 0.1f)
            {
                StartCoroutine(Dodge(lastHorizontal > 0 ? 1 : -1));
            }
        }
    }


    IEnumerator Dodge(int direction)
    {
        Debug.Log("DODGE START");

        canDodge = false;
        isDodging = true;

        // if (animator != null)
        //     animator.SetTrigger(direction > 0 ? "DodgeRight" : "DodgeLeft");

        Vector3 dodgeDir = transform.right * direction;
        rb.AddForce(dodgeDir * dodgeForce, ForceMode.VelocityChange);

        yield return new WaitForSeconds(dodgeDuration);

        isDodging = false;

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    #endregion

    #region MouseLook
    void HandleMouseLook()
    {
        Vector2 mouseDelta = input.Player.Look.ReadValue<Vector2>();


        mouseDelta *= mouseSensitivity;

        xRotation -= mouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseDelta.x);
    }

    #endregion

    #region Jump
    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundMask
        );
    }

    void HandleJump()
    {
        if (input.Player.Jump.WasPressedThisFrame() && isGrounded)
        {
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    #endregion
}
