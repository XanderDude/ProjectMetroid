using Godot;
using System;

public partial class groundedState : State
{
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 15.0f;
	public SlideVertColCheck vertColCheck;
	private float _landingCooldown = .1f; //3 frames (60fps)
	private float landingCDTimer;
	public override void Enter()
	{
		pm.slideBoost = false;
		if (msm._currentState.Name == "groundedState") parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded(); //could be crouching
		if (msm._previousState.Name == "jumpState") {
			GD.Print("Player Landed");
			landingCDTimer = 0; //start timer when landing
		}
	}
	
	public override void Exit() {
		landingCDTimer = _landingCooldown;
	}

	public override void PhysicsUpdate(float delta)
	{
		landingCDTimer += delta;

		if (!player.IsOnFloor() && landingCDTimer >= _landingCooldown) //immediately switch to jump state
		{
			msm.TransitionTo("jumpState");
			landingCDTimer = 0;
			//return;
		}
		
		HandleGroundedMovement(delta);
		player.MoveAndSlide();
		if (msm._currentState.Name == "groundedState" && pm.aimDirection.X != 0 && player.Velocity.X == 0) //check if player is trying to move
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).WallCollided(true);
		}
		else if (msm._currentState.Name == "groundedState" && player.Velocity.X != 0) parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).WallCollided(false);
	}

	private void HandleGroundedMovement(float delta)
	{
		//int input = Mathf.CeilToInt(Mathf.Abs(Input.GetAxis("Left", "Right"))) * Mathf.Sign(Input.GetAxis("Left", "Right")); //get absolute value of input (no negative), round up, multiply by sign to get direction
		Vector3 velocity = player.Velocity;
		velocity.Y -= _gravity * delta;
		velocity.X = Mathf.MoveToward(velocity.X, Mathf.Sign(pm.aimDirection.X) * groundMaxSpeed, delta + groundAcceleration);
		velocity.X = Mathf.Clamp(velocity.X, -groundMaxSpeed, groundMaxSpeed);
		player.Velocity = velocity;
	}


	public override void HandleInput(InputEvent @event) //Called whenever an input is detected
	{
		if (@event.IsActionPressed("forgemode"))
		{
			msm.TransitionTo("forgeState");
		}
		if (@event.IsActionPressed("Down") && pm.noAimDirection && msm._currentState.Name == "groundedState") //only crouch when previous frame had no aim direction
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true);
			msm.TransitionTo("crouchState");
		}
		
		if (Input.IsActionPressed("Slide") && pm.aimDirection.X != 0)
		{
			//player.Set(PlayerManager.PropertyName.slideQueued, true);
			msm.TransitionTo("slideState");
		}
		if (@event.IsActionPressed("Jump")) 
		{
			if (vertColCheck != null && vertColCheck.VertCheckIsColliding()) 
			{
				GD.Print("Play cannot stand animation");
			}
			else
            {
                player.Set("jumpQueued", true);
				msm.TransitionTo("jumpState");
            }
		}

		if (@event.IsActionPressed("Up") && pm.aimDirection.X == 0) //only stand up when only pressing up
		{
			if (vertColCheck != null && vertColCheck.VertCheckIsColliding()) GD.Print("Standing blocked");
			else
            {
                parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(false);
				msm.TransitionTo("groundedState");
            }
		}
	}
}
