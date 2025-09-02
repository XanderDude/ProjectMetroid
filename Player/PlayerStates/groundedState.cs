using Godot;
using System;

public partial class groundedState : State
{
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 15.0f;
	[Export] public float groundDeacceleration = 15.0f;
	[Export] public float groundProjectileSpeed = 25.0f;
	public bool isFirstTimeShoot = false;

	public override void Enter()
	{
		/*
		GD.Print("Entered Grounded State");
		if (Input.IsActionPressed("Slide") && (bool)player.Get(PlayerManager.PropertyName.slideBoost) && Input.GetAxis("Left", "Right") != 0) //player wants to slide so let them
		{
			msm.TransitionTo("slideState");
		}
		else
		{
			//parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
			player.Set(PlayerManager.PropertyName.slideBoost, false); //player is not sliding so player cannot retain boost
		}*/
		player.Set(PlayerManager.PropertyName.slideBoost, false);
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
	}
	
	public override void Exit() {
		GD.Print("Exited Grounded State.");
	}

	public override void PhysicsUpdate(float delta)
	{
		
		
		if (Mathf.Abs(player.Velocity.X) >= .1f) {
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
		}
		if (!player.IsOnFloor()) //immediately switch to jump state
		{
			msm.TransitionTo("jumpState");
			return;
		}
		else if (Input.IsActionPressed("Slide") && Input.GetAxis("Left", "Right") != 0)
		{
			//player.Set(PlayerManager.PropertyName.slideQueued, true);
			msm.TransitionTo("slideState");
		}
		else if (player.Velocity.X == 0 && Input.IsActionPressed("Crouch"))
		{
			//parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true);
		}
		
		//else parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
		HandleGroundedMovement(delta);
		player.MoveAndSlide();
		if (Input.GetAxis("Left", "Right") != 0 && player.Velocity.X == 0) //check if player is moving after MoveAndSlide
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).WallCollided();
		}
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
		if (@event.IsActionPressed("Up"))
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(false);
			msm.TransitionTo("crouchState");
		}
		if (@event.IsActionPressed("Jump"))
		{
			player.Set("jumpQueued", true);
			msm.TransitionTo("jumpState");
		}
		else player.Set("jumpQueued", false);
		
		if (@event.IsActionPressed("Shoot") || @event.IsActionPressed("SpecialShoot"))
		{
			asm.TransitionTo("attackState");
		}
		

	}

}
