using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle-Random Wander", menuName = "Enemy Logic/Idle Logic/Random Wander")]
public class EnemyIdleRandomWander : EnemyIdleSOBase
{
    /*----Idle Variables----*/
    public float randomMovementRange = 5f;
    public float randomMovementSpeed = 1f;

    private Vector3 _targetPos;
    private Vector3 _velocity;
    public override void DoAnimationTriggerEventLogic(Enemy.AnimationtriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();

        _targetPos = GetRandomPointInCircle();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        _velocity = (_targetPos - enemy.transform.position).normalized;

        enemy.MoveEnemy(_velocity * randomMovementSpeed);

        if ((enemy.transform.position - _targetPos).sqrMagnitude < 0.01)
            _targetPos = GetRandomPointInCircle();
    }

    public override void DoPhysicsUpdateLogic()
    {
        base.DoPhysicsUpdateLogic();
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }
    private Vector3 GetRandomPointInCircle() => enemy.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * randomMovementRange;
}

