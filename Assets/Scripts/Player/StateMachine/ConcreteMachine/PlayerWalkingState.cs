using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWalkingState : PlayerState
{
    public PlayerWalkingState(PlayerManager player, PlayerMovement playerMovement, PlayerInputSystem playerInput, StateMachine<PlayerState> playerStateMachine) : base(player, playerMovement, playerInput, playerStateMachine)
    {
    }

    public override void AnimationTriggerEvent(PlayerManager.PlayerAnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }

    public override void EnterState()
    {
        base.EnterState();
        Debug.Log("walking rn");
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        playerMovement.direction = playerMovement.playerInput.direction;

        if (!player.IsWalking)
            playerStateMachine.ChangeState(player.IdleState);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

    }
}
