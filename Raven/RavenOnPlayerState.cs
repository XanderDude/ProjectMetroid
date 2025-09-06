using Godot;
using System;

public partial class RavenOnPlayerState : State
{
	public override void Enter()
	{
		//GD.Print("Raven: Entered OnPlayer State");
		raven.isInAction = false;
		raven.isOnPlayer = true;
	}

	public override void Exit()
	{
		//GD.Print("Raven: Exited OnPlayer State");
		raven.isOnPlayer = false;
	}

	public override void Update(float delta)
	{
		// Handle player movement direction for raven positioning
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
	}

	public override void PhysicsUpdate(float delta)
	{
		// Stick the raven to the player's position with offsets
		Vector3 targetPosition = raven.player.GlobalPosition + raven.Yoffset + raven.Xoffset;
		raven.GlobalPosition = targetPosition;
		raven.Velocity = Vector3.Zero; // No independent movement while on player
	}

	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("RavenSpecial"))
		{
			rsm.TransitionTo("RavenLaunchState");
		}
		else if (@event.IsActionPressed("RavenSlash"))
		{
			rsm.TransitionTo("RavenAttackState");
		}
	}
}
