using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerManager : NetworkBehaviour, IDamageable
{
    private SpawnManager _spawnManagerRef;
    private Animator _playerAnimator;
    private AnimationClip _dyingAnimation;
    private PlayerMovement _playerMovement;
    private PlayerInputSystem _playerInput;
    public PlayerBomb playerBomb;

    private StateMachine<PlayerState> _stateMachine;
    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkingState WalkingState { get; private set; }
    public PlayerAttackingState AttackingState { get; set; }
    public PlayerDieState DieState { get; set; }

    public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }

    public bool IsWalking { get; private set; }
    public bool IsAttacking { get; set; }

    public string CurrentState { get; private set; }

    private void Awake()
    {
        MaxHealth = 1;
        CurrentHealth = MaxHealth;
        _spawnManagerRef = FindFirstObjectByType<SpawnManager>();
        _playerAnimator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerInput = GetComponent<PlayerInputSystem>();
        _stateMachine = new StateMachine<PlayerState>();

        IdleState = new PlayerIdleState(this, _playerMovement, _playerInput, _stateMachine, _playerAnimator);
        WalkingState = new PlayerWalkingState(this, _playerMovement, _playerInput, _stateMachine, _playerAnimator);
        AttackingState = new PlayerAttackingState(this, _playerMovement, _playerInput, _stateMachine, _playerAnimator);
        DieState = new PlayerDieState(this, _playerMovement, _playerInput, _stateMachine, _playerAnimator);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;
        
        if (IsServer)
            TrySpawnPlayer();
        else  
            RequestRespawnPlayerServerRpc();
        
        _stateMachine.Initialize(IdleState);
        base.OnNetworkSpawn();
    }

    [Rpc(SendTo.Server)]
    private void RequestRespawnPlayerServerRpc()
    {   
        TrySpawnPlayer();
    }

    private void TrySpawnPlayer()
    {
        if (!_spawnManagerRef.CanSpawn())
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


    private void Update()
    {
        if (!IsOwner)
            return;

        _stateMachine.CurrentState.FrameUpdate();
    }
    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        _stateMachine.CurrentState.PhysicsUpdate();
    }
    
    [Rpc(SendTo.Server)]
    private void SpawnPlayerServerRpc()
    {
        var spawnPoint = _spawnManagerRef.GetSpawnPoint();
        transform.position = spawnPoint;
        SetPlayerPosClientRpc(spawnPoint);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerPosClientRpc(Vector3 spawnPoint)
    {
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
            _stateMachine.ChangeState(DieState);
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
        if (!IsOwner || CurrentState == newState)
            return;

        _playerAnimator.Play(newState);

        CurrentState = newState;
    }
    #endregion
}
