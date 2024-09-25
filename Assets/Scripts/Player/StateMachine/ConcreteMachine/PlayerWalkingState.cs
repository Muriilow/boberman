using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWalkingState : PlayerState
{
    public PlayerWalkingState(PlayerManager player, PlayerMovement playerMovement, PlayerInputSystem playerInput, StateMachine<PlayerState> playerStateMachine, Animator playerAnimator) : base(player, playerMovement, playerInput, playerStateMachine, playerAnimator)
    {
    }

    public override void AnimationTriggerEvent(string triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }
    public override void EnterState()
    {
        base.EnterState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {

        if (!player.IsWalking)
            playerStateMachine.ChangeState(player.IdleState);

        playerMovement.direction = playerMovement.playerInput.direction;

        bool inputUp = playerInput.direction.x == 0 && playerInput.direction.y == 1;
        bool inputDown = playerInput.direction.x == 0 && playerInput.direction.y == - 1;

        bool inputLeft = playerInput.direction.x == - 1 && playerInput.direction.y == 0;
        bool inputRight = playerInput.direction.x == 1 && playerInput.direction.y == 0;


        if (inputUp)
            player.AnimationTriggerEvent("PlayerWalkingStateUp");

        else if (inputDown)
            player.AnimationTriggerEvent("PlayerWalkingStateDown");

        else if (inputRight)
            player.AnimationTriggerEvent("PlayerWalkingStateRight");

        else if (inputLeft)
            player.AnimationTriggerEvent("PlayerWalkingStateLeft");

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
    }
}
