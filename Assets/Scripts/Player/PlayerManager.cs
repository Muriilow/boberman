using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public abstract class PlayerManager : NetworkBehaviour, IDamageable
{
    public SpawnManager SpawnManagerRef { get; protected set; }
    public Animator PlayerAnimator { get; protected set; }
    public AnimationClip DyingAnimation { get; protected set; }
    public PlayerMovement PlayerMovement { get; protected set; }
    public PlayerInputSystem PlayerInput { get; protected set; }
    public PlayerBomb PlayerBomb { get; protected set; }

    public StateMachine<PlayerState> StateMachine { get; protected set; }
    public PlayerIdleState IdleState { get; protected set; }
    public PlayerWalkingState WalkingState { get; protected set; }
    public PlayerAttackingState AttackingState { get; protected set; }
    public PlayerDieState DieState { get; protected set; }

    public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }

    public bool IsWalking { get; set; }
    public bool IsAttacking { get; set; }
    
    public string CurrentState { get; protected set; }

    protected virtual void Awake()
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
        DieState = new PlayerDieState(this, PlayerMovement, PlayerInput, StateMachine, PlayerAnimator);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;
        
        if (IsServer)
            TrySpawnPlayer();
        else  
            RequestRespawnPlayerServerRpc();
        
        StateMachine.Initialize(IdleState);
        
        ManageRounds.Instance.OnGameOver += StopLogic;
        PlayerInput.enabled = true;
        
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        ManageRounds.Instance.OnGameOver -= StopLogic;
        base.OnNetworkDespawn();
    }

    protected virtual void StopLogic()
    {
        Debug.Log("Player stopped his logic");
        
        // Disable input and stop existing movement
        PlayerInput.enabled = false;
        PlayerInput.direction = Vector2.zero;
        IsWalking = false;
        
        StateMachine.ChangeState(IdleState);
    }

    [Rpc(SendTo.Server)]
    private void RequestRespawnPlayerServerRpc()
    {   
        TrySpawnPlayer();
    }

    private void TrySpawnPlayer()
    {
        if (!SpawnManagerRef.CanSpawn())
        {
            StartCoroutine(RetrySpawn());
            return;
        }

        SpawnPlayerServerRpc();
    }

    private IEnumerator RetrySpawn()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (IsServer)
            TrySpawnPlayer();
        else  
            RequestRespawnPlayerServerRpc();
    }


    protected virtual void Update()
    {
        if (!IsOwner)
            return;

        StateMachine.CurrentState.FrameUpdate();
    }
    protected virtual void FixedUpdate()
    {
        if (!IsOwner)
            return;

        StateMachine.CurrentState.PhysicsUpdate();
    }
    
    [Rpc(SendTo.Server)]
    private void SpawnPlayerServerRpc()
    {
        var spawnPoint = SpawnManagerRef.GetSpawnPoint();
        transform.position = spawnPoint;
        SetPlayerPosClientRpc(spawnPoint);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerPosClientRpc(Vector3 spawnPoint)
    {
        transform.position = spawnPoint;
    }

    #region Damageable
    public virtual void Damage(float damageAmount)
    {
        if (!IsOwner)
            return;

        if (CurrentHealth <= 0f)
            return;

        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0f)
            StateMachine.ChangeState(DieState);
    }

    [Rpc(SendTo.Server)]
    public void DieServerRpc()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }

    #endregion
    #region Animation Triggers
    public virtual void AnimationTriggerEvent(string newState)
    {
        if (!IsOwner || CurrentState == newState)
            return;

        PlayerAnimator.Play(newState);

        CurrentState = newState;
    }
    #endregion
}
