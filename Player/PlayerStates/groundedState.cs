using Godot;
using System;

public partial class groundedState : State
{
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 15.0f;
	public SlideVertColCheck vertColCheck;
	private float _landingCooldown = .1f; //6 frames (60fps)
	private float landingCDTimer = 0;
	private float _WalkOffCooldown = .2f; //12 frames (60fps)
	private float walkOffCDTimer = 0;
	private bool isPlaying = false;
	public override void Enter()
	{
		pm.slideBoost = false;
		if (msm._currentState.Name == "groundedState") parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
		else if (msm._currentState.Name == "crouchState") parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch();

		if (msm._previousState == null || msm._previousState.Name == "jumpState") {
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
			walkOffCDTimer += delta;
			if (walkOffCDTimer >= _WalkOffCooldown)
			{
				msm.TransitionTo("jumpState");
			}
		}
		else walkOffCDTimer = _WalkOffCooldown;

		if (pm.inwater && timer == 0.0f && pm.Velocity.X != 0)
		{
			SoundFriend.Play("player_treading_water_SFX");
			GD.Print("PLAY");
			timer = 0.3f;
			isPlaying = true;
		}
		else if (!pm.inwater && isPlaying || pm.Velocity.X == 0)
		{
			SoundFriend.Stop("player_treading_water_SFX");
			isPlaying = false;
		}

		timer -= delta;
		if (timer < 0)
		{
			timer = 0.0f;
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
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch();
			msm.TransitionTo("crouchState");
		}
		if (@event.IsActionPressed("Slide") && pm.aimDirection.X != 0)
		{
			//player.Set(PlayerManager.PropertyName.slideQueued, true);
			msm.TransitionTo("slideState");
		}
		if (@event.IsActionPressed("Jump")) 
		{
			if (vertColCheck != null && vertColCheck.VertCheckIsColliding()) 
			{
				parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).StandingBlocked();
			}
			else
			{
				player.Set("jumpQueued", true);
				msm.TransitionTo("jumpState");
			}
		}

		if (@event.IsActionPressed("Up") && pm.aimDirection.X == 0 && msm._currentState.Name == "crouchState") //only stand up when only pressing up
		{
			if (vertColCheck != null && vertColCheck.VertCheckIsColliding()) 
			{
				parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).StandingBlocked();
			}
			else
            {
                parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
				msm.TransitionTo("groundedState");
			}
		}
	}
}
