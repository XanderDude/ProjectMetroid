using Godot;
using System;

public partial class jumpState : PlayerState
{
	[ExportSubgroup("Jump State")]

	[Export]public float gravity = 9.8f;
	[Export] public float airMaxSpeed = 5.0f;
	[Export] public float jumpAcceleration = 10.0f;
	[Export] public float jumpDeceleration = 15f;
	[Export] public float jumpVelocity = 10.0f;
	[Export] public float jumpMaxHeight = 2f, jumpMinHeight = 0.2f; //meters
	[Export] public float pmTopWhileInJump = 1.322f;
	private float _mantleCooldown = .2f;
	private float mantleTimer = 1;
	public float jumpHeight = 0.0f; //pm's current jump height position
	private float startPosition = 0.0f;
	private float travelTime = 0.0f;
	private bool neutralJump = false;
	private bool cancelVelocity = true;
	public float meshTop = 0.0f;
	private const float pm_AND_MESH_OFFSET = 0.2f; //to account for difference in pm origin and mesh origin

	private bool isTouching() {
		if (pm.GetSlideCollisionCount() != 0) {
			return true;
		}
		return false;
	}
	
	private bool isSameHeight()
	{
		if (!isTouching() && !isGreaterHeight()) { 
			return false;
		}

		for (int i = 0; i < pm.GetSlideCollisionCount(); i++) //must be at least 1 collision
		{
			KinematicCollision3D collision = pm.GetSlideCollision(i);
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
			float pmHeight = pm.GlobalPosition.Y + pmTopWhileInJump + 0.5f;
			float meshTopTemp = collider.GlobalPosition.Y + scaleY / 2;
			meshTop = meshTopTemp;
			if (Mathf.Abs(pmHeight - meshTop) < pm_AND_MESH_OFFSET && pmHeight >= meshTop) {
				return true;
			}
		}
		return false;

	}

	private bool isGreaterHeight()
	{
		if (pm.GetSlideCollisionCount() == 0) {
			return false;
		}
		KinematicCollision3D collision = pm.GetSlideCollision(0);
		Node3D collider = collision.GetCollider() as Node3D;
		float pmHeight = pm.GlobalPosition.Y + pmTopWhileInJump;
		float meshTop = collider.GlobalPosition.Y;


		if (Mathf.Abs(pmHeight - meshTop) > 0.1f)
			return true;
		
		return false;
	}

	private bool IsAscending(float delta, ref Vector3 velocity) //check if pm should be ascending
	{
		if (jumpHeight < jumpMaxHeight)
		{
			velocity.Y = jumpVelocity; //continously set upward velocity
			jumpHeight = pm.GlobalPosition.Y - startPosition;
			//GD.Print("Jump Height: " + jumpHeight);
			return true;
		}
		else
		{
			velocity.Y = pm.Velocity.Y/2;
			pm.jumpQueued = false;
			jumpHeight = jumpMaxHeight; //clamp
			return false;
		}
	}

	private void CancelUpwardVelocity()
    {
		pm.jumpQueued = false;
        pm.Velocity = new Vector3(pm.Velocity.X, pm.Velocity.Y/2, pm.Velocity.Z);
    }

	public override void Enter()
	{
		GD.Print("Entered Jump State");
		//cancelVelocity = true;
		if (pm.jumpQueued)//jump state entered due to pm jumping
		{
			if (pm.jumpVFX != null && psm.previous_node_state_name != "mantleState") pm.SpawnJumpCloud(0,0);
			jumpHeight = 0.0f;
			startPosition = pm.GlobalPosition.Y;
			SoundFriend.Play("player_jump_SFX");
			if (pm.aimDirection.X == 0) cancelVelocity = true; //freeze horizontal velocity for neutral jump
		}
		pm.slideBoost = true; //pm must be airborne, enable boost
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

		//1. Check pm input for canceling jump early
		if (pm.jumpQueued && !Input.IsActionPressed("Jump") && jumpHeight > jumpMinHeight) //check when jump is released
		{
			CancelUpwardVelocity();
			pm.jumpQueued = false;
		}

		//2. Check if pm height (GlobalPosition) has increased after previous movement calculation (jumpHeight), no change means jump blocked
		if (pm.jumpQueued && jumpHeight != 0 && Mathf.Floor(jumpHeight * 1000) == Mathf.Floor((pm.GlobalPosition.Y - startPosition)* 1000)) CancelUpwardVelocity(); //position rounded to two decimal places
		
		HandleAirMovement(delta);

		//3. Perform collision checks prior to move and slide, but after HandleAirMovement()
		if (!pm.jumpQueued && pm.IsOnFloor())
        {
			pm.Velocity = new Vector3(pm.Velocity.X, 0, 0); //keep horizontal, reset vertical
			var ground = GetFloorYPosition();
			pm.GlobalPosition = new Vector3(pm.GlobalPosition.X, ground, pm.GlobalPosition.Z);
			//parentMesh.GetNode<pmAnimationHandler>(parentMesh.GetPath()).SetGroundedColliders();
            if (Input.IsActionPressed("Slide") && pm.aimDirection.X != 0) 
			{
				parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Sliding(true);
				EmitSignal(SignalName.Transition, "slideState");
				return;
			}
			else if (pm.vertColCheck != null && pm.vertColCheck.VertCheckIsColliding()) 
			{
				parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Crouch(true);
				EmitSignal(SignalName.Transition, "crouchState");
				return;
			}
			else 
			{
				parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Grounded();
				EmitSignal(SignalName.Transition, "groundedState");
			}
        }
		else if (GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("mantling") && mantleTimer >= _mantleCooldown && pm.aimDirection.X != 0 && isSameHeight()) //pm must be pressing towards ledge, pm top reset, and in range of ledge height
		{
			mantleTimer = 0;
			pm.Velocity = Vector3.Zero;
			
			EmitSignal(SignalName.Transition, "mantleState");
		}

		if (cancelVelocity)
		{
			pm.Velocity = Vector3.Zero;
			cancelVelocity = false;
		}

		//4. Update pm position
		pm.MoveAndSlide();
	}

	private float GetFloorYPosition()
    {
		float floorY = pm.GlobalPosition.Y;
		floorY = pm.GlobalPosition.Y - pm.groundCheck.GetClosestCollisionSafeFraction();
		for (int i = 0; i < pm.groundCheck.GetCollisionCount(); i++)
		{
			Vector3 point = pm.groundCheck.GetCollisionPoint(i);
			floorY = Mathf.Max(floorY, point.Y);
		}
		return floorY;
    }

	private void HandleAirMovement(float delta)
	{
		Vector3 velocity = pm.Velocity;
		float input = Mathf.Sign(pm.aimDirection.X);

		if (pm.jumpQueued && IsAscending(delta, ref velocity))
		{ //jump queued set true outside this state. if the pm releases jump, the bool is set false 
			velocity.Y -= gravity * delta;
			if (input == 0)
			{
				velocity.X = Mathf.MoveToward(velocity.X, 0, jumpDeceleration);
			}
			else velocity.X = input * airMaxSpeed;
		}
		else //must be falling
		{
			pm.groundCheck.ForceShapecastUpdate();
			velocity.Y -= gravity * 2.5f * delta; //faster falling speed
			if (input == 0)
			{
				velocity.X = Mathf.MoveToward(velocity.X, 0, jumpDeceleration);
			}
			else velocity.X = input * airMaxSpeed;
		}

		if (Input.IsActionPressed("Aim")) velocity.X = pm.Velocity.X;
		velocity.X = Mathf.Clamp(velocity.X, -airMaxSpeed, airMaxSpeed);//clamp horizontal speed
		velocity.Y = Mathf.Max(velocity.Y, -gravity * 2f); //clamp fall speed
		pm.Velocity = velocity;
	}

	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsActionPressed("forgemode") && GameFriend.gameinstance.initroomfriend == true)
		{
			EmitSignal(SignalName.Transition, "forgeState");
		}

		if (@event.IsActionPressed("Jump") && GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("wallJump") && !pm.IsOnFloor() && isTouching() && !isSameHeight()) {
			EmitSignal(SignalName.Transition, "walljumpState");
		}

	}	
	
}
