using Godot;
using System;

public partial class crouchState : State
{
	[Export] public float crouchDelay; //delay before player can move

	public override void Enter()
	{
		GD.Print("Entered Crouch State");
		if (Input.IsActionPressed("Slide") && Input.GetAxis("Left", "Right") != 0) //player wants to slide so let them
		{
			msm.TransitionTo("slideState");
		}
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true);
	}

	public override void Exit()
	{
		GD.Print("Exited Crouch State.");
	}

	public override void PhysicsUpdate(float delta)
	{
		if (!player.IsOnFloor()) //immediately switch to jump state
		{
			msm.TransitionTo("jumpState");
		}
		else if (Input.IsActionPressed("Slide") && Input.GetAxis("Left", "Right") != 0) //slide out of crouch
		{
			player.Set(PlayerManager.PropertyName.slideQueued, true);
			msm.TransitionTo("slideState");
		}
		//HandleGroundedMovement(delta);
		player.MoveAndSlide();
	}

	private void HandleCrouchMovement(float delta)
	{
		//
	}

	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Left") || @event.IsActionPressed("Right") || @event.IsActionPressed("Up"))
		{
			parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(false); //return to standing
			msm.TransitionTo("groundedState");
		}
		if (@event.IsActionPressed("Jump"))
		{
			player.Set("jumpQueued", true);
			msm.TransitionTo("jumpState");
		}
		else player.Set("jumpQueued", false);

	}

}
