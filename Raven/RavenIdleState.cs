using Godot;
using System;

public partial class RavenIdleState : State
{
	public override void Enter()
	{
		//GD.Print("Raven: Entered Idle State");

		
	}

	public override void Exit()
	{
	}

	public override void Update(float delta)
	{
	}

	public override void PhysicsUpdate(float delta)
	{
		raven.Velocity = Vector3.Zero;
		if ((raven.player.GlobalPosition + raven.Yoffset + raven.Xoffset).DistanceTo(raven.GlobalPosition) > 4.0f)
		{
			rsm.TransitionTo("RavenRecallState");
		}
	}

	public override void HandleInput(InputEvent @event)
	{
		

		if (@event.IsActionPressed("RavenSpecial"))
		{


				if (raven.canTeleport)
				{
					//GD.Print("Raven: Teleported to player position" + raven.player.StateMachine._currentState.Name);

					raven.player.GlobalPosition = raven.GlobalPosition;
					raven.canTeleport = false;
					raven.player.Velocity = Vector3.Zero;
				}


			if (raven.player.StateMachine._currentState.Name == "mantleState")
			{
				raven.player.StateMachine.TransitionTo("jumpState");  
			}

				rsm.TransitionTo("RavenRecallState");
		
				
				
				
				
			

		   
		}
	}


}
