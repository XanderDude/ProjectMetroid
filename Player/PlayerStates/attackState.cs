using Godot;
using System;

public partial class attackState : State
{
	[Export] public PackedScene arrowScene;
	[Export] public PackedScene bombArrowScene;
	[Export] private string shootNormalArrowSFX = "player_normal_shoot_SFX", shootBombArrowSFX = "player_normal_shoot_SFX",
		shootHalfChargeSFX = "player_normal_shoot_SFX", shootFullChargeSFX = "player_normal_shoot_SFX";
	[Export] private Material arrowOutlineMat;
	[Export] public float arrowSpeed = 20.0f;
	[Export] public float shootCooldown = 0.12f;
	private float shootCooldownTimer = 0.0f;
	private Node3D crossbowMesh;
	private Node3D arrowSpawnLoc;
	private float chargingTimer = 0.0f;

	[Export] private float HALF_CHARGE_TIME = 0.4f;
	[Export] private float FULL_CHARGE_TIME = 0.8f;

	private bool half_charge_shot = false;
	private bool full_charge_shot = false;

	

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
		half_charge_shot = false;
		full_charge_shot = false;
		chargingTimer = 0.0f;
		shootCooldownTimer = 0.0f;
		if (Input.IsActionPressed("Shoot"))
		{
			ShootArrow(false);
		}
		else if (Input.IsActionPressed("SpecialShoot") && CanShootBomb())
		{
			ShootArrow(true);
		}
	}
	

	public override void Exit()
	{
		if (full_charge_shot == true)
		{
			GD.Print("Shooting Full Charge Shot");
			ShootFullCharge();
		}
		else if (half_charge_shot == true)
		{
			GD.Print("Shooting Half Charge Shot");
			ShootHalfCharge();
		}
	}

	public override void PhysicsUpdate(float delta)
	{
		chargingTimer += delta;
		shootCooldownTimer += delta;
		if (shootCooldownTimer >= shootCooldown && !Input.IsActionPressed("Shoot")) asm.TransitionTo("noattackState");
		else
		{
			if (chargingTimer >= FULL_CHARGE_TIME) full_charge_shot = true; 
			else if (chargingTimer >= HALF_CHARGE_TIME) half_charge_shot = true;
			
			
		}
	}


	public void ShootHalfCharge()
	{
		
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Shoot();
		PackedScene projectileScene = arrowScene;
		
		if (projectileScene == null || arrowSpawnLoc == null)
		{
			return;
		}
		
		var arrow = projectileScene.Instantiate() as RigidBody3D;
		arrow.GravityScale = 0.0f;
		
		
		player.GetParent().AddChild(arrow);
		arrow.GlobalPosition = arrowSpawnLoc.GlobalPosition;

		Vector2 shootDirection = GetShootDirection();
		
		float dot = shootDirection.Dot(new Vector2(Mathf.Abs(shootDirection.X), 0));
		float rotation = (shootDirection.Y >= 0)
		? (1f - dot) / 4f 
		: (dot + 3f) / 4f;

		rotation *= 360f;

		arrow.RotationDegrees = new(0, 0, rotation);
		arrow.LinearVelocity = new(shootDirection.X * arrowSpeed * 0.03f, shootDirection.Y * arrowSpeed * 0.03f, 0);
		
		SoundFriend.Play("player_death_SFX");
		

	
	}



	public void ShootFullCharge()
	{
		
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Shoot();
		PackedScene projectileScene = arrowScene;
		
		if (projectileScene == null || arrowSpawnLoc == null)
		{
			return;
		}
		
		var arrow = projectileScene.Instantiate() as RigidBody3D;
		arrow.GravityScale = 0.0f;
		
		
		player.GetParent().AddChild(arrow);
		arrow.GlobalPosition = arrowSpawnLoc.GlobalPosition;

		Vector2 shootDirection = GetShootDirection();
		
		float dot = shootDirection.Dot(new Vector2(Mathf.Abs(shootDirection.X), 0));
		float rotation = (shootDirection.Y >= 0)
		? (1f - dot) / 4f 
		: (dot + 3f) / 4f;

		rotation *= 360f;

		arrow.RotationDegrees = new(0, 0, rotation);
		arrow.LinearVelocity = new(shootDirection.X * arrowSpeed * 5.0f, shootDirection.Y * arrowSpeed * 5.0f, 0);
		
		SoundFriend.Play("raven_launch_SFX");	
	}

	private bool CanShootBomb()
	{
		if (GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("bombArrows") && 
			GameFriend.gameinstance.inventoryfriend.consumables.ContainsKey("bombArrows") &&
			GameFriend.gameinstance.inventoryfriend.consumables["bombArrows"] > 0)
		{
			return true;
		}

		if (Input.IsActionPressed("Shoot")) return false;
		return false;
	}	
	
	private Vector2 GetShootDirection()
	{		
		Vector2 direction = new Vector2(Input.GetAxis("Left", "Right"), Input.GetAxis("Down", "Up"));

		if (direction == Vector2.Zero || player.IsOnFloor() && direction == new Vector2(0, -1))
		{
			direction = new(Math.Sign(parentMesh.RotationDegrees.Y), direction.Y);
		}
		
		return direction.Normalized();
	}
	
	private void ShootArrow(bool isBombArrow)
	{
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Shoot();
		PackedScene projectileScene = isBombArrow ? bombArrowScene : arrowScene;
		
		if (projectileScene == null || arrowSpawnLoc == null)
		{
			return;
		}
		
		var arrow = projectileScene.Instantiate() as RigidBody3D;
		arrow.GravityScale = 0.0f;
		if (isBombArrow)
		{
			arrow.GravityScale = 0.5f;
		}
		
		player.GetParent().AddChild(arrow);
		arrow.GlobalPosition = arrowSpawnLoc.GlobalPosition;

		Vector2 shootDirection = GetShootDirection();
		
		float dot = shootDirection.Dot(new Vector2(Mathf.Abs(shootDirection.X), 0));
		float rotation = (shootDirection.Y >= 0)
		? (1f - dot) / 4f 
		: (dot + 3f) / 4f;

		rotation *= 360f;

		arrow.RotationDegrees = new(0, 0, rotation);
		arrow.LinearVelocity = new(shootDirection.X * arrowSpeed, shootDirection.Y * arrowSpeed, 0);
		
		SoundFriend.Play(shootNormalArrowSFX);

		// Only consume after arrow is successfully created
		if (isBombArrow)
		{
			GameFriend.gameinstance.inventoryfriend.UseConsumable("bombArrows", 1);
		}

	}
}
