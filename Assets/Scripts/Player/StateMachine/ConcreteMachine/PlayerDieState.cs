using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDieState : PlayerState
{
    public PlayerDieState(PlayerManager player, PlayerMovement playerMovement, PlayerInputSystem playerInput, StateMachine<PlayerState> playerStateMachine, Animator playerAnimator)
                          : base(player, playerMovement, playerInput, playerStateMachine, playerAnimator)
    {
    }

    public override void AnimationTriggerEvent(string triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }

    public override void EnterState()
    {
        base.EnterState();
        
        playerMovement.moveSpeed = 0;
        player.AnimationTriggerEvent("PlayerDying");
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();


    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

}
