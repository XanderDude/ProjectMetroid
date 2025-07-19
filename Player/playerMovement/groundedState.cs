using Godot;
using System;

public partial class groundedState : State
{
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 15.0f;
	[Export] public float groundDeacceleration = 15.0f;

	public override void Enter()
	{
		GD.Print("Entered Grounded State");
		if (Input.GetAxis("Left", "Right") != 0 && Input.IsActionPressed("Slide")) //player wants to slide so let them
		{
			GD.Print("Boost Slide");
			msm.TransitionTo("slideState");
		}
		else player.Set(PlayerManager.PropertyName.slideBoost, false); //player is not sliding so player cannot retain boost
	}
	
	public override void Exit() {
		GD.Print("Exited Grounded State.");
	}

	public override void PhysicsUpdate(float delta)
	{
		if (!player.IsOnFloor()) //immediately switch to jump state
		{
			msm.TransitionTo("jumpState");
		}
		else if (Input.IsActionPressed("Slide") && Input.GetAxis("Left", "Right") != 0)
		{
			player.Set(PlayerManager.PropertyName.slideQueued, true);
			msm.TransitionTo("slideState");
		}
		HandleGroundedMovement(delta);
		player.MoveAndSlide();
	}

	private void HandleGroundedMovement(float delta)
	{
		float input = Input.GetAxis("Left", "Right");
		Vector3 velocity = player.Velocity;
		velocity.X = Mathf.MoveToward(velocity.X, input * groundMaxSpeed, delta + groundAcceleration);
		velocity.X = Mathf.Clamp(velocity.X, -groundMaxSpeed, groundMaxSpeed);
		player.Velocity = velocity;
	}

	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Jump") && player.IsOnFloor())
		{
			player.Set("jumpQueued", true);
			msm.TransitionTo("jumpState");
		}
		else player.Set("jumpQueued", false);
	}

}
