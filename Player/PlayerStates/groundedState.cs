using Godot;
using System;

public partial class groundedState : State
{
	[Export] public float groundMaxSpeed = 6.0f;
	[Export] public float groundAcceleration = 15.0f;
	public SlideVertColCheck vertColCheck;
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
		
		if (msm._currentState.Name == "groundedState" && Mathf.Abs(player.Velocity.X) >= .1f) 
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
		}
		if (!player.IsOnFloor()) //immediately switch to jump state
		{
			msm.TransitionTo("jumpState");
			return;
		}
		
		HandleGroundedMovement(delta);
		player.MoveAndSlide();
		if (msm._currentState.Name == "groundedState" && Input.GetAxis("Left", "Right") != 0 && player.Velocity.X == 0) //check if player is trying to move
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).WallCollided();
		}
	}

	private void HandleGroundedMovement(float delta)
	{
		//int input = Mathf.CeilToInt(Mathf.Abs(Input.GetAxis("Left", "Right"))) * Mathf.Sign(Input.GetAxis("Left", "Right")); //get absolute value of input (no negative), round up, multiply by sign to get direction
		Vector3 velocity = player.Velocity;
		velocity.X = Mathf.MoveToward(velocity.X, Mathf.Sign(pm.aimDirection.X) * groundMaxSpeed, delta + groundAcceleration);
		velocity.X = Mathf.Clamp(velocity.X, -groundMaxSpeed, groundMaxSpeed);
		player.Velocity = velocity;
	}


	public override void HandleInput(InputEvent @event) //Called whenever an input is detected
	{
		if (@event.IsActionPressed("forgemode") && GameFriend.gameinstance.initroomfriend == true)
		{
			msm.TransitionTo("forgeState");
		}
		if (@event.IsActionPressed("Down") && pm.noAimDirection && msm._currentState.Name == "groundedState") //only crouch when previous frame had no aim direction
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true);
			msm.TransitionTo("crouchState");
		}
		
		if (Input.IsActionPressed("Slide") && Input.GetAxis("Left", "Right") != 0)
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

		if (@event.IsActionPressed("Up") && Input.GetAxis("Left", "Right") == 0) //only stand up when only pressing up
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
