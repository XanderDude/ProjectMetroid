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
		//test
		if (Input.GetAxis("Left", "Right") == 1)
		{
			raven.Xoffset = new Vector3(0.5f, 0, 0);
			raven.RotationDegrees = new Vector3(0, 180, 0);
		}
		else if (Input.GetAxis("Left", "Right") == -1)
		{
			raven.Xoffset = new Vector3(-0.5f, 0, 0);
			raven.RotationDegrees = new Vector3(0, 0, 0);
		}
	   
		raven.direction = raven.GlobalPosition.DirectionTo(raven.player.GlobalPosition + raven.Yoffset + raven.Xoffset);

		raven.Velocity = (raven.direction) * raven.speed;

		raven.isOnPlayer = (raven.player.GlobalPosition + raven.Yoffset + raven.Xoffset).DistanceTo(raven.GlobalPosition) < 1.2f;
	   
		if (raven.isOnPlayer)
		{
			rsm.TransitionTo("RavenOnPlayerState");
		}
		

	}

	public override void HandleInput(InputEvent @event)
	{
		

		if (@event.IsActionPressed("RavenSpecial") && raven.isOnPlayer)
		{
			rsm.TransitionTo("RavenLaunchState");
		}

		else if (@event.IsActionPressed("RavenSlash") && raven.isOnPlayer)
		{

			rsm.TransitionTo("RavenAttackState");
			
		}

	}

}
