using Godot;
using System;

public partial class groundedState : PlayerState
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
		player.ApplyFloorSnap();
		walkOffCDTimer = 0;
		pm.slideBoost = false;
		if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding() && pm.aimDirection.X != 0)
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true); //force crouch
			EmitSignal(SignalName.Transition, "crouchState");
		}
		else if (psm.current_node_state_name == "groundedState") parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
		else if (psm.current_node_state_name == "crouchState") parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(false);
	}
	
	public override void Exit()
    {
        pm.vertColCheck.shape.ForceShapecastUpdate();
    }

	public override void PhysicsUpdate(float delta)
	{
		player.ApplyFloorSnap();

		if (pm.JumpBuffered)
		{
			pm.jumpQueued = true;
			pm.jumpBufferTimer = 0f;
			EmitSignal(SignalName.Transition, "jumpState");
			return;
		}
		if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding())
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true); //force crouch
			EmitSignal(SignalName.Transition, "crouchState");
			return;
		}

		if (!player.IsOnFloor() && !pm.groundCheck.IsColliding())
		{
			walkOffCDTimer += delta;
			if (walkOffCDTimer >= _WalkOffCooldown)
			{
				EmitSignal(SignalName.Transition, "jumpState");
				return;
			}
		}
		else walkOffCDTimer = 0;

		waterFootstepTimer -= delta;
		if (waterFootstepTimer < 0)
		{
			waterFootstepTimer = 0.0f;
		}

		if (pm.inwater && waterFootstepTimer == 0.0f && Mathf.Abs(player.Velocity.X) > 0.4f)
		{
			SoundFriend.Play("player_treading_water_SFX");
			waterFootstepTimer = 0.3f;
		}
		
		if (turnAroundTimer < _turnAroundTime) //player is turning around
        {
			turnAroundTimer += delta;
		}
		else if (!Input.IsActionPressed("Aim") && Mathf.Sign(pm.aimDirection.X) == pm.facingDirection && pm.vertColCheck != null && !pm.vertColCheck.VertCheckIsColliding()) //turn finished and player is holding direction
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
			EmitSignal(SignalName.Transition, "groundedState");
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
		Vector3 velocity = player.Velocity;
		velocity.Y = 0;
		float targetX = pm.noAimDirection || Input.IsActionPressed("Aim") ? 0 : Mathf.Sign(pm.aimDirection.X) * groundMaxSpeed;
		velocity.X = Mathf.MoveToward(velocity.X, targetX, delta + groundAcceleration);
		velocity.X = Mathf.Clamp(velocity.X, -groundMaxSpeed, groundMaxSpeed);
		player.Velocity = velocity;
	}


	public override void HandleInput(InputEvent @event) //Called whenever an input is detected
	{
		if (@event.IsActionPressed("forgemode"))
		{
			EmitSignal(SignalName.Transition, "forgeState");
			return;
		}
		if (@event.IsActionPressed("Down") && !Input.IsActionPressed("Aim") && Mathf.Abs(pm.aimDirection.X) < 0.1f && psm.current_node_state_name == "groundedState") //only crouch when previous frame had no aim direction
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(false);
			EmitSignal(SignalName.Transition, "crouchState");
			return;
		}
		if (@event.IsActionPressed("Slide"))
		{
			//player.Set(PlayerManager.PropertyName.slideQueued, true);
			EmitSignal(SignalName.Transition, "slideState");
			return;
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
				EmitSignal(SignalName.Transition, "jumpState");
				return;
			}
		}

		if (@event.IsActionPressed("Up") && !Input.IsActionPressed("Aim") && psm.current_node_state_name == "crouchState") //only stand up when only pressing up
		{
			if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding()) 
			{
				parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).StandingBlocked();
			}
			else
            {
                parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
				EmitSignal(SignalName.Transition, "groundedState");
			}
		}
	}
}
