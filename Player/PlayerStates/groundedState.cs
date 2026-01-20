using Godot;
using System;

public partial class groundedState : State
{
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 15.0f;
	public SlideVertColCheck vertColCheck;
	private float _WalkOffCooldown = .2f; //12 frames (60fps)
	private float walkOffCDTimer = 0;
	private float waterFootstepTimer = 0.0f;
	private float _turnAroundTime = 0.12f; //time it takes to turn around before standing up while crouched
	public float turnAroundTimer = 0.0f;

	public override void Ready()
	{
		turnAroundTimer = _turnAroundTime;
	}
	public override void Enter()
	{
		walkOffCDTimer = 0;
		pm.slideBoost = false;
		if (msm._currentState.Name == "groundedState") parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
		else if (msm._currentState.Name == "crouchState") parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch();
	}
	
	public override void Exit() {
	}

	public override void PhysicsUpdate(float delta)
	{
		if (!player.IsOnFloor())
		{
			walkOffCDTimer += delta;
			if (walkOffCDTimer >= _WalkOffCooldown)
			{
				msm.TransitionTo("jumpState");
				return;
			}
		}
		else walkOffCDTimer = _WalkOffCooldown;

		waterFootstepTimer -= delta;
		if (waterFootstepTimer < 0)
		{
			waterFootstepTimer = 0.0f;
		}

		if (pm.inwater && waterFootstepTimer == 0.0f && Mathf.Abs(player.Velocity.X) > 0.4f)
		{
			SoundFriend.Play("player_treading_water_SFX");
			GD.Print("PLAY");
			waterFootstepTimer = 0.3f;
		}
		
		if (turnAroundTimer < _turnAroundTime) //player is turning around
        {
			turnAroundTimer += delta;
		}
		else if (!Input.IsActionPressed("Aim") && Mathf.Sign(pm.aimDirection.X) == pm.facingDirection && vertColCheck != null && !vertColCheck.VertCheckIsColliding()) //turn finished and player is holding direction
		{
			msm.TransitionTo("groundedState");
		} 

		HandleGroundedMovement(delta);
		player.MoveAndSlide();
		/*
		if (msm._currentState.Name == "groundedState" && pm.aimDirection.X != 0 && player.Velocity.X == 0) //check if player is trying to move
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).WallCollided(true);
		}
		else if (msm._currentState.Name == "groundedState" && player.Velocity.X != 0) parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).WallCollided(false);
		*/
	}

	private void HandleGroundedMovement(float delta)
	{
		//int input = Mathf.CeilToInt(Mathf.Abs(Input.GetAxis("Left", "Right"))) * Mathf.Sign(Input.GetAxis("Left", "Right")); //get absolute value of input (no negative), round up, multiply by sign to get direction
		Vector3 velocity = player.Velocity;
		velocity.Y -= _gravity * delta;
		if (Input.IsActionPressed("Aim"))
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, delta + groundAcceleration);
        }
		else velocity.X = Mathf.MoveToward(velocity.X, Mathf.Sign(pm.aimDirection.X) * groundMaxSpeed, delta + groundAcceleration);
		velocity.X = Mathf.Clamp(velocity.X, -groundMaxSpeed, groundMaxSpeed);
		player.Velocity = velocity;
	}


	public override void HandleInput(InputEvent @event) //Called whenever an input is detected
	{
		if (@event.IsActionPressed("forgemode"))
		{
			msm.TransitionTo("forgeState");
		}
		if (@event.IsActionPressed("Down") && !Input.IsActionPressed("Aim") && msm._currentState.Name == "groundedState") //only crouch when previous frame had no aim direction
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch();
			msm.TransitionTo("crouchState");
		}
		if (@event.IsActionPressed("Slide"))
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

		if (@event.IsActionPressed("Up") && !Input.IsActionPressed("Aim") && msm._currentState.Name == "crouchState") //only stand up when only pressing up
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
