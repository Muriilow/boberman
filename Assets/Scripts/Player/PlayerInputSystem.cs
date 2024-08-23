using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInputSystem: MonoBehaviour
{
    private PlayerInput playerInput;
    public InputActionReference moveAction;
    public Vector2 direction;
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        direction = moveAction.action.ReadValue<Vector2>();
    }

}
