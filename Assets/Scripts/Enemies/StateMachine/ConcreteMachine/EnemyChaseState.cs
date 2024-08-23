using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyState
{
    private Transform _playerTransform;
    private float _movementSpeed = 1.75f;
    public EnemyChaseState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) 
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override void EnterState() 
    {
        base.EnterState();

        Debug.Log("Hello from enter state");
    }
    public override void ExitState() { }
    public override void FrameUpdate() 
    {
        base.FrameUpdate();

        Vector2 moveDirection = (_playerTransform.position - enemy.transform.position).normalized;
        enemy.MoveEnemy(moveDirection * _movementSpeed);

        if (enemy.IsWithinStrikingDistance)
            enemy.StateMachine.ChangeState(enemy.AttackState);
    }
    public override void PhysicsUpdate() { }
    public override void AnimationTriggerEvent(Enemy.AnimationtriggerType triggerType) { }
}
