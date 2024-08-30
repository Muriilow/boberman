using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInputSystem: MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerManager playerManager;
    public InputActionReference moveAction;
    public Vector2 direction;
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerManager = GetComponent<PlayerManager>();
    }

    void Update()
    {
        direction = moveAction.action.ReadValue<Vector2>();

        if (direction.normalized == Vector2.zero)
            playerManager.SetWalking(false);
        else
            playerManager.SetWalking(true);
    }

}
