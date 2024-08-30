using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void EnterState() { }
    void ExitState() { }
    void FrameUpdate() { }
    void PhysicsUpdate() { }
    void AnimationTriggerEvent(Enemy.AnimationtriggerType triggerType) { }

}
