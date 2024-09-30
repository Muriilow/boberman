using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : NetworkBehaviour, IDamageable
{
    public SpawnManager SpawnManagerRef { get; private set; }
    public Animator PlayerAnimator { get; private set; }
    public AnimationClip DyingAnimation { get; private set; }
    public PlayerMovement PlayerMovement { get; private set; }
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

    public string CurrentState { get; private set; }

    private void Awake()
    {
        MaxHealth = 1;
        CurrentHealth = MaxHealth;
        SpawnManagerRef = FindFirstObjectByType<SpawnManager>();
        PlayerAnimator = GetComponent<Animator>();
        PlayerMovement = GetComponent<PlayerMovement>();
        PlayerInput = GetComponent<PlayerInputSystem>();
        StateMachine = new StateMachine<PlayerState>();

        IdleState = new PlayerIdleState(this, PlayerMovement, PlayerInput, StateMachine, PlayerAnimator);
        WalkingState = new PlayerWalkingState(this, PlayerMovement, PlayerInput, StateMachine, PlayerAnimator);
        AttackingState = new PlayerAttackingState(this, PlayerMovement, PlayerInput, StateMachine, PlayerAnimator);
    }

    public override void OnNetworkSpawn()
    {

        base.OnNetworkSpawn();

        StateMachine.Initialize(IdleState);
        SpawnPlayerServerRpc();
    }


    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
            return;

        StateMachine.CurrentState.FrameUpdate();
    }
    void FixedUpdate()
    {
        if (!IsOwner)
            return;

        StateMachine.CurrentState.PhysicsUpdate();
    }

    [Rpc(SendTo.Server)]
    private void SpawnPlayerServerRpc()
    {
        Vector3Int spawnPoint = SpawnManagerRef.GetSpawnPoint();
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
            AnimationTriggerEvent("PlayerDying");
        }
    }

    [Rpc(SendTo.Server)]
    public void DieServerRpc()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }

    #endregion
    #region Animation Triggers
    public void AnimationTriggerEvent(string newState)
    {
        if (!IsOwner)
            return;
        if (CurrentState == newState)
            return;

        PlayerAnimator.Play(newState);

        CurrentState = newState;
    }
    #endregion
}
