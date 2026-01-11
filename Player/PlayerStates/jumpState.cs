using Godot;
using System;

public partial class jumpState : State
{
	[ExportSubgroup("Jump State")]
	[Export] public float airMaxSpeed = 5.0f;
	[Export] public float jumpAcceleration = 10.0f;
	[Export] public float jumpDeceleration = 15f;
	[Export] public float jumpVelocity = 10.0f;
	[Export] public float jumpMaxHeight = 0.17f;
	[Export] public float playerTopWhileInJump = 1.322f;
	private float _mantleCooldown = .2f;
	private float mantleTimer = 1;
	public float jumpHeight = 0.0f; //player's current jump height position
	private bool neutralJump = false;
	private bool cancelVelocity = true;

	public float meshTop = 0.0f;

	private const float PLAYER_AND_MESH_OFFSET = 0.2f; //to account for difference in player origin and mesh origin

	private bool isTouching() {
		if (player.GetSlideCollisionCount() != 0) {
			return true;
		}
		return false;
	}
	private bool isSameHeight()
	{
		if (!isTouching() && !isGreaterHeight()) { 
			return false;
		}


		

		for (int i = 0; i < player.GetSlideCollisionCount(); i++) //must be at least 1 collision
		{
			KinematicCollision3D collision = player.GetSlideCollision(i);
			BoxShape3D boxShape;
			StaticBody3D collider;
			try
			{
				collider = collision.GetCollider() as StaticBody3D;
				if (!collider.GetCollisionLayerValue(1)) continue;
				boxShape = (BoxShape3D)collider.GetChild<CollisionShape3D>(0).Shape;
			}
			catch { continue; }

			var scaleY = new Vector3(collider.Transform.Basis.X.Y * boxShape.Size.Y,
			collider.Transform.Basis.Y.Y * boxShape.Size.Y,
			collider.Transform.Basis.Z.Y * boxShape.Size.Y).Length(); 
			float playerHeight = player.GlobalPosition.Y + playerTopWhileInJump + 0.5f;
			float meshTopTemp = collider.GlobalPosition.Y + scaleY / 2;
			meshTop = meshTopTemp;
			if (Mathf.Abs(playerHeight - meshTop) < PLAYER_AND_MESH_OFFSET && playerHeight >= meshTop) {
				return true;
			}
		}
		return false;

	}

	private bool isGreaterHeight()
	{
		if (player.GetSlideCollisionCount() == 0) {
			return false;
		}
		KinematicCollision3D collision = player.GetSlideCollision(0);
		Node3D collider = collision.GetCollider() as Node3D;
		float playerHeight = player.GlobalPosition.Y + playerTopWhileInJump;
		float meshTop = collider.GlobalPosition.Y;


		if (Mathf.Abs(playerHeight - meshTop) > 0.1f)
			return true;
		
		return false;
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
			pm.jumpQueued = false;
			jumpHeight = jumpMaxHeight; //clamp
			return false;
		}
	}

	public override void Enter()
	{
		cancelVelocity = true;
		if (pm.jumpQueued)//jump state entered due to player jumping
		{
			if (pm.jumpVFX != null && msm._previousState.Name != "mantleState") pm.SpawnJumpCloud(0);
			jumpHeight = 0.0f;
			SoundFriend.Play("player_jump_SFX");
			if (pm.aimDirection.X == 0) cancelVelocity = true; //freeze horizontal velocity for neutral jump
		}
		player.Set(PlayerManager.PropertyName.slideBoost, true); //player must be airborne, enable boost
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).BeginJump();
	}

	public override void Exit()
	{
		//GD.Print("Exited Jump State");
		pm.jumpQueued = false; //don't jump on exit if holding jump
		//parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
	}


	public override void PhysicsUpdate(float delta)
	{
		mantleTimer += delta;

		if (!pm.jumpQueued && (pm.groundCheck.IsColliding() || player.IsOnFloor()))
        {
			cancelVelocity = true;
			//if (player.IsOnFloor()) player.Velocity = new Vector3(-Mathf.Sign(pm.aimDirection.X), 0, 0) * airMaxSpeed;
            if (Input.IsActionPressed("Slide") && pm.aimDirection.X != 0) {
				GD.Print("Slide boosting");
				msm.TransitionTo("slideState");
			}
			else msm.TransitionTo("groundedState");
        }
		else if (GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("mantling") && mantleTimer >= _mantleCooldown && pm.aimDirection.X != 0 && isSameHeight()) //player must be pressing towards ledge, player top reset, and in range of ledge height
		{
			mantleTimer = 0;
			player.Velocity = Vector3.Zero;
			
			msm.TransitionTo("mantleState");
		}
		HandleAirMovement(delta);
		if (cancelVelocity)
		{
			player.Velocity = Vector3.Zero;
			cancelVelocity = false;
		}
		player.MoveAndSlide();
	}

	private void HandleAirMovement(float delta)
	{
		Vector3 velocity = player.Velocity;
		float input = Mathf.Sign(pm.aimDirection.X);

		if (pm.jumpQueued && IsAscending(delta, ref velocity))
		{ //jump queued set true outside this state. if the player releases jump, the bool is set false 
			velocity.Y -= _gravity * delta;
			if (input == 0)
			{
				velocity.X = Mathf.MoveToward(velocity.X, 0, jumpDeceleration);
			}
			else velocity.X = input * airMaxSpeed;
		}
		else //must be falling
		{

			velocity.Y -= _gravity * 2.5f * delta; //faster falling speed
			if (input == 0)
			{
				velocity.X = Mathf.MoveToward(velocity.X, 0, jumpDeceleration);
			}
			else velocity.X = input * airMaxSpeed;
		}

		velocity.X = Mathf.Clamp(velocity.X, -airMaxSpeed, airMaxSpeed);//clamp horizontal speed
		player.Velocity = velocity;
	}

	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("forgemode") && GameFriend.gameinstance.initroomfriend == true)
		{
			msm.TransitionTo("forgeState");
		}
		if (@event.IsActionReleased("Jump")) //check when jump is released
		{
			pm.jumpQueued = false;
		}
		if (@event.IsActionPressed("Jump") && GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("wallJump") && !player.IsOnFloor() && isTouching() && !isSameHeight()) {
			msm.TransitionTo("walljumpState");
		}

	}	
	
}
