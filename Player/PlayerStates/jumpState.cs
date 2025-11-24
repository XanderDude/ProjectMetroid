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
	[Export] public float playerTop = 1.5f;
	private float mantleCooldown = .2f;
	private float mantleTimer = .2f;
	public float jumpHeight = 0.0f; //player's current jump height position
	private bool neutralJump = false;
	private bool cancelVelocity = true;
	[Export] Godot.AudioStreamPlayer jumpySound;

	private bool isTouching() {
		if (player.GetSlideCollisionCount() != 0) {
			return true;
		}
		return false;
	}
	private bool isSameHeight()
	{
		if (player.GetSlideCollisionCount() == 0) {
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
			collider.Transform.Basis.Z.Y * boxShape.Size.Y).Length(); //local y scale derived from scale and rotation matrix
			float playerHeight = player.GlobalPosition.Y + playerTop;
			float meshTop = collider.GlobalPosition.Y + scaleY / 2;
			if (Mathf.Abs(playerHeight - meshTop) < 0.1f) return true;
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
		float playerHeight = player.GlobalPosition.Y + playerTop;
		float meshTop = collider.GlobalPosition.Y;


		if (Mathf.Abs(playerHeight - meshTop) > 0.1f)
			return true;
		else return false;
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
			jumpHeight = jumpMaxHeight; //clamp
			return false;
		}
	}

	public override void Enter()
	{
		//GD.Print("Entered Jump State. Jump queued: " + player.Get("jumpQueued"));
		cancelVelocity = true;
		if ((bool)player.Get("jumpQueued"))//jump state entered due to player jumping
		{
			if (pm.jumpVFX != null && msm._previousState.Name != "mantleState") pm.SpawnJumpCloud(0);
			jumpHeight = 0.0f;
			jumpySound = GetNode<Godot.AudioStreamPlayer>("%jumpSound");
			jumpySound.Play();
			if (Input.GetAxis("Left", "Right") == 0) cancelVelocity = true; //freeze horizontal velocity for neutral jump
		}
		player.Set(PlayerManager.PropertyName.slideBoost, true); //player must be airborne, enable boost
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).BeginJump();
	}

	public override void Exit()
	{
		//GD.Print("Exited Jump State");
		player.Set("jumpQueued", false); //don't jump on exit if holding jump
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
	}


	public override void PhysicsUpdate(float delta)
	{
		if (cancelVelocity)
		{
			player.Velocity = Vector3.Zero;
			cancelVelocity = false;
		}
		if (mantleTimer > 0) mantleTimer -= delta; //player can mantle again when mantleTimer = 0

		/*if (Input.IsKeyPressed(Key.Down)) {
				velocity.Y -= (jumpGravity * 4) * (float)delta;
			}
		*/

		HandleAirMovement(delta);
		player.MoveAndSlide();
	}

	private void HandleAirMovement(float delta)
	{
		Vector3 velocity = player.Velocity;
		float input = Input.GetAxis("Left", "Right");

		if ((bool)player.Get("jumpQueued") && IsAscending(delta, ref velocity))
		{ //jump queued set true outside this state. if the player releases jump, the bool is set false 
			velocity.Y -= _gravity * delta;
			if (input == 0)
			{
				velocity.X = Mathf.MoveToward(velocity.X, 0, jumpDeceleration);
				/*
				if (velocity.X > 0.3f) velocity.X -= 1.0f;
				else if (velocity.X < -0.3f) velocity.X += 1.0f;
				else { velocity.X = 0; }*/
			}
			else velocity.X = input * airMaxSpeed;
		}
		else //must be falling
		{
			velocity.Y -= _gravity * 2.5f * delta; //faster falling speed
			if (input == 0)
			{
				velocity.X = Mathf.MoveToward(velocity.X, 0, jumpDeceleration);
				/*
				if (velocity.X > 0.1f) velocity.X = 0.5f;
				else if (velocity.X < -0.1f) velocity.X = -0.5f;
				*/
			}
			else velocity.X = input * airMaxSpeed;

			if (player.IsOnFloor())
			{
				mantleTimer = 0;
				if (Input.IsActionPressed("Slide")) msm.TransitionTo("slideState");
				else msm.TransitionTo("groundedState");
			}
			else if (mantleTimer <= 0 && Input.GetAxis("Left", "Right") != 0 && isSameHeight() && GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("mantling")) //player must be pressing towards ledge, player top reset, and in range of ledge height
			{

				//GD.Print("Mantling");
				mantleTimer = mantleCooldown;
				cancelVelocity = true;
				msm.TransitionTo("mantleState");
				return; //dont continue updating movement
			}
		}

		velocity.X = Mathf.Clamp(velocity.X, -airMaxSpeed, airMaxSpeed);//clamp horizontal speed
		player.Velocity = velocity;
	}

	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionReleased("Jump")) //check when jump is released
		{
			player.Set(PlayerManager.PropertyName.jumpQueued, false);
		}
		if (@event.IsActionPressed("Jump") && isTouching() && !isSameHeight() && GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("wallJump")) {
			msm.TransitionTo("walljumpState");
		}

		if (@event.IsActionPressed("Shoot") || @event.IsActionPressed("SpecialShoot"))
		{
			asm.TransitionTo("attackState");
		}


	}	
	
}
