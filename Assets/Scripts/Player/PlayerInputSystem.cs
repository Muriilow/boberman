using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInputSystem: MonoBehaviour
{
    private PlayerInput _playerInput;
    private PlayerManager _playerManager;
    public InputActionReference moveAction;
    public InputActionReference attackAction;
    public Vector2 direction;
    void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerManager = GetComponent<PlayerManager>();
    }

    void Update()
    {
        direction = moveAction.action.ReadValue<Vector2>();
        if (direction.normalized == Vector2.zero)
            _playerManager.SetWalking(false);
        else
            _playerManager.SetWalking(true);
        
    }

}
