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
	[Export] public float playerTop = 1.5f;

	public float jumpHeight = 0.0f;
	[Export] Godot.AudioStreamPlayer jumpySound;
	private void checkDirection() {
		KinematicCollision3D collision = player.GetSlideCollision(0);
		Node3D collider = collision.GetCollider() as Node3D;
		if (collider.GlobalPosition.X >= player.GlobalPosition.X) 
		{ 
			airMaxSpeed = -airMaxSpeed;
			parentMesh.RotationDegrees = new(0, Mathf.Abs(parentMesh.RotationDegrees.Y) * -1, 0);
		}
		if (collider.GlobalPosition.X <= player.GlobalPosition.X)
		{ 
			airMaxSpeed = Mathf.Abs(airMaxSpeed); 
			parentMesh.RotationDegrees = new(0, Mathf.Abs(parentMesh.RotationDegrees.Y), 0);	
		}
	}

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
			//GD.Print("jump max height reached");
			jumpHeight = jumpMaxHeight; 
			return false;
		}
	}

	public override void Enter()
	{
		airMaxSpeed = Mathf.Abs(airMaxSpeed);
		checkDirection();
		SoundFriend.Play("player_jump_SFX");
		player.Set("jumpQueued", true);
		//GD.Print("Entered Wall Jump State. Jump queued: " + player.Get("jumpQueued"));
		jumpHeight = 0.0f;

		player.Set(PlayerManager.PropertyName.slideBoost, true); //player must be airborne, enable boost
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).BeginJump();
		
	}
	public override void Exit()
	{
		//GD.Print("Exited Wall Jump State");
		player.Set("jumpQueued", false); //don't jump on exit if holding jump
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
	}

	
	public override void PhysicsUpdate(float delta)
	{
		HandleAirMovement(delta);
		player.MoveAndSlide();
		
	}

	private void HandleAirMovement(float delta)
	{
		Vector3 velocity = player.Velocity;
		

		if ((bool)player.Get("jumpQueued") && IsAscending(delta, ref velocity))
		{ //jump queued set true outside this state. if the player releases jump, the bool is set false 
			//GD.Print("Jumping");
			velocity.X = airMaxSpeed;

			velocity.Y -= _gravity * delta;
		}
		
		else
		{	
			velocity.X = airMaxSpeed;
			velocity.Y -= _gravity * 2.5f * delta;
			
			
			
			
			
		}
		
		if (velocity.Y < 0) msm.TransitionTo("jumpState");
		
		player.Velocity = velocity;
	}

	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionReleased("Jump")) //check when jump is released
		{
			player.Set("jumpQueued", false);
		}
	}
	
	
	
	
}
