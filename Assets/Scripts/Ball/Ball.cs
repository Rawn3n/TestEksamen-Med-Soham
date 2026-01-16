using UnityEngine;
using PurrNet;

[RequireComponent(typeof(Rigidbody))]
public class Ball : NetworkBehaviour
{
    [Header("Collision")]
    [SerializeField] private string[] destroyTags = { "Enviorment", "Player" };

    [Header("Lifetime")]
    [SerializeField] private float maxLifetime = 5f;

    bool destroyed;

    protected override void OnSpawned()
    {
        if (isServer)
        {
            Invoke(nameof(DestroyBall), maxLifetime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer || destroyed)
            return;

        foreach (string tag in destroyTags)
        {
            if (!collision.gameObject.CompareTag(tag))
                continue;

            // Optional: player-specific logic
            //if (collision.collider.TryGetComponent<PlayerHealth>(out var player))
            //{
            //    player.TakeHit();
            //}

            DestroyBall();
            break;
        }
    }

    public void DestroyBall()
    {
        if (destroyed)
            return;

        destroyed = true;
        Destroy(gameObject);
    }
}
