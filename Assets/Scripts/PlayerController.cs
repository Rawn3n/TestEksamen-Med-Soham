using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkIdentity
{
    [SerializeField] private float moveSpeed = 5f;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;

        if (isOwner)
            inputActions.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
            inputActions.Disable();
    }

    private void Update()
    {
        Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 movement = new Vector3(move.x, 0f, move.y);

        transform.position += movement * moveSpeed * Time.deltaTime;
    }
}
