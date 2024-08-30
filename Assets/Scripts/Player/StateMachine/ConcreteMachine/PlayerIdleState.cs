using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerManager player, PlayerMovement playerMovement, PlayerInputSystem playerInput, StateMachine<PlayerState> playerStateMachine) : base(player, playerMovement, playerInput, playerStateMachine)
    {
    }

    public override void AnimationTriggerEvent(PlayerManager.PlayerAnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }

    public override void EnterState()
    {
        base.EnterState();
        Debug.Log("Idle rn");
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        if (player.IsWalking)
            playerStateMachine.ChangeState(player.WalkingState);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

}
