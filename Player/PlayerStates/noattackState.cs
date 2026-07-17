using Godot;
using System;

public partial class noattackState : PlayerState
{
	public override void HandleInput(InputEvent @event) //Called whenever an input is detected
	{
        if ((@event.IsActionPressed("Shoot") || @event.IsActionPressed("SpecialShoot")) 
        && pm.psm.current_node_state_name != "mantleState" && pm.psm.current_node_state_name != "deathState")
        {
            EmitSignal(SignalName.Transition, "attackState");
        }
    }
}
