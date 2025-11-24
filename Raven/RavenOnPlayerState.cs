using Godot;
using System;

public partial class RavenOnPlayerState : State
{
	
	private float timeAccumulator = 0.0f;
	
	public override void Enter()
	{
		raven.isInAction = false;
		raven.isOnPlayer = true;
		
	}
	
	public override void Exit()
	{
		raven.isOnPlayer = false;
	}
	
	public override void Update(float delta)
	{
		timeAccumulator += delta;
		


		
	}
	
	public override void PhysicsUpdate(float delta)
	{
		var followlocation = raven.player.GetNode<MeshInstance3D>("PlayerMesh/PlayerRavenFollow");
		Vector3 targetPosition = followlocation.GlobalPosition;
		Vector3 slope = (targetPosition - raven.GlobalPosition).Normalized();

		if (raven.GlobalPosition.DistanceTo(targetPosition) > 0.2f)
		{
			raven.GlobalPosition += slope * (raven.speed / 2) * delta;
			raven.Velocity = Vector3.Zero;
			if (raven.player.GlobalPosition > raven.GlobalPosition)
			{
				
				raven.RotationDegrees = new Vector3(0, 180, 0);
			}
			else if (raven.player.GlobalPosition < raven.GlobalPosition)
			{
				raven.RotationDegrees = new Vector3(0, 0, 0);
			}
		}
		if (raven.player.IsOnFloor() || raven.player.StateMachine._currentState.Name == "mantleState")
		{
			raven.canLaunch = true;
		}
	}
	
	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("RavenSpecial") && raven.canLaunch && raven.inventoryfriend.isUpgradeUnlocked("ravenTeleport"))
		{
			
			raven.GlobalPosition = raven.player.GlobalPosition + raven.Yoffset;
			
			rsm.TransitionTo("RavenLaunchState");
		}
		else if (@event.IsActionPressed("RavenSlash"))
		{
			raven.GlobalPosition = raven.player.GlobalPosition + raven.Yoffset;
			rsm.TransitionTo("RavenAttackState");
		}
	}
}
