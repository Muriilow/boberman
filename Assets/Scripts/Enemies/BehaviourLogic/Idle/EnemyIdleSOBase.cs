using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleSOBase : ScriptableObject
{
    protected Enemy enemy;
    protected Transform transform;
    protected GameObject gameObject;

    protected Transform playerTransform;

    public virtual void Initialize(GameObject gameObject, Enemy enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;

        GameObject _player = GameObject.FindGameObjectWithTag("Player");
        if (_player != null)
            playerTransform = _player.transform;
    }

    public virtual void DoEnterLogic() { }
    public virtual void DoExitLogic() { ResetValues(); }
    public virtual void DoFrameUpdateLogic() 
    {
        if (enemy.IsAggroed)
            enemy.StateMachine.ChangeState(enemy.ChaseState);
    }
    public virtual void DoPhysicsUpdateLogic() { }
    public virtual void DoAnimationTriggerEventLogic(Enemy.AnimationtriggerType triggerType) { }
    public virtual void ResetValues() { }
}
