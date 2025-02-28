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

        base.OnNetworkSpawn();

        _stateMachine.Initialize(IdleState);
        SpawnPlayerServerRpc();
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
        Vector3Int spawnPoint = _spawnManagerRef.GetSpawnPoint();
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
            _stateMachine.ChangeState(DieState);
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
        if (!IsOwner || CurrentState == newState)
            return;

        _playerAnimator.Play(newState);

        CurrentState = newState;
    }
    #endregion
}
