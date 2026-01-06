using UnityEngine;
using UnityEngine.UI;

public class StaminaSlider : MonoBehaviour
{
    [SerializeField] Slider slider;

    PlayerController localPlayer;

    void Awake()
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
    }

    void Update()
    {
        if (localPlayer == null)
        {
            TryFindLocalPlayer();
            return;
        }

        slider.value = localPlayer.StaminaNormalized;
    }

    void TryFindLocalPlayer()
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var player in players)
        {
            if (player.isOwner)
            {
                localPlayer = player;
                break;
            }
        }
    }
}
