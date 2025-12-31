using Godot;
using System;

public partial class attackState : State
{
	[Export] public PackedScene arrowScene, chargedArrowScene;
	[Export] public PackedScene bombArrowScene;
	[Export] private string shootNormalArrowSFX = "player_normal_shoot_SFX", shootBombArrowSFX = "player_normal_shoot_SFX", 
		chargingArrowStartSFX = "player_chargingArrowStart_SFX", chargingArrowLoopSFX = "player_chargingArrowLoop_SFX", 
		shootHalfChargeSFX = "player_normal_shoot_SFX", shootFullChargeSFX = "player_normal_shoot_SFX";

	[Export] private Node3D chargingArrowVFX, fullChargedArrowVFX;
	[Export] private Material arrowOutlineMat;
	[Export] public float arrowSpeed = 20.0f;
	[Export] public float shootCooldown = 0.12f;
	private float shootCooldownTimer = 0.0f;
	private Node3D crossbowMesh;
	private Node3D arrowSpawnLoc;
	private float chargingTimer = 0.0f;

	//[Export] private float HALF_CHARGE_TIME = 0.4f;
	[Export] private float FULL_CHARGE_TIME = 0.8f;

	private enum ArrowType
	{
		None,
		Normal,
		Bomb,
		HalfCharge,
		FullCharge
	}
	private ArrowType currentArrowType = ArrowType.None;

	public override void _Ready()
	{
		Node current = this;
		while (current != null && !(current is PlayerManager))
		{
			current = current.GetParent();
		}
		if (current is PlayerManager)
		{
			pm = current as PlayerManager;
		}
		if (pm != null)
		{
			crossbowMesh = pm.GetNode<Node3D>("PlayerMesh/Skeleton3D/Crossbow");
			arrowSpawnLoc = (Node3D)crossbowMesh.GetChild(0);
		}
	}
	
	public override void Enter()
	{
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Aiming(true);
		chargingTimer = 0.0f;
		shootCooldownTimer = 0.0f;
		if (Input.IsActionPressed("Shoot"))
		{
			ShootArrow(ArrowType.Normal);
			if (GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("chargeShot"))
			{
				SoundFriend.Play(chargingArrowStartSFX);
			}
		}
		else if (Input.IsActionPressed("SpecialShoot") && CanShootBomb())
		{
			currentArrowType = ArrowType.Bomb;
			ShootArrow(ArrowType.Bomb);
		}
	}
	

	public override void Exit()
	{
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Aiming(false);
		chargingArrowVFX.Visible = false;
		fullChargedArrowVFX.Visible = false;
		SoundFriend.Stop(chargingArrowStartSFX);
		SoundFriend.Stop(chargingArrowLoopSFX);
		if (currentArrowType == ArrowType.FullCharge && pm.StateMachine._currentState.Name != "mantleState")
		{
			ShootArrow(ArrowType.FullCharge);
		}
		currentArrowType = ArrowType.None;
	}

	public override void PhysicsUpdate(float delta)
	{
		shootCooldownTimer += delta;
		if (!GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("chargeShot") || currentArrowType == ArrowType.Bomb)
        {
			if (shootCooldownTimer >= shootCooldown) asm.TransitionTo("noattackState");
            return;
        }

		chargingTimer += delta;
		if (shootCooldownTimer >= shootCooldown && !Input.IsActionPressed("Shoot")) asm.TransitionTo("noattackState");
		else
		{
			if (chargingTimer > 0.4f) chargingArrowVFX.Visible = true;
			
			if (chargingTimer >= FULL_CHARGE_TIME && currentArrowType != ArrowType.FullCharge) 
            {
				currentArrowType = ArrowType.FullCharge;
				chargingArrowVFX.Visible = false;
				fullChargedArrowVFX.Visible = true;
				SoundFriend.Play(chargingArrowLoopSFX);
            }
		}
	}

	private bool CanShootBomb()
	{
		if (GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("bombArrows") && 
			GameFriend.gameinstance.inventoryfriend.consumables.ContainsKey("bombArrows") &&
			GameFriend.gameinstance.inventoryfriend.consumables["bombArrows"] > 0)
		{
			return true;
		}
		return false;
	}	
	
	public Vector2 GetShootDirection()
	{		
		if (Input.IsActionPressed("Aim"))
        {
            return new Vector2(Math.Sign(parentMesh.RotationDegrees.Y), 1f).Normalized();
        }
		Vector2 direction = pm.aimDirection;

		if (direction == Vector2.Zero || player.IsOnFloor() && direction == new Vector2(0, -1))
		{
			direction = new(Math.Sign(parentMesh.RotationDegrees.Y), direction.Y);
		}
		
		return direction.Normalized();
	}
	
	private void ShootArrow(ArrowType arrowType)
	{
		PackedScene projectileScene; RigidBody3D arrow;
		float speedModifier = 1f;
		switch (arrowType)
		{
			case ArrowType.Normal:
				projectileScene = arrowScene;
				arrow = projectileScene.Instantiate() as RigidBody3D;
				arrow.GravityScale = 0.0f;
				break;
			case ArrowType.Bomb:
				projectileScene = bombArrowScene;
				arrow = projectileScene.Instantiate() as RigidBody3D;
				arrow.GravityScale = 0.5f;
				speedModifier = 0.8f;
				break;
			case ArrowType.HalfCharge:
				projectileScene = arrowScene;
				arrow = projectileScene.Instantiate() as RigidBody3D;
				arrow.GravityScale = 0.0f;
				break;
			case ArrowType.FullCharge:
				projectileScene = chargedArrowScene;
				arrow = projectileScene.Instantiate() as RigidBody3D;
				arrow.GravityScale = 0.0f;
				speedModifier = 1.5f;
				break;
			default:
				GD.PrintErr("No arrow type selected!");
				return;
		}
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Shooting();

		if (projectileScene == null || arrowSpawnLoc == null)
		{
			return;
		}
		
		player.GetParent().AddChild(arrow);
		arrow.GlobalPosition = new(arrowSpawnLoc.GlobalPosition.X, arrowSpawnLoc.GlobalPosition.Y, 0);

		Vector2 shootDirection = GetShootDirection();
		
		float dot = shootDirection.Dot(new Vector2(Mathf.Abs(shootDirection.X), 0));
		float rotation = (shootDirection.Y >= 0)
		? (1f - dot) / 4f 
		: (dot + 3f) / 4f;

		rotation *= 360f;

		arrow.RotationDegrees = new(0, 0, rotation);
		arrow.LinearVelocity = new(shootDirection.X * arrowSpeed * speedModifier, 
		shootDirection.Y * arrowSpeed * speedModifier, 0);
		
		switch (arrowType)
		{
			case ArrowType.Normal:
				SoundFriend.Play(shootNormalArrowSFX);
				break;
			case ArrowType.Bomb:
				SoundFriend.Play(shootBombArrowSFX);
				GameFriend.gameinstance.inventoryfriend.UseConsumable("bombArrows", 1);
				break;
			case ArrowType.HalfCharge:
				SoundFriend.Play(shootHalfChargeSFX);
				break;
			case ArrowType.FullCharge:
				SoundFriend.Play(shootFullChargeSFX);
				break;
		}
	}
}
