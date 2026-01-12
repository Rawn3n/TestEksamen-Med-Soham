using UnityEngine;
using PurrNet;

public class Ball : NetworkBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private string[] destroyTags = { "Wall", "Player" };

    // Only the server should handle destruction
    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer) return;

        foreach (string tag in destroyTags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                DestroyBall();
                break;
            }
        }
    }

    // Called by spawner or collision
    public void DestroyBall()
    {
        // Network-safe destroy
        Destroy(gameObject);
    }
}
