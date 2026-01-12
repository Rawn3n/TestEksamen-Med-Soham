using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using PurrNet;

public class PlayerController : NetworkBehaviour
{
    [Header("Mouse Look")]
    [SerializeField] float mouseSensitivity = 0.1f;
    [SerializeField] Transform cameraPivot;
    [SerializeField] CinemachineCamera cmCamera;
    [SerializeField] PlayerAim playerAim;

    PlayerInputActions input;
    float xRotation;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        input = new PlayerInputActions();

        if (isOwner)
        {
            input.Enable();
            cmCamera.Priority = 20;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            cmCamera.Priority = 0;
        }
    }

    void OnDisable()
    {
        input?.Disable();
    }

    void Update()
    {
        if (!isOwner) return;
        HandleMouseLook();
    }

    void HandleMouseLook()
    {
        float sensMultiplier = playerAim != null ? playerAim.GetSensitivityMultiplier() : 1f;

        Vector2 mouseDelta = input.Player.Look.ReadValue<Vector2>() * mouseSensitivity * sensMultiplier;

        xRotation -= mouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseDelta.x);
    }
}
