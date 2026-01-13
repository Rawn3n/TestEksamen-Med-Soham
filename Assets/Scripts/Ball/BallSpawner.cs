using UnityEngine;
using PurrNet;

public class BallSpawner : NetworkBehaviour
{
    [Header("Ball Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float respawnDelay = 1f;

    private GameObject currentBall;
    private int nextSpawnIndex = 0;

    protected override void OnSpawned(bool asServer)
    {
        base.OnSpawned(asServer);

        // Only the server/host should spawn balls automatically
        if (asServer)
        {
            SpawnBall();
        }
    }

    //private void Update()
    //{
    //    // Debug: press E to destroy the current ball
    //    if (!isServer) return; // Only server handles destruction
    //    if (Input.GetKeyDown(KeyCode.F))
    //    {
    //        DestroyCurrentBall();
    //    }
    //}

    private void SpawnBall()
    {
        if (currentBall != null) return;

        Transform spawnPoint = spawnPoints[nextSpawnIndex];
        nextSpawnIndex++;
        if (nextSpawnIndex >= spawnPoints.Length)
        {
            nextSpawnIndex = 0;

        }

        currentBall = Instantiate(ballPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    private void DestroyCurrentBall()
    {
        if (currentBall == null) return;

        Destroy(currentBall);
        currentBall = null;

        Invoke(nameof(SpawnBall), respawnDelay);
    }
}
