using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Windows;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerManager player, PlayerMovement playerMovement, PlayerInputSystem playerInput, StateMachine<PlayerState> playerStateMachine, Animator playerAnimator)
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
        Debug.Log("idle rn");

        if (player.CurrentState == "PlayerWalkingStateUp")
            player.AnimationTriggerEvent("PlayerIdleUp");

        else if (player.CurrentState == "PlayerWalkingStateDown")
            player.AnimationTriggerEvent("PlayerIdleDown");

        else if (player.CurrentState == "PlayerWalkingStateLeft")
            player.AnimationTriggerEvent("PlayerIdleLeft");

        else if (player.CurrentState == "PlayerWalkingStateRight")
            player.AnimationTriggerEvent("PlayerIdleRight");
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
