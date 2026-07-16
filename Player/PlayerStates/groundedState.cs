using Godot;
using System;

public partial class groundedState : PlayerState
{
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 15.0f;
	
	private float coyote_timer = 0f;

	private float coyote_cd = 0.067f;
	private float waterFootstepTimer = 0.0f;

	public override void Enter()
	{
		pm.ApplyFloorSnap();
		coyote_timer = 0;
		pm.slideBoost = false;
		if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding() && pm.aimDirection.X != 0)
		{
			pm.playerMesh.GetNode<PlayerAnimationHandler>(pm.playerMesh.GetPath()).Crouch(true); //force crouch
			EmitSignal(SignalName.Transition, "crouchState");
		}
		else if (pm.psm.current_node_state_name == "groundedState") pm.playerMesh.GetNode<PlayerAnimationHandler>(pm.playerMesh.GetPath()).Grounded();
		else if (pm.psm.current_node_state_name == "crouchState") pm.playerMesh.GetNode<PlayerAnimationHandler>(pm.playerMesh.GetPath()).Crouch(false);
	}
	
	public override void Exit()
    {
        pm.vertColCheck.shape.ForceShapecastUpdate();
    }

	public override void PhysicsUpdate(float delta)
	{
		pm.ApplyFloorSnap();

		if (pm.JumpBuffered)
		{
			pm.jumpQueued = true;
			pm.jumpBufferTimer = 0f;
			EmitSignal(SignalName.Transition, "jumpState");
			return;
		}
		if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding())
		{
			pm.playerMesh.GetNode<PlayerAnimationHandler>(pm.playerMesh.GetPath()).Crouch(true); //force crouch
			EmitSignal(SignalName.Transition, "crouchState");
			return;
		}


		if (!pm.IsOnFloor() && !pm.groundCheck.IsColliding())
		{
			coyote_timer += delta;
			if (coyote_timer >= coyote_cd)
			{
				EmitSignal(SignalName.Transition, "jumpState");
				return;
			}
		}
		else coyote_timer = 0;



		HandleGroundedMovement(delta);
		pm.MoveAndSlide();
	}

	private void HandleGroundedMovement(float delta)
	{
		Vector3 velocity = pm.Velocity;
		var direction = Mathf.Sign(pm.aimDirection.X);
		velocity.Y = 0;
		float targetX = pm.noAimDirection || Input.IsActionPressed("Aim") ? 0 : direction * groundMaxSpeed;
		velocity.X = Mathf.MoveToward(velocity.X, targetX, delta + groundAcceleration);
		velocity.X = Mathf.Clamp(velocity.X, -groundMaxSpeed, groundMaxSpeed);
		pm.Velocity = velocity;
	}


	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("forgemode"))
		{
			EmitSignal(SignalName.Transition, "forgeState");
			return;
		}
		if (@event.IsActionPressed("Down") && !Input.IsActionPressed("Aim"))
		{
			pm.playerMesh.GetNode<PlayerAnimationHandler>(pm.playerMesh.GetPath()).Crouch(false);
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
				pm.playerMesh.GetNode<PlayerAnimationHandler>(pm.playerMesh.GetPath()).StandingBlocked();
			}
			else
			{
				pm.playerMesh.GetNode<PlayerAnimationHandler>(".").Airborne(pm.jumpQueued);
				EmitSignal(SignalName.Transition, "jumpState");
				return;
			}
		}

		if (@event.IsActionPressed("Up") && !Input.IsActionPressed("Aim") && pm.psm.current_node_state_name == "crouchState") 
		{
			if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding()) 
			{
				pm.playerMesh.GetNode<PlayerAnimationHandler>(pm.playerMesh.GetPath()).StandingBlocked();
			}
			else
            {
                pm.playerMesh.GetNode<PlayerAnimationHandler>(pm.playerMesh.GetPath()).Grounded();
				EmitSignal(SignalName.Transition, "groundedState");
			}
		}
	}
}
