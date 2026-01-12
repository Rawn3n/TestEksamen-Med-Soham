using PurrNet;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] PlayerAim playerAim;

    PlayerInputActions input;

    protected override void OnSpawned()
    {
        if (!isOwner)
        {
            enabled = false;
            return;
        }

        input = new PlayerInputActions();
        input.Enable();
    }

    void Update()
    {
        if (!isOwner) return;

        if (input.Player.Attack.WasPressedThisFrame())
        {
            Shoot();
        }
    }

    void Shoot()
    {
        float spread = playerAim.IsAiming ? 0f : 2.5f;

        // raycast with spread
    }
}
