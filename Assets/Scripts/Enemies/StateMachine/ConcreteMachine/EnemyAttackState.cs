using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyAttackState : EnemyState
{
    public EnemyAttackState(Enemy enemy, StateMachine<EnemyState> enemyStateMachine) : base(enemy, enemyStateMachine) { }
    public override void EnterState() 
    {
        base.EnterState();
        enemy.EnemyAttackBaseInstance.DoEnterLogic();
    }
    public override void ExitState() 
    {
        base.ExitState();
        enemy.EnemyAttackBaseInstance.DoExitLogic();
    }
    public override void FrameUpdate()  
    {
        base.FrameUpdate();
        enemy.EnemyAttackBaseInstance.DoFrameUpdateLogic();
    }
    public override void PhysicsUpdate() 
    {
        base.PhysicsUpdate();
        enemy.EnemyAttackBaseInstance.DoPhysicsUpdateLogic();
    }
    public override void AnimationTriggerEvent(Enemy.AnimationtriggerType triggerType) 
    {
        base.AnimationTriggerEvent(triggerType);
        enemy.EnemyAttackBaseInstance.DoAnimationTriggerEventLogic(triggerType);
    }
}
