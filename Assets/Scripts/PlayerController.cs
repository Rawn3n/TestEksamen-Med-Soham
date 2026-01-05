using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    #region Inspector

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dodgeForce = 8f;
    [SerializeField] private float dodgeCooldown = 0.6f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Animator animator;

    #endregion

    #region Private Fields

    private Rigidbody rb;
    private float lastDodgeTime;
    private float moveInput;

    #endregion

    #region Netcode
    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        if (!IsOwner)
        {
            playerCamera.enabled = false;
            GetComponentInChildren<CinemachineBrain>().enabled = false;
        }
    }

    #endregion

    #region Unity Callbacks
    void Update()
    {
        if (!IsOwner) return;

        ReadMovement();
        HandleDodge();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        Move();
    }

    #endregion

    #region Movement
    void ReadMovement()
    {
        moveInput =
            (Keyboard.current.dKey.isPressed ? 1 : 0) -
            (Keyboard.current.aKey.isPressed ? 1 : 0);

        animator.SetFloat("Speed", Mathf.Abs(moveInput));
    }

    void Move()
    {
        rb.linearVelocity = new Vector3(
            moveInput * moveSpeed,
            rb.linearVelocity.y,
            rb.linearVelocity.z
        );
    }

    #endregion

    #region Dodge
    void HandleDodge()
    {
        if (Time.time < lastDodgeTime + dodgeCooldown) return;

        bool shiftHeld = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;

        if (!shiftHeld) return;

        if (Keyboard.current.aKey.wasPressedThisFrame)
            Dodge(-1);

        if (Keyboard.current.dKey.wasPressedThisFrame)
            Dodge(1);
    }

    void Dodge(int direction)
    {
        lastDodgeTime = Time.time;

        animator.SetTrigger("Dodge");

        // Animation is left by default → flip for right dodge
        transform.localScale = new Vector3(direction, 1, 1);

        rb.AddForce(Vector3.right * direction * dodgeForce, ForceMode.VelocityChange);
    }

    #endregion
}
