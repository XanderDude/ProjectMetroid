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
	//private bool crouchQueued = false;
	private float slideTimer; //time in current slide
	public override void Enter()
	{
		input = Mathf.Sign(Input.GetAxis("Left", "Right"));
		GD.Print("Entered Slide State. Facing " + input);
		slideTimer = 0;
		if ((bool)player.Get(PlayerManager.PropertyName.slideBoost))
		{
			GD.Print("Boosting");
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
		//player.Set(PlayerManager.PropertyName.slideQueued, false);
		player.Set(PlayerManager.PropertyName.slideBoost, false);
		//if (Input.IsActionPressed("Slide") && crouchQueued) parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true);
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Sliding(false); //return to grounded state
		GD.Print("Exited Slide State");
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
			msm.TransitionTo("groundedState"); //switch to grounded and reverse direction
		}

		/*else if (slideTimer >= slideMaxTime) //end slide after allowed time
		{
			crouchQueued = true; //crouch is not techincally queued yet but it's possible
			msm.TransitionTo("groundedState");
		}*/
		HandleSlidingMovement(delta);
		player.MoveAndSlide();
		if (player.GetSlideCollisionCount() > 1 && currentSlideSpeed < slideMaxSpeed)
		{
			GD.Print("Can't Stand up");
			currentSlideSpeed = slideMaxSpeed * input;
		}
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
			player.Set(PlayerManager.PropertyName.jumpQueued, true);
			msm.TransitionTo("jumpState");
		}
		/*
		if (@event.IsActionPressed("Slide")) //is slide being pressed
		{
			msm.slideQueued = true;
			//slide out of slide?
		}
		else
		{
			msm.slideQueued = false;
		}*/
	}

}
