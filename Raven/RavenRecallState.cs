using Godot;
using System;

public partial class RavenRecallState : State
{
	public override void Enter()
	{
		//GD.Print("Raven: Entered Recall State");
		raven.isInAction = false;

	}

	public override void Exit()
	{
		//GD.Print("Raven: Exited Recall State");
	  
	}

	public override void Update(float delta)
	{
	}

	public override void PhysicsUpdate(float delta)
	{
		
	   
		raven.direction = raven.GlobalPosition.DirectionTo(raven.player.GlobalPosition + raven.Yoffset + raven.Xoffset).Normalized();

		raven.Velocity = (raven.direction) * raven.speed;

		raven.isOnPlayer = (raven.player.GlobalPosition + raven.Yoffset + raven.Xoffset).DistanceTo(raven.GlobalPosition) < 0.1f;
	    
		if (raven.player.GlobalPosition > raven.GlobalPosition)
			{
				
				raven.RotationDegrees = new Vector3(0, 180, 0);
			}
			else if (raven.player.GlobalPosition < raven.GlobalPosition)
			{
				raven.RotationDegrees = new Vector3(0, 0, 0);
			}
		
		if (raven.isOnPlayer)
		{
			rsm.TransitionTo("RavenOnPlayerState");
		}
		

	}

}
