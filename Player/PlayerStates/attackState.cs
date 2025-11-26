using Godot;
using System;

public partial class attackState : State
{
	[Export] public PackedScene arrowScene;
	[Export] public PackedScene bombArrowScene;
	[Export] public float arrowSpeed = 20.0f;
	[Export] public float shootCooldown = 0.12f;
	private float shootCooldownTimer = 0.0f;
	private Godot.AudioStreamPlayer shootSoundNormal, shootSoundBomb;
	private Node3D crossbowMesh;
	private Node3D arrowSpawnLoc;

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
		shootSoundNormal = GetNode<Godot.AudioStreamPlayer>("../../shootingsound");
	}
	
	public override void Enter()
	{
		shootCooldownTimer = 0.0f;
		if (Input.IsActionPressed("Shoot") && CanShoot())
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
	}

	public override void PhysicsUpdate(float delta)
	{
		shootCooldownTimer += delta;
		if (shootCooldownTimer >= shootCooldown) asm.TransitionTo("noattackState");
	}


	
	public override void HandleInput(InputEvent @event)
	{
		/*
		if (@event.IsActionReleased("Shoot") && !Input.IsActionPressed("SpecialShoot"))
		{
			asm.TransitionTo("noattackState");
		}

		if (@event.IsActionReleased("SpecialShoot") && !Input.IsActionPressed("Shoot"))
		{
			asm.TransitionTo("noattackState");
		}*/
	}
	
	private bool CanShoot()
	{
		return true;

		if (Input.IsActionPressed("SpecialShoot")) return false;
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
		
		shootSoundNormal?.Play();

		// Only consume after arrow is successfully created
		if (isBombArrow)
		{
			GameFriend.gameinstance.inventoryfriend.UseConsumable("bombArrows", 1);
		}
	}
}
