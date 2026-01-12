using UnityEngine;
using UnityEngine.UI;
using PurrNet;

public class StaminaSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    private PlayerMovement localPlayer;

    void Awake()
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
    }

    void Start()
    {
        TryFindLocalPlayer();
    }

    void Update()
    {
        // Hvis vi mister reference til localPlayer, prøv igen
        if (localPlayer == null)
        {
            TryFindLocalPlayer();
        }
    }

    void TryFindLocalPlayer()
    {
        foreach (var player in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
        {
            // Kun ejeren kan binde UI
            if (player.isOwner)
            {
                if (localPlayer != null)
                    localPlayer.OnStaminaUpdated -= OnStaminaChanged;

                localPlayer = player;

                // Abonner på SyncVar opdateringer
                localPlayer.OnStaminaUpdated += OnStaminaChanged;

                // Sæt slider initialt
                slider.value = localPlayer.StaminaNormalized;

                return;
            }
        }
    }

    void OnStaminaChanged(float newValue)
    {
        if (localPlayer == null) return;
        slider.value = localPlayer.StaminaNormalized;
    }

    void OnDestroy()
    {
        if (localPlayer != null)
            localPlayer.OnStaminaUpdated -= OnStaminaChanged;
    }
}
