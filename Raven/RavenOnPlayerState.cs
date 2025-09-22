using Godot;
using System;

public partial class RavenOnPlayerState : State
{
	private float followSpeed = 3.0f;
	private float bobAmount = 0.3f;
	private float bobSpeed = 2.0f;
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
		
		if (Input.GetAxis("Left", "Right") == 1)
		{
			raven.Xoffset = new Vector3(-1.0f, 0, 0);
			raven.RotationDegrees = new Vector3(0, 180, 0);
		}
		else if (Input.GetAxis("Left", "Right") == -1)
		{
			raven.Xoffset = new Vector3(1.0f, 0, 0);
			raven.RotationDegrees = new Vector3(0, 0, 0);
		}
	}
	
	public override void PhysicsUpdate(float delta)
	{
	    Vector3 targetPosition = raven.player.GlobalPosition + raven.Yoffset + raven.Xoffset;
		raven.GlobalPosition = targetPosition;
		raven.Velocity = Vector3.Zero;
		
		if (raven.player.IsOnFloor())
        {
			raven.canLaunch = true;
		}
	}
	
	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("RavenSpecial") && raven.canLaunch)
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
