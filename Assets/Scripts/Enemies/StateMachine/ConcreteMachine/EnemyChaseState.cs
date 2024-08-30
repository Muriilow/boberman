using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyState
{
    

    public EnemyChaseState(Enemy enemy, StateMachine<EnemyState> enemyStateMachine) : base(enemy, enemyStateMachine) { }
    public override void EnterState() 
    {
        base.EnterState();
        enemy.EnemyChaseBaseInstance.DoEnterLogic();
    }
    public override void ExitState() 
    {
        base.ExitState();
        enemy.EnemyChaseBaseInstance.DoExitLogic();
    }
    public override void FrameUpdate() 
    {
        base.FrameUpdate();
        enemy.EnemyChaseBaseInstance.DoFrameUpdateLogic();
    }
    public override void PhysicsUpdate() 
    {
        base.PhysicsUpdate();
        enemy.EnemyChaseBaseInstance.DoPhysicsUpdateLogic();
    }
    public override void AnimationTriggerEvent(Enemy.AnimationtriggerType triggerType) 
    {
        base.AnimationTriggerEvent(triggerType);
        enemy.EnemyChaseBaseInstance.DoAnimationTriggerEventLogic(triggerType);
    }
}
