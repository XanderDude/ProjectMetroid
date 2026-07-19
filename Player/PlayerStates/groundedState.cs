using Godot;
using System;

public partial class groundedState : State
{
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 15.0f;
	private float _WalkOffCooldown = .067f; //4 frames (60fps)
	private float walkOffCDTimer = 0;
	private float waterFootstepTimer = 0.0f;
	[Export] private float _turnAroundTime = 0.1f; //time it takes to turn around before standing up while crouched
	public float turnAroundTimer = 0.0f;

	public override void Ready()
	{
		turnAroundTimer = _turnAroundTime;
	}
	public override void Enter()
	{
		if (player.IsOnFloor()) player.ApplyFloorSnap();
		walkOffCDTimer = 0;
		pm.slideBoost = false;
		if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding() && pm.aimDirection.X != 0 && pm.IsOnFloor())
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true); //force crouch
			msm.TransitionTo("crouchState");
		}
		else if (msm._currentState.Name == "groundedState") parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
		else if (msm._currentState.Name == "crouchState") parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(false);
	}
	
	public override void Exit()
    {
        pm.vertColCheck.shape.ForceShapecastUpdate();
    }

	public override void PhysicsUpdate(float delta)
	{
		if (player.IsOnFloor()) player.ApplyFloorSnap();

		if (!player.IsOnFloor() && !pm.groundCheck.IsColliding())
		{
			walkOffCDTimer += delta;
			if (walkOffCDTimer >= _WalkOffCooldown)
			{
				msm.TransitionTo("jumpState");
				return;
			}
		}
		else walkOffCDTimer = 0;

		if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding())
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true);
			msm.TransitionTo("crouchState");
			return;
		}

		if (turnAroundTimer < _turnAroundTime)
		{
			turnAroundTimer += delta;
		}
		else if (!Input.IsActionPressed("Aim") && Mathf.Sign(pm.aimDirection.X) == pm.facingDirection && pm.vertColCheck != null && !pm.vertColCheck.VertCheckIsColliding())
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
			msm.TransitionTo("groundedState");
		}

		HandleGroundedMovement(delta);
		player.MoveAndSlide();

		if (msm._currentState.Name == "groundedState" && pm.aimDirection.X != 0 && player.Velocity.X == 0 && pm.forwardCheck.IsColliding())
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).WallCollided(true);
		}
		else if (msm._currentState.Name == "groundedState" && player.Velocity.X != 0)
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).WallCollided(false);
		}
	}

	private void HandleGroundedMovement(float delta)
	{
		//int input = Mathf.CeilToInt(Mathf.Abs(Input.GetAxis("Left", "Right"))) * Mathf.Sign(Input.GetAxis("Left", "Right")); //get absolute value of input (no negative), round up, multiply by sign to get direction
		Vector3 velocity = player.Velocity;
		velocity.Y = 0;
		//velocity.Y -= _gravity * delta;
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
		if (@event.IsActionPressed("Down") && !Input.IsActionPressed("Aim") && Mathf.Abs(pm.aimDirection.X) < 0.1f && msm._currentState.Name == "groundedState") //only crouch when previous frame had no aim direction
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(false);
			msm.TransitionTo("crouchState");
		}
		if (@event.IsActionPressed("Slide"))
		{
			//player.Set(PlayerManager.PropertyName.slideQueued, true);
			msm.TransitionTo("slideState");
		}
		if (@event.IsActionPressed("Jump")) 
		{
			if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding()) 
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
			if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding()) 
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
