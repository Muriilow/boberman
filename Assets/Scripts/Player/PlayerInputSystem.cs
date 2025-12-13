using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInputSystem: MonoBehaviour
{
    private PlayerInput _playerInput;
    private PlayerManager _playerManager;
    public InputActionReference moveAction;
    public InputActionReference attackAction;
    public Vector2 direction;
    private void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerManager = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        direction = moveAction.action.ReadValue<Vector2>();
        _playerManager.SetWalking(direction.normalized != Vector2.zero);
    }

}
