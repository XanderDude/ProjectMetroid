using Godot;
using System;

public partial class noattackState : State
{
	public override void HandleInput(InputEvent @event) //Called whenever an input is detected
	{
        if ((@event.IsActionPressed("Shoot") || @event.IsActionPressed("SpecialShoot")) 
        && pm.StateMachine._currentState.Name != "mantleState" && pm.StateMachine._currentState.Name != "deathState")
        {
            asm.TransitionTo("attackState");
        }
    }
}
