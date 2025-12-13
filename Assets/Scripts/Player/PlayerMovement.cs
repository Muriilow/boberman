using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float maxMoveSpeed = 9f;
    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] public PlayerInputSystem playerInput;
    [SerializeField] public Vector2 direction;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        Move(playerInput.direction);
    }

    #region Basic Movement
    private void Move(Vector3 input)
    {
        _rigidBody.linearVelocity = input * moveSpeed;
    }
    private bool CanMove(Vector3 input)
    {
        //PlaceHolder
        return true;
    }

    public void AddMoveSpeed(int amount)
    {
        if (moveSpeed + amount < maxMoveSpeed)
            moveSpeed += amount;
        else
            moveSpeed = maxMoveSpeed;
    }
    #endregion
}
