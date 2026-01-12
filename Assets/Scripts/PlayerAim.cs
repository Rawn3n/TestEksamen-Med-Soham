using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerAim : NetworkBehaviour
{
    [SerializeField] CinemachineCamera cmCamera;
    [SerializeField] Crosshair crosshair;

    [Header("FOV")]
    [SerializeField] float baseFOV = 75f;
    [SerializeField] float aimFOV = 55f;
    [SerializeField] float zoomSpeed = 10f;

    [Header("Sensitivity")]
    [SerializeField] float aimSensitivityMultiplier = 0.6f;

    PlayerInputActions input;
    float targetFOV;

    public bool IsAiming { get; private set; }

    protected override void OnSpawned()
    {
        if (!isOwner)
        {
            enabled = false;
            return;
        }

        input = new PlayerInputActions();
        input.Enable();

        targetFOV = baseFOV;
        cmCamera.Lens.FieldOfView = baseFOV;
    }


    void Update()
    {
        if (!isOwner) return;

        HandleAimInput();

        cmCamera.Lens.FieldOfView = Mathf.Lerp(
            cmCamera.Lens.FieldOfView,
            targetFOV,
            Time.deltaTime * zoomSpeed
        );
    }

    void HandleAimInput()
    {
        if (input.Player.Aim.WasPressedThisFrame())
        {
            StartAim();
        }
        else if (input.Player.Aim.WasReleasedThisFrame())
        {
            StopAim();
        }
    }


    void StartAim()
    {
        IsAiming = true;
        targetFOV = aimFOV;
        crosshair.SetAiming(true);
    }

    void StopAim()
    {
        IsAiming = false;
        targetFOV = baseFOV;
        crosshair.SetAiming(false);
    }

    public float GetSensitivityMultiplier()
    {
        return IsAiming ? aimSensitivityMultiplier : 1f;
    }
}
