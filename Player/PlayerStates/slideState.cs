using Godot;
using System;

public partial class slideState : PlayerState
{
	//[Export] public float slideTime = 1.0f; //max time the player can stay in a slide before standing up/crouching
	[Export] public float slideMinTime = .2f; //min time the player has to stay in the slide state before standing up (can still jump)
	[Export] public float slideMaxTime = 1;
	[Export] public float slideMaxSpeed = 12.0f;
	[Export] public float boostMaxSpeed = 6.0f;
	[Export] public float slideAcceleration = 15.0f;
	[Export] public float slideDeceleration = 15.0f;
	private float currentSlideSpeed;
	private int slideDirection;
	private bool crouchQueued = false;
	private float slideTimer; //time in current slide
	public override void Enter()
	{
		slideDirection = pm.facingDirection;
		crouchQueued = false;
		slideTimer = 0;

		if (pm.slideBoost && GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("slideBoost"))
		{
			if (pm.slideBoostVFX != null)
			{
				var slideBoostEffect = pm.slideBoostVFX.Instantiate() as Node3D;
				this.AddChild(slideBoostEffect, true);
				slideBoostEffect.GlobalPosition = pm.playerMesh.GlobalPosition;
				slideBoostEffect.GlobalRotation = pm.playerMesh.GlobalRotation;

			}
			currentSlideSpeed = boostMaxSpeed * slideDirection;
		}
		else
		{
			currentSlideSpeed = slideMaxSpeed * slideDirection;
		}
		//parentMesh.RotationDegrees = new Vector3(0, Mathf.Abs(parentMesh.RotationDegrees.Y) * slideDirection, 0); //rotate mesh
		pm.pah.Sliding(true);
		pm.ApplyFloorSnap();
	}

	public override void Exit()
	{
		pm.Set(PlayerManager.PropertyName.slideBoost, false);
		if (crouchQueued) pm.pah.Crouch(false);
		else pm.pah.Sliding(false); //return to grounded state
	}
	public override void PhysicsUpdate(float delta)
	{
		slideTimer += delta;
		pm.ApplyFloorSnap();
		if (!pm.groundCheck.IsColliding() && !pm.IsOnFloor() && slideTimer > 0.05f) //immediately switch to jump state
		{
			EmitSignal(SignalName.Transition, "jumpState");
			return;
		}

		if ((Mathf.Sign(pm.aimDirection.X) == slideDirection * -1 || Mathf.Abs(pm.Velocity.X) < .01f) && !Input.IsActionPressed("Aim") && slideTimer >= slideMinTime)
		{ //if player is holding opposite direction of slide or there's no movement, stop sliding
			if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding())// || Mathf.Sign(pm.aimDirection.X) == slideDirection * -1)
			{
				crouchQueued = true; //for animation purposes
				EmitSignal(SignalName.Transition, "crouchState");
				return;
			}
			else EmitSignal(SignalName.Transition, "groundedState"); //switch to grounded state
		}
		else if (slideTimer >= slideMaxTime || Mathf.Abs(currentSlideSpeed) < .2f) //slide is at its end
		{
			if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding())
			{
				crouchQueued = true; //for animation purposes
				EmitSignal(SignalName.Transition, "crouchState");
				return;
			}
			else EmitSignal(SignalName.Transition, "groundedState"); //switch to grounded state
		}
		HandleSlidingMovement(delta);
		pm.MoveAndSlide();
	}

	private void HandleSlidingMovement(float delta)
	{
		Vector3 velocity = pm.Velocity;

		if (pm.slideBoost)
		{
			currentSlideSpeed = Mathf.MoveToward(currentSlideSpeed, 0, delta * slideDeceleration);
		}
		else //normal slide
		{
			currentSlideSpeed = Mathf.MoveToward(currentSlideSpeed, 0, delta * slideDeceleration);
		}

		//velocity.X = Mathf.MoveToward(slideMaxSpeed, 0, delta + slideDeceleration);
		velocity.X = currentSlideSpeed;
		pm.Velocity = velocity; 
	}

	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Jump")) //allow jumping out of slide
		{
            if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding())
            {
				pm.pah.StandingBlocked();
        	}
			else
            {
				pm.Set(PlayerManager.PropertyName.jumpQueued, true);
				EmitSignal(SignalName.Transition, "jumpState");
			}
		}

		if (@event.IsActionPressed("Down") && pm.aimDirection.X == 0 && slideTimer >= slideMinTime)
		{
			pm.pah.Crouch(false);
			EmitSignal(SignalName.Transition, "crouchState");
		}
	}
}
