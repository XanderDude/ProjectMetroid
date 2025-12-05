using Godot;
using System;

public partial class slideState : State
{
	//[Export] public float slideTime = 1.0f; //max time the player can stay in a slide before standing up/crouching
	[Export] public float slideMinTime = .2f; //min time the player has to stay in the slide state before standing up (can still jump)
	[Export] public float slideMaxTime = 1;
	[Export] public float slideMaxSpeed = 12.0f;
	[Export] public float boostMaxSpeed = 6.0f;
	[Export] public float slideAcceleration = 15.0f;
	[Export] public float slideDeceleration = 15.0f;
	private float currentSlideSpeed;
	private float input;
	private bool crouchQueued = false;
	private float slideTimer; //time in current slide
	public SlideVertColCheck vertColCheck;
	public override void Enter()
	{
		input = Mathf.Sign(Input.GetAxis("Left", "Right"));
		crouchQueued = false;
		slideTimer = 0;
		if ((bool)player.Get(PlayerManager.PropertyName.slideBoost) && GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("slideBoost"))
		{
			currentSlideSpeed = boostMaxSpeed * input;
		}
		else
		{
			currentSlideSpeed = slideMaxSpeed * input;
		}
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Sliding(true);
	}

	public override void Exit()
	{
		player.Set(PlayerManager.PropertyName.slideBoost, false);
		if (crouchQueued) parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true);
		else parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Sliding(false); //return to grounded state
	}
	public override void PhysicsUpdate(float delta)
	{
		slideTimer += delta;

		if (!player.IsOnFloor()) //immediately switch to jump state
		{
			msm.TransitionTo("jumpState");
			return;
		}

		if ((Mathf.Sign(Input.GetAxis("Left", "Right")) == input * -1 || !Input.IsActionPressed("Slide")) && slideTimer >= slideMinTime)
		{ //if player is holding opposite direction of slide or is not holding slide button
			if (vertColCheck != null && vertColCheck.RayIsColliding())
			{
				crouchQueued = true; //for animation purposes
				msm.TransitionTo("crouchState");
				return;
			}
			else msm.TransitionTo("groundedState"); //switch to grounded state
		}
		else if (slideTimer >= slideMinTime && Mathf.Abs(currentSlideSpeed) < .2f)
		{
			crouchQueued = true; //for animation purposes
			msm.TransitionTo("crouchState");
			return;
		}
		HandleSlidingMovement(delta);
		player.MoveAndSlide();
	}

	private void HandleSlidingMovement(float delta)
	{
		Vector3 velocity = player.Velocity;

		if ((bool)player.Get(PlayerManager.PropertyName.slideBoost))
		{
			currentSlideSpeed = Mathf.MoveToward(currentSlideSpeed, 0, delta * slideDeceleration);
		}
		else //normal slide
		{
			currentSlideSpeed = Mathf.MoveToward(currentSlideSpeed, 0, delta * slideDeceleration);
		}

		//velocity.X = Mathf.MoveToward(slideMaxSpeed, 0, delta + slideDeceleration);
		velocity.X = currentSlideSpeed;
		player.Velocity = velocity; 
	}

	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Jump")) //allow jumping out of slide
		{
            if (vertColCheck != null && vertColCheck.RayIsColliding())
            {
				GD.Print("Jump Blocked");
        	}
			else
            {
				player.Set(PlayerManager.PropertyName.jumpQueued, true);
				msm.TransitionTo("jumpState");
			}
		}

		if (@event.IsActionPressed("Down") && Input.GetAxis("Left", "Right") == 0 && slideTimer >= slideMinTime)
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true);
			msm.TransitionTo("crouchState");
		}

		if (@event.IsActionPressed("Shoot") || @event.IsActionPressed("SpecialShoot"))
		{
			asm.TransitionTo("attackState");
		}
	}
}
