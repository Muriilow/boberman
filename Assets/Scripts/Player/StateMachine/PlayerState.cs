using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : IState
{
    //Pretty much to handle animation 
    protected PlayerManager player;
    protected StateMachine<PlayerState> playerStateMachine;
    protected PlayerInputSystem playerInput;
    protected PlayerMovement playerMovement;

    public PlayerState(PlayerManager player, PlayerMovement playerMovement, PlayerInputSystem playerInput, StateMachine<PlayerState> playerStateMachine)
    {
        this.player = player;
        this.playerStateMachine = playerStateMachine;
        this.playerMovement = playerMovement;
        this.playerInput = playerInput;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void FrameUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void AnimationTriggerEvent(PlayerManager.PlayerAnimationTriggerType triggerType) { }
}
