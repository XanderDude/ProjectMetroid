using Godot;

public partial class walljumpState : State
{
	[ExportSubgroup("Wall Jump State")]
	[Export] public float airMaxSpeed = 5.0f;
	[Export] public float jumpAcceleration = 10.0f;
	[Export] public float jumpDeceleration = 15f;
 	[Export] public float jumpVelocity = 10.0f;
	[Export] public float jumpMaxHeight = 1f, jumpMinHeight = 0.2f; //meters
	public float jumpHeight = 0.0f; //player's current jump height position
	private float startPosition = 0.0f;
	[Export] public float playerTop = 1.5f;

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
			jumpHeight = player.GlobalPosition.Y - startPosition;
			GD.Print("Jump Height: " + jumpHeight);
			return true;
		}
		else
		{
			velocity.Y = player.Velocity.Y/2;
			pm.jumpQueued = false;
			jumpHeight = jumpMaxHeight; //clamp
			return false;
		}
	}
	private void CancelUpwardVelocity()
    {
		pm.jumpQueued = false;
        player.Velocity = new Vector3(player.Velocity.X, player.Velocity.Y/2, player.Velocity.Z);
		GD.Print("Jump velocity Cancelled");
    }

	public override void Enter()
	{
		airMaxSpeed = Mathf.Abs(airMaxSpeed);
		CheckDirection();
		SoundFriend.Play("player_jump_SFX");
		pm.jumpQueued = true;
		jumpHeight = 0.0f;
		startPosition = player.GlobalPosition.Y;
		player.Set(PlayerManager.PropertyName.slideBoost, true); //player must be airborne, enable boost
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).BeginJump();
	}
	
	public override void Exit()
	{
		pm.jumpQueued = false;
	}

	public override void PhysicsUpdate(float delta)
	{
		if (pm.jumpQueued && !Input.IsActionPressed("Jump") && jumpHeight > jumpMinHeight) //check when jump is released
		{
			CancelUpwardVelocity();
			pm.jumpQueued = false;
		}
		if (pm.jumpQueued && jumpHeight != 0 && Mathf.Floor(jumpHeight * 1000) == Mathf.Floor((player.GlobalPosition.Y - startPosition)* 1000)) CancelUpwardVelocity();
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
		if (@event.IsActionPressed("Jump")) //check when jump is released
		{
			//walljump check
		}
	}	
}