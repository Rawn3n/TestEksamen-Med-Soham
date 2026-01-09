using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float dodgeForce = 15f;
    [SerializeField] float dodgeDuration = 0.25f;
    [SerializeField] float dodgeCooldown = 1.2f;
    [SerializeField] float sprintMultiplier = 1.5f;

    bool isDodging;
    bool canDodge = true;

    [Header("Stamina")]
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaRegenRate = 20f;
    [SerializeField] float sprintStaminaDrain = 15f;
    [SerializeField] float dodgeStaminaCost = 35f;

    // SyncVar with owner authorization
    public SyncVar<float> currentStamina = new SyncVar<float>(0f, ownerAuth: true);

    public float StaminaNormalized =>
        maxStamina <= 0f ? 0f : currentStamina.value / maxStamina;

    public event System.Action<float> OnStaminaUpdated;

    [Header("Jump")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] LayerMask groundMask;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.2f;
    bool isGrounded;

    [Header("Mouse Look")]
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] Transform cameraPivot;
    [SerializeField] CinemachineCamera cmCamera;

    Rigidbody rb;
    PlayerInputActions input;
    float xRotation;

    #region Unity Methods
    protected override void OnSpawned()
    {
        base.OnSpawned();

        rb = GetComponent<Rigidbody>();
        input = new PlayerInputActions();

        // Subscribe to SyncVar changes
        currentStamina.onChanged += HandleStaminaChanged;

        if (isOwner)
        {
            input.Enable();
            cmCamera.Priority = 20;

            // Initialize stamina (owner can write)
            currentStamina.value = maxStamina;
        }
        else
        {
            cmCamera.Priority = 0;
        }
    }

    void OnDisable()
    {
        input?.Disable();
        currentStamina.onChanged -= HandleStaminaChanged;
    }

    void Update()
    {
        if (!isOwner) return;

        HandleMouseLook();
        HandleDodge();
        HandleJump();
        RegenerateStamina();
    }

    void FixedUpdate()
    {
        if (!isOwner) return;

        CheckGround();
        HandleMovement();
    }
    #endregion

    #region Movement
    void HandleMovement()
    {
        if (isDodging) return;

        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
        Vector3 moveDir = transform.forward * moveInput.y + transform.right * moveInput.x;

        float speed = moveSpeed;

        if (input.Player.Sprint.IsPressed() && currentStamina.value > 0f)
        {
            speed *= sprintMultiplier;
            float newStamina = Mathf.Max(0f, currentStamina.value - sprintStaminaDrain * Time.fixedDeltaTime);
            SetStamina(newStamina);
        }

        var v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(moveDir.x * speed, v.y, moveDir.z * speed);
    }
    #endregion

    #region Dodge
    void HandleDodge()
    {
        if (!canDodge || currentStamina.value < dodgeStaminaCost) return;

        if (input.Player.Sprint.WasPressedThisFrame())
        {
            Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
            if (moveInput.sqrMagnitude < 0.01f) return;

            float newStamina = Mathf.Max(0f, currentStamina.value - dodgeStaminaCost);
            SetStamina(newStamina);

            Vector3 dir = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
            StartCoroutine(Dodge(dir));
        }
    }

    IEnumerator Dodge(Vector3 direction)
    {
        canDodge = false;
        isDodging = true;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * dodgeForce, ForceMode.VelocityChange);

        yield return new WaitForSeconds(dodgeDuration);
        isDodging = false;

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }
    #endregion

    #region Mouse Look
    void HandleMouseLook()
    {
        Vector2 mouseDelta = input.Player.Look.ReadValue<Vector2>() * mouseSensitivity;

        xRotation -= mouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseDelta.x);
    }
    #endregion

    #region Jump
    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }

    void HandleJump()
    {
        if (!input.Player.Jump.WasPressedThisFrame() || !isGrounded) return;

        var v = rb.linearVelocity;
        v.y = 0f;
        rb.linearVelocity = v;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    #endregion

    #region Stamina
    void RegenerateStamina()
    {
        if (isDodging || input.Player.Sprint.IsPressed()) return;

        float newStamina = Mathf.Clamp(currentStamina.value + staminaRegenRate * Time.deltaTime, 0f, maxStamina);
        SetStamina(newStamina);
    }

    void SetStamina(float value)
    {
        if (!isOwner) return; // only owner can write
        currentStamina.value = value;
    }

    void HandleStaminaChanged(float newStamina)
    {
        OnStaminaUpdated?.Invoke(newStamina);
    }
    #endregion
}
