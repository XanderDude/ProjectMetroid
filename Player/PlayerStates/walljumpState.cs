using Godot;

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
	private void CheckDirection() 
	{
		KinematicCollision3D collision = player.GetSlideCollision(0);
		Node3D collider = collision.GetCollider() as Node3D;
		airMaxSpeed *= Mathf.Sign(player.GlobalPosition.X - collider.GlobalPosition.X);
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).RotateMesh(Mathf.Sign(airMaxSpeed));
		pm.SpawnJumpCloud(0.8f, -45f * Mathf.Sign(airMaxSpeed));
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
		CheckDirection();
		SoundFriend.Play("player_jump_SFX");
		player.Set("jumpQueued", true);
		//GD.Print("Entered Wall Jump State. Jump queued: " + player.Get("jumpQueued"));
		jumpHeight = 0.0f;

		player.Set(PlayerManager.PropertyName.slideBoost, true); //player must be airborne, enable boost
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).BeginJump();
	}
	
	public override void Exit()
	{
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