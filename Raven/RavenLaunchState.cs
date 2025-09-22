using System;
using Godot;

public partial class RavenLaunchState : State
{
	public override void Enter()
	{
		//GD.Print("Raven: Entered Launch State");
		raven.canLaunch = false;
		

	}
	public override void Exit()
	{
	}
	public override void Update(float delta)
	{
	}
	public override void PhysicsUpdate(float delta)
	{
		RavenLaunch();
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


	public Vector3 RavenDirection()
	{
		Vector3 dir = Vector3.Zero;
		if (Input.IsActionPressed("Up")) dir.Y += 1;
		if (Input.IsActionPressed("Down")) dir.Y -= 1;
		if (Input.IsActionPressed("Left")) dir.X -= 1;
		if (Input.IsActionPressed("Right")) dir.X += 1;
		if (dir != Vector3.Zero)
			return new Vector3(Input.GetAxis("Left", "Right"), Input.GetAxis("Down", "Up"), 0).Normalized();

		else
			return new Vector3(Mathf.Sign(raven.player.GetNode<Node3D>("%PlayerMesh").RotationDegrees.Y), 0, 0);  
	}

	public void RavenLaunch()
	{

	
				
		if (raven.player == null)
		{
			GD.PrintErr("Raven: PlayerManager is null");
			return;
		}
		if (!raven.isInAction)
		{
			raven.direction = RavenDirection();
			raven.targetPosition = raven.GlobalPosition + (raven.direction * raven.maxDistance);
			raven.Velocity = raven.direction * raven.speed;

		}
		raven.isInAction = true;

		

		if (raven.direction != Vector3.Zero)
		{
		   
			
		   // GD.Print("Raven: Launched in Direction:  " + raven.direction);
			if (Mathf.Abs(raven.GlobalPosition.DistanceTo(raven.targetPosition)) < 0.1f)
			{
				
				//GD.Print("Raven: Reached target position");
				//GD.Print("Raven: targetPosition: " + raven.targetPosition);
				raven.Velocity = Vector3.Zero;
				rsm.TransitionTo("RavenRecallState");

			}
			else
			{


			  // GD.Print("Raven: Moving towards target position");
				//GD.Print("Raven: Current Position: " + raven.GlobalPosition);
				//GD.Print("Raven: targetPosition: " + raven.targetPosition);
			   
			}
		}
	}
}
