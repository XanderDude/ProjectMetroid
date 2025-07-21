using Godot;
using System;

public partial class walljumpState : State
{
	[ExportSubgroup("Wall Jump State")]
	[Export] public float airMaxSpeed = 5.0f;
	[Export] public float jumpAcceleration = 10.0f;
	[Export] public float jumpDeceleration = 15f;
 	[Export] public float jumpVelocity = 10.0f;
	[Export] public float jumpMaxHeight = 0.17f;
	

	public float jumpHeight = 0.0f;
	
	private bool IsAscending(float delta, ref Vector3 velocity) //check if player should be ascending
	{
		if (jumpHeight < jumpMaxHeight)
		{
			velocity.Y = jumpVelocity; //continously set upward velocity
			jumpHeight += delta;
			//GD.Print("Jump Height: " + jumpHeight);
			return true;
		}
		else
		{
			GD.Print("jump max height reached");
			jumpHeight = jumpMaxHeight; //clamp
			return false;
		}
	}

	public override void Enter()
	{
		
		GD.Print("Entered Wall Jump State. Jump queued: " + player.Get("jumpQueued"));
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).BeginJump();
	}
	public override void Exit()
	{
		GD.Print("Exited Wall Jump State");
	}

	
	public override void PhysicsUpdate(float delta)
	{
		
		player.MoveAndSlide();
		HandleAirMovement(delta);
		
	}

	private void HandleAirMovement(float delta)
	{
		Vector3 velocity = player.Velocity;
		if (IsAscending(delta, ref velocity)) {
		velocity.X = airMaxSpeed;
		}
		else {
			msm.TransitionTo("jumpState");
		}
		
	}

	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("Jump")) //check when jump is released
		{
			player.Set("jumpQueued",true);
		}
		
		if (@event.IsActionReleased("Jump")) //check when jump is released
		{
			player.Set(PlayerManager.PropertyName.jumpQueued, false);
		}
	}
	
	
	
	
}
