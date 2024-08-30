using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : NetworkBehaviour, IDamageable
{
    public PlayerMovement PlayerMovement {  get; private set; }
    public PlayerInputSystem PlayerInput { get; private set; }
    public StateMachine<PlayerState> StateMachine { get; set; }
    //public PlayerAttackState AttackState { get; set; }
    public PlayerIdleState IdleState { get; set; }
    public PlayerWalkingState WalkingState { get; set; }
    public PlayerAttackingState AttackingState { get; set; }

    public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }

    public bool IsWalking { get; set; }
    public bool IsAtacking { get; set; }

    private void Awake()
    {
        PlayerMovement = GetComponent<PlayerMovement>();
        PlayerInput = GetComponent<PlayerInputSystem>();
        StateMachine = new StateMachine<PlayerState>();

        IdleState = new PlayerIdleState(this, PlayerMovement, PlayerInput, StateMachine);
        WalkingState = new PlayerWalkingState(this, PlayerMovement, PlayerInput, StateMachine);
        AttackingState = new PlayerAttackingState(this, PlayerMovement, PlayerInput, StateMachine);
    }

    void Start()
    {
        StateMachine.Initialize(IdleState);
        SpawnPlayer();
    }


    // Update is called once per frame
    void Update()
    {
        StateMachine.CurrentState.FrameUpdate();
    }
    void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }

    private void SpawnPlayer()
    {

        Vector3Int spawnPoint = SpawnManager.Instance.GetSpawnPoint();
        transform.position = spawnPoint;
    }

    #region Triggers
    public void SetWalking(bool isWalking)
    {
        IsWalking = isWalking;
    }

    #endregion
    #region Damageable
    public void Damage(float damageAmount)
    {
        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
    #endregion
    #region Animation Triggers
    private void AnimationTriggerEvent(PlayerAnimationTriggerType triggerType)
    {
        StateMachine.CurrentState.AnimationTriggerEvent(triggerType);
    }
    public enum PlayerAnimationTriggerType
    {
        PlayerDamaged,
        PlayFootstepSound,
    }
    #endregion
}
