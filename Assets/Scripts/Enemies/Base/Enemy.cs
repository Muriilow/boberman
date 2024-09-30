using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable, IEnemyMoveable, ITriggerCheckabled
{
    [field: SerializeField] public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }
    public Rigidbody2D Rigidbody { get; set; }
    public bool IsFacingRight { get; set; } = true;

    /*----State Machine Variables----*/
    public StateMachine<EnemyState> StateMachine { get; set; }
    public EnemyAttackState AttackState { get; set; }
    public EnemyIdleState IdleState { get; set; }
    public EnemyChaseState ChaseState { get; set; }


    /*----Aggro----*/
    public bool IsAggroed { get; set; }
    public bool IsWithinStrikingDistance { get; set; }

    /*----ScriptableObject Variables----*/
    [SerializeField] private EnemyIdleSOBase EnemyIdleBase;
    [SerializeField] private EnemyChaseSOBase EnemyChaseBase;
    [SerializeField] private EnemyAttackSOBase EnemyAttackBase;

    public EnemyIdleSOBase EnemyIdleBaseInstance { get; set; }
    public EnemyChaseSOBase EnemyChaseBaseInstance { get; set; }
    public EnemyAttackSOBase EnemyAttackBaseInstance { get; set; }

    
    private void Awake()
    {
        EnemyIdleBaseInstance = Instantiate(EnemyIdleBase);
        EnemyChaseBaseInstance = Instantiate(EnemyChaseBase);
        EnemyAttackBaseInstance = Instantiate(EnemyAttackBase);

        StateMachine = new StateMachine<EnemyState>();

        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        AttackState = new EnemyAttackState(this, StateMachine);
    }
    void Start()
    {
        CurrentHealth = MaxHealth;
        Rigidbody = GetComponent<Rigidbody2D>();

        EnemyIdleBaseInstance.Initialize(gameObject, this);
        EnemyChaseBaseInstance.Initialize(gameObject, this);
        EnemyAttackBaseInstance.Initialize(gameObject, this);

        StateMachine.Initialize(IdleState);
    }

    void Update()
    {
        StateMachine.CurrentState.FrameUpdate();
    }

    void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }

    #region Trigger
    public void SetAggroStatus(bool isAggroed)
    {
        IsAggroed = isAggroed;
    }

    public void SetStrikingDistanceBool(bool isWithinStrikingDistance)
    {
        IsWithinStrikingDistance = isWithinStrikingDistance;
    }
    #endregion
    #region Damageable

    public void Damage(float damageAmount)
    {
        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0f)
        {
            DieServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    public void DieServerRpc()
    {
        Destroy(gameObject);
    }
    #endregion

    #region EnemyMoveable
    public void MoveEnemy(Vector2 velocity)
    {
        Rigidbody.velocity = velocity;
        CheckForLeftOrRightFacing(velocity);
    }
    public void CheckForLeftOrRightFacing(Vector2 velocity)
    {
        if (IsFacingRight && velocity.x < 0f)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            IsFacingRight = !IsFacingRight;
        }
        else if (!IsFacingRight && velocity.x > 0f)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            IsFacingRight = !IsFacingRight;
        }
    }
    #endregion

    #region Animation Triggers

    private void AnimationTriggerEvent(AnimationtriggerType triggerType)
    {
        StateMachine.CurrentState.AnimationTriggerEvent(triggerType);
    }
    public enum AnimationtriggerType
    {
        EnemyDamaged,
        PlayFootstepSound,
    }
    #endregion
}
