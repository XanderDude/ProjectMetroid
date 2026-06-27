using Godot;
using System;

public partial class jumpState : State
{
	[ExportSubgroup("Jump State")]
	[Export] public float airMaxSpeed = 5.0f;
	[Export] public float jumpAcceleration = 10.0f;
	[Export] public float jumpDeceleration = 15f;
	[Export] public float jumpVelocity = 10.0f;
	[Export] public float jumpMaxHeight = 2f, jumpMinHeight = 0.2f; //meters
	[Export] public float playerTopWhileInJump = 1.322f;
	private float _mantleCooldown = .2f;
	private float mantleTimer = 1;
	public float jumpHeight = 0.0f; //player's current jump height position
	private float startPosition = 0.0f;
	private float travelTime = 0.0f;
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
			CollisionShape3D colShapeNode;
			try
			{
				var colObj = (CollisionObject3D)collision.GetCollider();
				if (!colObj.GetCollisionLayerValue(1)) continue;
				colShapeNode = (CollisionShape3D)collision.GetColliderShape();
				boxShape = (BoxShape3D)colShapeNode.Shape;
			}
			catch { continue; }

			var scaleY = new Vector3(colShapeNode.Transform.Basis.X.Y * boxShape.Size.Y,
			colShapeNode.Transform.Basis.Y.Y * boxShape.Size.Y,
			colShapeNode.Transform.Basis.Z.Y * boxShape.Size.Y).Length(); 
			float playerHeight = player.GlobalPosition.Y + playerTopWhileInJump + 0.5f;
			float meshTopTemp = colShapeNode.GlobalPosition.Y + scaleY / 2;
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
			jumpHeight = player.GlobalPosition.Y - startPosition;
			//GD.Print("Jump Height: " + jumpHeight);
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
    }

	public override void Enter()
	{
		//cancelVelocity = true;
		if (pm.jumpQueued)//jump state entered due to player jumping
		{
			if (pm.jumpVFX != null && msm._previousState.Name != "mantleState") pm.SpawnJumpCloud(0,0);
			jumpHeight = 0.0f;
			startPosition = player.GlobalPosition.Y;
			SoundFriend.Play("player_jump_SFX");
			if (pm.aimDirection.X == 0) cancelVelocity = true; //freeze horizontal velocity for neutral jump
		}
		pm.slideBoost = true; //player must be airborne, enable boost
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Airborne(pm.jumpQueued);
	}

	public override void Exit()
	{
		//GD.Print("Exited Jump State");
		pm.jumpQueued = false; //don't jump on exit if holding jump
	}

	public override void PhysicsUpdate(float delta)
	{
		mantleTimer += delta;

		//1. Check player input for canceling jump early
		if (pm.jumpQueued && !Input.IsActionPressed("Jump") && jumpHeight > jumpMinHeight) //check when jump is released
		{
			CancelUpwardVelocity();
			pm.jumpQueued = false;
		}

		//2. Check if player height (GlobalPosition) has increased after previous movement calculation (jumpHeight), no change means jump blocked
		if (pm.jumpQueued && jumpHeight != 0 && Mathf.Floor(jumpHeight * 1000) == Mathf.Floor((player.GlobalPosition.Y - startPosition)* 1000)) CancelUpwardVelocity(); //position rounded to two decimal places
		
		HandleAirMovement(delta);

		//3. Perform collision checks prior to move and slide, but after HandleAirMovement()
		if (!pm.jumpQueued && player.IsOnFloor())
        {
			player.Velocity = new Vector3(player.Velocity.X, 0, 0); //keep horizontal, reset vertical
			var ground = GetFloorYPosition();
			player.GlobalPosition = new Vector3(player.GlobalPosition.X, ground, player.GlobalPosition.Z);
			//parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).SetGroundedColliders();
            if (Input.IsActionPressed("Slide") && pm.aimDirection.X != 0) 
			{
				parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Sliding(true);
				msm.TransitionTo("slideState");
			}
			else if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding()) 
			{
				parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true);
				msm.TransitionTo("crouchState");
			}
			else 
			{
				parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
				msm.TransitionTo("groundedState");
			}
        }
		else if (GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("mantling") && mantleTimer >= _mantleCooldown && pm.aimDirection.X != 0 && isSameHeight()) //player must be pressing towards ledge, player top reset, and in range of ledge height
		{
			mantleTimer = 0;
			player.Velocity = Vector3.Zero;
			
			msm.TransitionTo("mantleState");
		}

		if (cancelVelocity)
		{
			player.Velocity = Vector3.Zero;
			cancelVelocity = false;
		}

		//4. Update player position
		player.MoveAndSlide();
	}

	private float GetFloorYPosition()
    {
		float floorY = player.GlobalPosition.Y;
		floorY = player.GlobalPosition.Y - pm.groundCheck.GetClosestCollisionSafeFraction();
		for (int i = 0; i < pm.groundCheck.GetCollisionCount(); i++)
		{
			Vector3 point = pm.groundCheck.GetCollisionPoint(i);
			floorY = Mathf.Max(floorY, point.Y);
		}
		return floorY;
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
			pm.groundCheck.ForceShapecastUpdate();
			velocity.Y -= _gravity * 2.5f * delta; //faster falling speed
			if (input == 0)
			{
				velocity.X = Mathf.MoveToward(velocity.X, 0, jumpDeceleration);
			}
			else velocity.X = input * airMaxSpeed;
		}

		if (Input.IsActionPressed("Aim")) velocity.X = player.Velocity.X;
		velocity.X = Mathf.Clamp(velocity.X, -airMaxSpeed, airMaxSpeed);//clamp horizontal speed
		velocity.Y = Mathf.Max(velocity.Y, -_gravity * 2f); //clamp fall speed
		player.Velocity = velocity;
	}

	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("forgemode") && GameFriend.gameinstance.initroomfriend == true)
		{
			msm.TransitionTo("forgeState");
		}

		if (@event.IsActionPressed("Jump") && GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("wallJump") && !player.IsOnFloor() && isTouching() && !isSameHeight()) {
			msm.TransitionTo("walljumpState");
		}

	}	
	
}
