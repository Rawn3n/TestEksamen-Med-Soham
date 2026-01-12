using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using PurrNet;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float dodgeForce = 15f;
    [SerializeField] float dodgeDuration = 0.25f;
    [SerializeField] float dodgeCooldown = 1.2f;
    [SerializeField] float sprintMultiplier = 1.5f;

    bool isDodging;
    bool canDodge = true;

    [Header("Jump")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] LayerMask groundMask;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.2f;
    bool isGrounded;

    Rigidbody rb;
    PlayerInputActions input;

    // ========== STAMINA NETWORK SYNC ==========
    [Header("Stamina")]
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaRegenRate = 20f;
    [SerializeField] float sprintStaminaDrain = 15f;
    [SerializeField] float dodgeStaminaCost = 35f;

    public SyncVar<float> currentStamina = new SyncVar<float>(0f, ownerAuth: true);
    public float StaminaNormalized =>
        maxStamina <= 0f ? 0f : currentStamina.value / maxStamina;
    public event Action<float> OnStaminaUpdated;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        rb = GetComponent<Rigidbody>();
        input = new PlayerInputActions();

        currentStamina.onChanged += HandleStaminaChanged;

        if (isOwner)
        {
            input.Enable();
            currentStamina.value = maxStamina;
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

    // ---------- MOVEMENT ----------
    void HandleMovement()
    {
        if (isDodging) return;

        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
        Vector3 moveDir = transform.forward * moveInput.y + transform.right * moveInput.x;

        float speed = moveSpeed;

        if (input.Player.Sprint.IsPressed() && currentStamina.value > 0f)
        {
            speed *= sprintMultiplier;
            SetStamina(Mathf.Max(0f, currentStamina.value - sprintStaminaDrain * Time.fixedDeltaTime));
        }

        var v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(moveDir.x * speed, v.y, moveDir.z * speed);
    }

    // ---------- DODGE ----------
    void HandleDodge()
    {
        if (!canDodge || currentStamina.value < dodgeStaminaCost) return;

        if (input.Player.Dodge.WasPressedThisFrame())
        {
            Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
            if (moveInput.sqrMagnitude < 0.01f) return;

            SetStamina(Mathf.Max(0f, currentStamina.value - dodgeStaminaCost));

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

    // ---------- JUMP ----------
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

    // ---------- STAMINA ----------
    void RegenerateStamina()
    {
        if (isDodging || input.Player.Sprint.IsPressed()) return;

        SetStamina(Mathf.Clamp(currentStamina.value + staminaRegenRate * Time.deltaTime, 0f, maxStamina));
    }

    void SetStamina(float value)
    {
        if (!isOwner) return;
        currentStamina.value = value;
    }

    void HandleStaminaChanged(float newValue)
    {
        OnStaminaUpdated?.Invoke(newValue);
    }
}
