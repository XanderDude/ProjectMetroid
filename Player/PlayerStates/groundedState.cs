using Godot;
using System;

public partial class groundedState : State
{
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 15.0f;

	public override void Enter()
	{
		player.Set(PlayerManager.PropertyName.slideBoost, false);
		if (msm._currentState.Name == "groundedState") parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded(); //could be crouching
	}
	
	public override void Exit() {
		//GD.Print("Exited Grounded State.");
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
		else if (player.Velocity.X == 0 && Input.IsActionPressed("Crouch"))
		{
			//parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true);
		}
		
		//else parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
		HandleGroundedMovement(delta);
		player.MoveAndSlide();
		if (msm._currentState.Name == "groundedState" && Input.GetAxis("Left", "Right") != 0 && player.Velocity.X == 0) //check if player is trying to move
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
		if (@event.IsActionPressed("forgemode"))
		{
			msm.TransitionTo("forgeState");
		}
		if (@event.IsActionPressed("Down") && Input.GetAxis("Left", "Right") == 0) //only crouch when not moving
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true);
			msm.TransitionTo("crouchState");
		}
		if (@event.IsActionPressed("Up") && Input.GetAxis("Left", "Right") == 0) //only stand up when only pressing up
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(false);
			msm.TransitionTo("groundedState");
		}
		if (Input.IsActionPressed("Slide") && Input.GetAxis("Left", "Right") != 0)
		{
			//player.Set(PlayerManager.PropertyName.slideQueued, true);
			msm.TransitionTo("slideState");
		}
		if (@event.IsActionPressed("Jump"))
		{
			player.Set("jumpQueued", true);
			msm.TransitionTo("jumpState");
		}
		else player.Set("jumpQueued", false);

		if (@event.IsActionPressed("Shoot"))
		{
			asm.TransitionTo("attackState");
		}
		else if (@event.IsActionPressed("SpecialShoot"))
		{
			asm.TransitionTo("attackState");
		}
	}
}
