using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable, IEnemyMoveable, ITriggerCheckabled
{
    [field: SerializeField] public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }
    public Rigidbody2D Rigidbody { get; set; }
    public bool IsFacingRight { get; set; } = true;

    /*----State Machine Variables----*/
    public EnemyStateMachine StateMachine { get; set; }
    public EnemyAttackState AttackState { get; set; }
    public EnemyIdleState IdleState { get; set; }
    public EnemyChaseState ChaseState { get; set; }

    /*----Idle Variables----*/
    public float randomMovementRange = 5f;
    public float randomMovementSpeed = 1f;

    /*----Aggro----*/
    public bool IsAggroed { get; set; }
    public bool IsWithinStrikingDistance { get; set; }


    public Rigidbody2D bulletPrefab;
    private void Awake()
    {
        StateMachine = new EnemyStateMachine();

        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        AttackState = new EnemyAttackState(this, StateMachine);
    }
    void Start()
    {
        CurrentHealth = MaxHealth;
        Rigidbody = GetComponent<Rigidbody2D>();

        StateMachine.Initialize(IdleState);
    }

    void Update()
    {
        StateMachine.CurrentEnemyState.FrameUpdate();
    }

    void FixedUpdate()
    {
        StateMachine.CurrentEnemyState.PhysicsUpdate();
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
            Die();
        }
    }

    public void Die()
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
        StateMachine.CurrentEnemyState.AnimationTriggerEvent(triggerType);
    }
    public enum AnimationtriggerType
    {
        EnemyDamaged,
        PlayFootstepSound,
    }
    #endregion
}
