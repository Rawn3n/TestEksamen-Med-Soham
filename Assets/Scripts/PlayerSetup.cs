using PurrNet;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    [Header("Ignore")]
    [SerializeField] GameObject playerCamera;
    [SerializeField] GameObject crosshairCanvas;
    [SerializeField] AudioListener audioListener;

    protected override void OnSpawned()
    {
        if (!isOwner)
        {
            // Disable alle objekter det kun skal ses lokalt
            playerCamera.SetActive(false);
            crosshairCanvas.SetActive(false);
            audioListener.enabled = false;
        }
    }
}
