using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyState
{

    private Vector3 _targetPos;
    private Vector3 _velocity;
    public EnemyIdleState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) 
    {

    }

    public override void EnterState() 
    {
        base.EnterState();

        _targetPos = GetRandomPointInCircle();
    }
    public override void ExitState() { }
    public override void FrameUpdate() 
    {
        base.FrameUpdate();

        if(enemy.IsAggroed)
            enemy.StateMachine.ChangeState(enemy.ChaseState);
        
        _velocity = (_targetPos - enemy.transform.position).normalized;

        enemy.MoveEnemy(_velocity * enemy.randomMovementSpeed);

        if((enemy.transform.position - _targetPos).sqrMagnitude < 0.01)
            _targetPos = GetRandomPointInCircle();
    }
    public override void PhysicsUpdate() { }
    public override void AnimationTriggerEvent(Enemy.AnimationtriggerType triggerType) { }

    private Vector3 GetRandomPointInCircle() => enemy.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * enemy.randomMovementRange;
}
