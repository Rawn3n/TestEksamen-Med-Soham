using PurrNet;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    [Header("Local-only objects")]
    [SerializeField] GameObject playerCamera;
    [SerializeField] GameObject crosshairCanvas;

    protected override void OnSpawned()
    {
        if (isOwner)
            return;

        // Disable everything that should only exist locally
        playerCamera.SetActive(false);
        crosshairCanvas.SetActive(false);
    }
}
