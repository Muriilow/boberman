using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T> where T : IState
{
    public T CurrentState { get; set; }

    public void Initialize(T startingState)
    {
        CurrentState = startingState;
        CurrentState.EnterState();
    }

    public void ChangeState(T newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        newState.EnterState();
    }
}
