using Godot;
using System;

public partial class groundedState : State
{
	[Export] public float groundInitSpeed = 1.0f;
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 15.0f;
	[Export] public float groundDeacceleration = 15.0f;

	//private Vector3 velocity;

	public override void Enter()
	{
		GD.Print("Entered Grounded State");
		if (msm.slideQueued && msm.slideBoost) //player wants to slide with a boost so let them
		{
			GD.Print("Slide Queued: " + msm.slideQueued);
			msm.TransitionTo("slideState");
		}
		else msm.slideBoost = false;
		//playerMesh.RotationDegrees = new Vector3(0, 0, 0);
	}
	
	public override void Exit() {
		GD.Print("Exited Grounded State");
	}

	public override void PhysicsUpdate(double delta)
	{
		if (!player.IsOnFloor()) //immediately switch to jump state
		{
			msm.TransitionTo("jumpState");
		}
		HandleGroundedMovement(delta);
		//CheckTransitions();
		player.MoveAndSlide();
	}

	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Jump") && player.IsOnFloor())
		{
			msm.jumpQueued = true;
			msm.TransitionTo("jumpState");
		}
		else msm.jumpQueued = false;
		if (@event.IsActionPressed("Slide") && player.Velocity.X > 0)
		{
			msm.slideQueued = true;
			msm.TransitionTo("slideState");
		}
		else msm.slideQueued = false;
	}

	private void HandleGroundedMovement(double delta)
	{
		Vector3 velocity = player.Velocity;
		if (velocity.X > groundMaxSpeed) velocity.X = groundMaxSpeed;
		else if (velocity.X < -groundMaxSpeed) velocity.X = -groundMaxSpeed;

		if (Input.IsKeyPressed(Key.Left))
		{
			velocity.X = -groundMaxSpeed;
			//GD.Print("Moving left, velocity.X = " + velocity.X);
		}
		else if (Input.IsKeyPressed(Key.Right))
		{
			velocity.X = groundMaxSpeed;
			//GD.Print("Moving right, velocity.X = " + velocity.X);
		}
		else
		{
			velocity.X = 0;
		}
		player.Velocity = velocity;
	}
}
