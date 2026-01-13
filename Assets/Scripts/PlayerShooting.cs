using PurrNet;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] PlayerAim playerAim;
    [SerializeField] Transform throwOrigin;
    [SerializeField] LineRenderer trajectoryLine;
    [SerializeField] Camera playerCamera;

    [Header("Throw Settings")]
    [SerializeField] float throwForce = 18f;
    [SerializeField] int trajectoryPoints = 30;
    [SerializeField] float timeStep = 0.05f;
    [SerializeField] LayerMask collisionMask;

    [Header("Ball")]
    [SerializeField] GameObject ballPrefab;


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

        trajectoryLine.enabled = false;
    }

    void Update()
    {
        if (!isOwner) return;

        if (playerAim.IsAiming)
            DrawTrajectory();
        else
            trajectoryLine.enabled = false;

        if (input.Player.Attack.WasPressedThisFrame())
            Shoot();
    }

    void Shoot()
    {
        Vector3 direction = GetShootDirection();

        RequestShoot(direction);
    }

    void DrawTrajectory()
    {
        trajectoryLine.enabled = true;
        trajectoryLine.positionCount = trajectoryPoints;

        Vector3 startPos = throwOrigin.position;
        Vector3 startVelocity = GetShootDirection() * throwForce;


        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * timeStep;
            Vector3 point = startPos + startVelocity * t + 0.5f * Physics.gravity * t * t;

            if (i > 0)
            {
                Vector3 prev = trajectoryLine.GetPosition(i - 1);
                Vector3 dir = point - prev;

                if (Physics.Raycast(prev, dir, out RaycastHit hit, dir.magnitude, collisionMask))
                {
                    trajectoryLine.positionCount = i + 1;
                    trajectoryLine.SetPosition(i, hit.point);
                    return;
                }
            }

            trajectoryLine.SetPosition(i, point);
        }
    }

    Vector3 GetShootDirection()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 baseDirection;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, collisionMask))
            baseDirection = (hit.point - throwOrigin.position).normalized;
        else
            baseDirection = ray.direction;

        float spread = playerAim.IsAiming ? 0f : 2.5f;
        baseDirection = Quaternion.Euler(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0f) * baseDirection;

        return baseDirection;
    }



    [ServerRpc]
    void RequestShoot(Vector3 direction)
    {
        if (!isServer) return;

        GameObject ball = Instantiate(ballPrefab, throwOrigin.position, Quaternion.identity);
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * throwForce, ForceMode.VelocityChange);
    }

}
